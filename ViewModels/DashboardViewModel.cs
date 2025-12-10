using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IncidentReportApp.Services;
using IncidentReportApp.Views;
using System.Threading.Tasks;

namespace IncidentReportApp.ViewModels
{
    /// <summary>
    /// ViewModel para la página Dashboard (pantalla principal)
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;
        private readonly ApiService _apiService;

        public DashboardViewModel(AuthService authService, DatabaseService databaseService, ApiService apiService)
        {
            _authService = authService;
            _databaseService = databaseService;
            _apiService = apiService;
        }

        [ObservableProperty]
        private string welcomeMessage = string.Empty;

        [ObservableProperty]
        private int totalIncidents = 0;

        [ObservableProperty]
        private int pendingIncidents = 0;

        [ObservableProperty]
        private int resolvedIncidents = 0;

        [ObservableProperty]
        private int syncedIncidents = 0;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hasConnection = false;

        /// <summary>
        /// Carga los datos del dashboard
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Obtener nombre del usuario actual
                var userName = _authService.GetCurrentUserFullName();
                WelcomeMessage = $"Bienvenido, {userName}";

                // Cargar estadísticas
                TotalIncidents = await _databaseService.GetTotalIncidentsCountAsync();
                PendingIncidents = await _databaseService.GetPendingIncidentsCountAsync();
                ResolvedIncidents = await _databaseService.GetResolvedIncidentsCountAsync();
                SyncedIncidents = await _databaseService.GetSyncedIncidentsCountAsync();

                // Verificar conexión
                HasConnection = await _apiService.CheckConnectionAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Dashboard cargado: {TotalIncidents} reportes totales");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navega a la página de crear nueva incidencia
        /// </summary>
        [RelayCommand]
        private async Task CreateNewIncidentAsync()
        {
            await Shell.Current.GoToAsync(nameof(CreateIncidentPage));
        }

        /// <summary>
        /// Navega a la lista de incidencias
        /// </summary>
        [RelayCommand]
        private async Task ViewAllIncidentsAsync()
        {
            await Shell.Current.GoToAsync(nameof(IncidentListPage));
        }

        /// <summary>
        /// Sincroniza todas las incidencias pendientes con la API
        /// </summary>
        [RelayCommand]
        private async Task SyncIncidentsAsync()
        {
            try
            {
                if (!HasConnection)
                {
                    await Shell.Current.DisplayAlert("Sin conexión", "No hay conexión a internet", "OK");
                    return;
                }

                IsLoading = true;

                var (synced, failed) = await _apiService.SyncAllIncidentsAsync(_databaseService);

                if (synced > 0)
                {
                    await Shell.Current.DisplayAlert("✅ Sincronización",
                        $"Se sincronizaron {synced} incidencias correctamente", "OK");

                    // Recargar datos
                    await LoadDataAsync();
                }
                else if (failed > 0)
                {
                    await Shell.Current.DisplayAlert("⚠️ Error",
                        "No se pudo sincronizar algunas incidencias", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("ℹ️ Info",
                        "No hay incidencias pendientes por sincronizar", "OK");
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert("❌ Error",
                    $"Error al sincronizar: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cierra sesión
        /// </summary>
        [RelayCommand]
        private async Task LogoutAsync()
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "No");

            if (confirm)
            {
                _authService.Logout();

                // Volver al login
                Application.Current.MainPage = new AppShell();
            }
        }
    }
}