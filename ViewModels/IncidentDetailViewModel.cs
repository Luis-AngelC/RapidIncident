using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IncidentReportApp.Models;
using IncidentReportApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IncidentReportApp.ViewModels
{
    /// <summary>
    /// ViewModel para ver y editar el detalle de una incidencia
    /// </summary>
    [QueryProperty(nameof(Incident), "Incident")]
    public partial class IncidentDetailViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly ApiService _apiService;
        private readonly CameraService _cameraService;

        public IncidentDetailViewModel(
            DatabaseService databaseService,
            ApiService apiService,
            CameraService cameraService)
        {
            _databaseService = databaseService;
            _apiService = apiService;
            _cameraService = cameraService;

            // Inicializar listas
            StatusOptions = new List<string>
            {
                "Pendiente",
                "En Proceso",
                "Resuelto"
            };
        }

        [ObservableProperty]
        private Incident incident = new();

        [ObservableProperty]
        private List<string> statusOptions = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isEditing = false;

        [ObservableProperty]
        private string pageTitle = "Detalle de Incidencia";

        /// <summary>
        /// Se ejecuta cuando se recibe la incidencia como parámetro
        /// </summary>
        partial void OnIncidentChanged(Incident value)
        {
            if (value != null)
            {
                PageTitle = $"Incidencia #{value.Id}";
                System.Diagnostics.Debug.WriteLine($"✅ Cargado detalle de incidencia: {value.Title}");
            }
        }

        /// <summary>
        /// Activa o desactiva el modo de edición
        /// </summary>
        [RelayCommand]
        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
        }

        /// <summary>
        /// Guarda los cambios realizados
        /// </summary>
        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;

                // Actualizar fecha de modificación
                Incident.UpdatedAt = DateTime.Now;

                // Guardar en base de datos
                var result = await _databaseService.SaveIncidentAsync(Incident);

                if (result > 0)
                {
                    await Shell.Current.DisplayAlert("✅ Éxito",
                        "Cambios guardados correctamente", "OK");

                    IsEditing = false;

                    // Intentar sincronizar con API si ya estaba sincronizada
                    if (Incident.ApiSynced && Incident.ApiId.HasValue)
                    {
                        var hasConnection = await _apiService.CheckConnectionAsync();
                        if (hasConnection)
                        {
                            await _apiService.UpdateIncidentAsync(Incident);
                        }
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("❌ Error",
                        "No se pudieron guardar los cambios", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cancela la edición y restaura los valores originales
        /// </summary>
        [RelayCommand]
        private async Task CancelEditAsync()
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Cancelar",
                "¿Descartar los cambios realizados?",
                "Sí",
                "No");

            if (confirm)
            {
                // Recargar la incidencia desde la base de datos
                var original = await _databaseService.GetIncidentByIdAsync(Incident.Id);
                if (original != null)
                {
                    Incident = original;
                }
                IsEditing = false;
            }
        }

        /// <summary>
        /// Elimina la incidencia
        /// </summary>
        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                var confirm = await Shell.Current.DisplayAlert(
                    "Confirmar Eliminación",
                    $"¿Estás seguro de eliminar la incidencia '{Incident.Title}'?\n\nEsta acción no se puede deshacer.",
                    "Sí, eliminar",
                    "Cancelar");

                if (confirm)
                {
                    IsLoading = true;

                    // Eliminar foto si existe
                    if (!string.IsNullOrEmpty(Incident.PhotoPath))
                    {
                        _cameraService.DeletePhoto(Incident.PhotoPath);
                    }

                    // Eliminar de base de datos
                    var result = await _databaseService.DeleteIncidentAsync(Incident.Id);

                    if (result > 0)
                    {
                        await Shell.Current.DisplayAlert("✅ Éxito",
                            "Incidencia eliminada correctamente", "OK");

                        // Volver atrás
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("❌ Error",
                            "No se pudo eliminar la incidencia", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Sincroniza la incidencia con la API
        /// </summary>
        [RelayCommand]
        private async Task SyncWithApiAsync()
        {
            try
            {
                IsLoading = true;

                var hasConnection = await _apiService.CheckConnectionAsync();
                if (!hasConnection)
                {
                    await Shell.Current.DisplayAlert("Sin conexión",
                        "No hay conexión a internet", "OK");
                    return;
                }

                if (Incident.ApiSynced && Incident.ApiId.HasValue)
                {
                    // Actualizar en API
                    var success = await _apiService.UpdateIncidentAsync(Incident);
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("✅ Éxito",
                            "Incidencia actualizada en la API", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("❌ Error",
                            "No se pudo actualizar en la API", "OK");
                    }
                }
                else
                {
                    // Crear en API
                    var (success, apiId) = await _apiService.PostIncidentAsync(Incident);
                    if (success && apiId.HasValue)
                    {
                        Incident.ApiSynced = true;
                        Incident.ApiId = apiId.Value;
                        await _databaseService.SaveIncidentAsync(Incident);

                        await Shell.Current.DisplayAlert("✅ Éxito",
                            $"Incidencia sincronizada con ID: {apiId.Value}", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("❌ Error",
                            "No se pudo sincronizar con la API", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al sincronizar: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Abre la foto en pantalla completa
        /// </summary>
        [RelayCommand]
        private async Task ViewPhotoAsync()
        {
            if (!string.IsNullOrEmpty(Incident.PhotoPath) && _cameraService.PhotoExists(Incident.PhotoPath))
            {
                // Aquí podrías implementar una vista de imagen en pantalla completa
                // Por ahora solo mostramos un mensaje
                await Shell.Current.DisplayAlert("📷 Foto",
                    $"Ruta: {Incident.PhotoPath}", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("⚠️ Aviso",
                    "No hay foto disponible", "OK");
            }
        }

        /// <summary>
        /// Abre el mapa con la ubicación (si existe)
        /// </summary>
        [RelayCommand]
        private async Task ViewLocationAsync()
        {
            if (Incident.Latitude.HasValue && Incident.Longitude.HasValue)
            {
                try
                {
                    var location = new Location(Incident.Latitude.Value, Incident.Longitude.Value);
                    var options = new MapLaunchOptions { Name = Incident.Title };

                    await Map.Default.OpenAsync(location, options);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error al abrir mapa: {ex.Message}");
                    await Shell.Current.DisplayAlert("❌ Error",
                        "No se pudo abrir el mapa", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("⚠️ Aviso",
                    "Esta incidencia no tiene ubicación GPS", "OK");
            }
        }

        /// <summary>
        /// Comparte la incidencia
        /// </summary>
        [RelayCommand]
        private async Task ShareAsync()
        {
            try
            {
                var text = $"📋 Incidencia: {Incident.Title}\n\n" +
                          $"📝 Descripción: {Incident.Description}\n" +
                          $"📂 Categoría: {Incident.Category}\n" +
                          $"⚠️ Prioridad: {Incident.Priority}\n" +
                          $"📊 Estado: {Incident.Status}\n" +
                          $"📅 Creada: {Incident.CreatedAt:dd/MM/yyyy HH:mm}";

                if (Incident.Latitude.HasValue && Incident.Longitude.HasValue)
                {
                    text += $"\n📍 Ubicación: {Incident.Latitude:F6}, {Incident.Longitude:F6}";
                }

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = text,
                    Title = "Compartir Incidencia"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al compartir: {ex.Message}");
            }
        }
    }
}