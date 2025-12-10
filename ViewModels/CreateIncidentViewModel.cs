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
    /// ViewModel para crear nuevas incidencias
    /// </summary>
    public partial class CreateIncidentViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;
        private readonly CameraService _cameraService;
        private readonly LocationService _locationService;
        private readonly ApiService _apiService;

        public CreateIncidentViewModel(
            AuthService authService,
            DatabaseService databaseService,
            CameraService cameraService,
            LocationService locationService,
            ApiService apiService)
        {
            _authService = authService;
            _databaseService = databaseService;
            _cameraService = cameraService;
            _locationService = locationService;
            _apiService = apiService;

            // Inicializar listas
            Categories = new List<string>
            {
                "Soporte IT",
                "Mantenimiento",
                "Infraestructura",
                "Seguridad",
                "Limpieza",
                "Otro"
            };

            Priorities = new List<string>
            {
                "Baja",
                "Media",
                "Alta",
                "Crítica"
            };

            // Valores por defecto
            SelectedCategory = Categories[0];
            SelectedPriority = Priorities[1]; // Media
        }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private List<string> categories = new();

        [ObservableProperty]
        private string selectedCategory = string.Empty;

        [ObservableProperty]
        private List<string> priorities = new();

        [ObservableProperty]
        private string selectedPriority = string.Empty;

        [ObservableProperty]
        private string photoPath = string.Empty;

        [ObservableProperty]
        private bool hasPhoto = false;

        [ObservableProperty]
        private double? latitude;

        [ObservableProperty]
        private double? longitude;

        [ObservableProperty]
        private string locationText = "📍 Sin ubicación";

        [ObservableProperty]
        private bool hasLocation = false;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isTakingPhoto = false;

        [ObservableProperty]
        private bool isGettingLocation = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        /// <summary>
        /// Comando para tomar foto con la cámara
        /// </summary>
        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            try
            {
                IsTakingPhoto = true;
                ErrorMessage = string.Empty;

                var photo = await _cameraService.TakePhotoAsync();

                if (!string.IsNullOrEmpty(photo))
                {
                    PhotoPath = photo;
                    HasPhoto = true;
                    System.Diagnostics.Debug.WriteLine($"✅ Foto capturada: {photo}");
                }
                else
                {
                    await Shell.Current.DisplayAlert("⚠️ Aviso",
                        "No se pudo capturar la foto. Verifica los permisos de la cámara.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al tomar foto: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error al capturar foto: {ex.Message}", "OK");
            }
            finally
            {
                IsTakingPhoto = false;
            }
        }

        /// <summary>
        /// Comando para seleccionar foto de la galería
        /// </summary>
        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                IsTakingPhoto = true;
                ErrorMessage = string.Empty;

                var photo = await _cameraService.PickPhotoAsync();

                if (!string.IsNullOrEmpty(photo))
                {
                    PhotoPath = photo;
                    HasPhoto = true;
                    System.Diagnostics.Debug.WriteLine($"✅ Foto seleccionada: {photo}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al seleccionar foto: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error al seleccionar foto: {ex.Message}", "OK");
            }
            finally
            {
                IsTakingPhoto = false;
            }
        }

        /// <summary>
        /// Comando para eliminar la foto capturada
        /// </summary>
        [RelayCommand]
        private void RemovePhoto()
        {
            PhotoPath = string.Empty;
            HasPhoto = false;
        }

        /// <summary>
        /// Comando para obtener ubicación GPS
        /// </summary>
        [RelayCommand]
        private async Task GetLocationAsync()
        {
            try
            {
                IsGettingLocation = true;
                ErrorMessage = string.Empty;

                var location = await _locationService.GetCurrentLocationAsync();

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    HasLocation = true;

                    // Obtener nombre del lugar
                    var locationName = await _locationService.GetLocationNameAsync(
                        location.Latitude,
                        location.Longitude);

                    LocationText = $"📍 {locationName}";

                    System.Diagnostics.Debug.WriteLine($"✅ Ubicación obtenida: {Latitude}, {Longitude}");
                }
                else
                {
                    await Shell.Current.DisplayAlert("⚠️ Aviso",
                        "No se pudo obtener la ubicación. Verifica que el GPS esté activado y los permisos estén concedidos.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener ubicación: {ex.Message}");
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error al obtener ubicación: {ex.Message}", "OK");
            }
            finally
            {
                IsGettingLocation = false;
            }
        }

        /// <summary>
        /// Comando para guardar la incidencia
        /// </summary>
        [RelayCommand]
        private async Task SaveIncidentAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Validaciones
                if (string.IsNullOrWhiteSpace(Title))
                {
                    ErrorMessage = "⚠️ El título es obligatorio";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Description))
                {
                    ErrorMessage = "⚠️ La descripción es obligatoria";
                    return;
                }

                if (Title.Length < 5)
                {
                    ErrorMessage = "⚠️ El título debe tener al menos 5 caracteres";
                    return;
                }

                if (Description.Length < 10)
                {
                    ErrorMessage = "⚠️ La descripción debe tener al menos 10 caracteres";
                    return;
                }

                // Crear nueva incidencia
                var incident = new Incident
                {
                    UserId = _authService.GetCurrentUserId(),
                    Title = Title.Trim(),
                    Description = Description.Trim(),
                    Category = SelectedCategory,
                    Priority = SelectedPriority,
                    Status = "Pendiente",
                    PhotoPath = PhotoPath,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    LocationName = HasLocation ? LocationText.Replace("📍 ", "") : null,
                    CreatedAt = DateTime.Now,
                    ApiSynced = false
                };

                // Guardar en base de datos local
                var result = await _databaseService.SaveIncidentAsync(incident);

                if (result > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Incidencia guardada con ID: {result}");

                    // Intentar sincronizar con API si hay conexión
                    var hasConnection = await _apiService.CheckConnectionAsync();
                    if (hasConnection)
                    {
                        var (success, apiId) = await _apiService.PostIncidentAsync(incident);
                        if (success && apiId.HasValue)
                        {
                            incident.ApiSynced = true;
                            incident.ApiId = apiId.Value;
                            await _databaseService.SaveIncidentAsync(incident);
                            System.Diagnostics.Debug.WriteLine($"✅ Incidencia sincronizada con API ID: {apiId}");
                        }
                    }

                    // Mostrar confirmación
                    await Shell.Current.DisplayAlert("✅ Éxito",
                        "Incidencia creada correctamente", "OK");

                    // Volver atrás
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "❌ Error al guardar la incidencia";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar incidencia: {ex.Message}");
                ErrorMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando para cancelar y volver atrás
        /// </summary>
        [RelayCommand]
        private async Task CancelAsync()
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Cancelar",
                "¿Estás seguro? Se perderán los datos ingresados.",
                "Sí",
                "No");

            if (confirm)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        /// <summary>
        /// Limpia el formulario
        /// </summary>
        public void ClearForm()
        {
            Title = string.Empty;
            Description = string.Empty;
            SelectedCategory = Categories[0];
            SelectedPriority = Priorities[1];
            PhotoPath = string.Empty;
            HasPhoto = false;
            Latitude = null;
            Longitude = null;
            LocationText = "📍 Sin ubicación";
            HasLocation = false;
            ErrorMessage = string.Empty;
        }
    }
}