using SQLite;
using IncidentReportApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IncidentReportApp.Services
{
    /// <summary>
    /// Servicio para manejar todas las operaciones de base de datos SQLite
    /// </summary>
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            // Ruta donde se guardará la base de datos
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "IncidentReport.db3");
        }

        /// <summary>
        /// Inicializa la base de datos y crea las tablas si no existen
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            if (_database != null)
                return;

            try
            {
                _database = new SQLiteAsyncConnection(_dbPath);

                // Crear tablas
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Incident>();

                // Crear usuario de prueba si no existe
                await CreateDefaultUserAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Base de datos inicializada en: {_dbPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al inicializar BD: {ex.Message}");
                throw;
            }
        }

        #region User Operations (CRUD)

        /// <summary>
        /// Obtiene un usuario por username
        /// </summary>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _database.Table<User>()
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener usuario: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public async Task<int> SaveUserAsync(User user)
        {
            try
            {
                if (user.Id == 0)
                {
                    // Insertar nuevo usuario
                    return await _database.InsertAsync(user);
                }
                else
                {
                    // Actualizar usuario existente
                    return await _database.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar usuario: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        public async Task<User> ValidateUserAsync(string username, string password)
        {
            try
            {
                return await _database.Table<User>()
                    .Where(u => u.Username == username && u.Password == password)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al validar usuario: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Crea un usuario por defecto para pruebas
        /// </summary>
        private async Task CreateDefaultUserAsync()
        {
            try
            {
                var existingUser = await GetUserByUsernameAsync("user");
                if (existingUser == null)
                {
                    var defaultUser = new User
                    {
                        Username = "user",
                        Password = "user",
                        FullName = "Usuario comun",
                        Email = "usuario@incidentapp.com",
                        CreatedAt = DateTime.Now
                    };

                    await SaveUserAsync(defaultUser);
                    System.Diagnostics.Debug.WriteLine("✅ Usuario por defecto creado: user / user");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al crear usuario por defecto: {ex.Message}");
            }
        }

        #endregion

        #region Incident Operations (CRUD)

        /// <summary>
        /// Obtiene todas las incidencias
        /// </summary>
        public async Task<List<Incident>> GetAllIncidentsAsync()
        {
            try
            {
                return await _database.Table<Incident>()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener incidencias: {ex.Message}");
                return new List<Incident>();
            }
        }

        /// <summary>
        /// Obtiene las incidencias de un usuario específico
        /// </summary>
        public async Task<List<Incident>> GetIncidentsByUserAsync(int userId)
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener incidencias del usuario: {ex.Message}");
                return new List<Incident>();
            }
        }

        /// <summary>
        /// Obtiene una incidencia por ID
        /// </summary>
        public async Task<Incident> GetIncidentByIdAsync(int id)
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener incidencia: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda o actualiza una incidencia
        /// </summary>
        public async Task<int> SaveIncidentAsync(Incident incident)
        {
            try
            {
                if (incident.Id == 0)
                {
                    // Insertar nueva incidencia
                    incident.CreatedAt = DateTime.Now;
                    return await _database.InsertAsync(incident);
                }
                else
                {
                    // Actualizar incidencia existente
                    incident.UpdatedAt = DateTime.Now;
                    return await _database.UpdateAsync(incident);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar incidencia: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Elimina una incidencia
        /// </summary>
        public async Task<int> DeleteIncidentAsync(int id)
        {
            try
            {
                var incident = await GetIncidentByIdAsync(id);
                if (incident != null)
                {
                    return await _database.DeleteAsync(incident);
                }
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar incidencia: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Busca incidencias por título o descripción
        /// </summary>
        public async Task<List<Incident>> SearchIncidentsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return await GetAllIncidentsAsync();

                query = query.ToLower();

                return await _database.Table<Incident>()
                    .Where(i => i.Title.ToLower().Contains(query) ||
                               i.Description.ToLower().Contains(query))
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar incidencias: {ex.Message}");
                return new List<Incident>();
            }
        }

        /// <summary>
        /// Obtiene incidencias filtradas por estado
        /// </summary>
        public async Task<List<Incident>> GetIncidentsByStatusAsync(string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status) || status == "Todos")
                    return await GetAllIncidentsAsync();

                return await _database.Table<Incident>()
                    .Where(i => i.Status == status)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al filtrar por estado: {ex.Message}");
                return new List<Incident>();
            }
        }

        /// <summary>
        /// Obtiene incidencias no sincronizadas con la API
        /// </summary>
        public async Task<List<Incident>> GetUnsyncedIncidentsAsync()
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => !i.ApiSynced)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener incidencias sin sincronizar: {ex.Message}");
                return new List<Incident>();
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Obtiene el total de incidencias
        /// </summary>
        public async Task<int> GetTotalIncidentsCountAsync()
        {
            try
            {
                return await _database.Table<Incident>().CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al contar incidencias: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene incidencias pendientes
        /// </summary>
        public async Task<int> GetPendingIncidentsCountAsync()
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => i.Status == "Pendiente")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene incidencias resueltas
        /// </summary>
        public async Task<int> GetResolvedIncidentsCountAsync()
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => i.Status == "Resuelto")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene incidencias sincronizadas
        /// </summary>
        public async Task<int> GetSyncedIncidentsCountAsync()
        {
            try
            {
                return await _database.Table<Incident>()
                    .Where(i => i.ApiSynced)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                return 0;
            }
        }

        #endregion
    }
}