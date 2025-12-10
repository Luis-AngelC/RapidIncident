using IncidentReportApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IncidentReportApp.Services
{
    /// <summary>
    /// Servicio para manejar comunicación con API REST externa
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://jsonplaceholder.typicode.com";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Verifica si hay conexión a internet
        /// </summary>
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var current = Connectivity.NetworkAccess;

                if (current != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sin conexión a internet");
                    return false;
                }

                // Intentar hacer ping a la API
                var response = await _httpClient.GetAsync("/posts/1");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al verificar conexión: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene posts de ejemplo desde la API (simula obtener incidencias)
        /// </summary>
        public async Task<List<JsonPlaceholderPost>> GetPostsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/posts");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var posts = JsonConvert.DeserializeObject<List<JsonPlaceholderPost>>(json);

                    System.Diagnostics.Debug.WriteLine($"✅ Se obtuvieron {posts.Count} posts de la API");
                    return posts;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error API: {response.StatusCode}");
                    return new List<JsonPlaceholderPost>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener posts: {ex.Message}");
                return new List<JsonPlaceholderPost>();
            }
        }

        /// <summary>
        /// Envía una incidencia a la API (POST)
        /// </summary>
        public async Task<(bool Success, int? ApiId)> PostIncidentAsync(Incident incident)
        {
            try
            {
                // Verificar conexión primero
                if (!await CheckConnectionAsync())
                {
                    return (false, null);
                }

                // Convertir incidencia a formato de post
                var postData = new
                {
                    userId = incident.UserId,
                    title = incident.Title,
                    body = incident.Description
                };

                var json = JsonConvert.SerializeObject(postData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/posts", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<JsonPlaceholderPost>(responseJson);

                    System.Diagnostics.Debug.WriteLine($"✅ Incidencia enviada a API con ID: {result.id}");
                    return (true, result.id);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error al enviar a API: {response.StatusCode}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al enviar incidencia: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Actualiza una incidencia en la API (PUT)
        /// </summary>
        public async Task<bool> UpdateIncidentAsync(Incident incident)
        {
            try
            {
                if (!await CheckConnectionAsync())
                {
                    return false;
                }

                if (!incident.ApiId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("❌ La incidencia no tiene ApiId");
                    return false;
                }

                var postData = new
                {
                    userId = incident.UserId,
                    title = incident.Title,
                    body = incident.Description,
                    id = incident.ApiId.Value
                };

                var json = JsonConvert.SerializeObject(postData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/posts/{incident.ApiId.Value}", content);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Incidencia actualizada en API: {incident.ApiId.Value}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar incidencia: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sincroniza una incidencia con la API
        /// </summary>
        public async Task<bool> SyncIncidentAsync(Incident incident, DatabaseService database)
        {
            try
            {
                if (incident.ApiSynced)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ La incidencia {incident.Id} ya está sincronizada");
                    return true;
                }

                var (success, apiId) = await PostIncidentAsync(incident);

                if (success && apiId.HasValue)
                {
                    // Actualizar incidencia local con el ID de la API
                    incident.ApiSynced = true;
                    incident.ApiId = apiId.Value;
                    await database.SaveIncidentAsync(incident);

                    System.Diagnostics.Debug.WriteLine($"✅ Incidencia {incident.Id} sincronizada con API ID: {apiId.Value}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al sincronizar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sincroniza todas las incidencias pendientes
        /// </summary>
        public async Task<(int Synced, int Failed)> SyncAllIncidentsAsync(DatabaseService database)
        {
            try
            {
                var unsyncedIncidents = await database.GetUnsyncedIncidentsAsync();
                int synced = 0;
                int failed = 0;

                foreach (var incident in unsyncedIncidents)
                {
                    var success = await SyncIncidentAsync(incident, database);
                    if (success)
                        synced++;
                    else
                        failed++;
                }

                System.Diagnostics.Debug.WriteLine($"📊 Sincronización: {synced} exitosas, {failed} fallidas");
                return (synced, failed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en sincronización masiva: {ex.Message}");
                return (0, 0);
            }
        }
    }
}