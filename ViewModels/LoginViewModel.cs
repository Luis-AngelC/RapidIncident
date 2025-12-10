using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IncidentReportApp.Services;
using IncidentReportApp.Views;
using System.Threading.Tasks;

namespace IncidentReportApp.ViewModels
{
    /// <summary>
    /// ViewModel para la página de Login
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;

        public LoginViewModel(AuthService authService, DatabaseService databaseService)
        {
            _authService = authService;
            _databaseService = databaseService;
        }

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        /// <summary>
        /// Comando para iniciar sesión
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var trimmedUsername = Username?.Trim() ?? string.Empty;
                var trimmedPassword = Password?.Trim() ?? string.Empty;
                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(trimmedUsername))
                {
                    ErrorMessage = "⚠️ Ingresa tu nombre de usuario";
                    return;
                }

                if (string.IsNullOrWhiteSpace(trimmedPassword))
                {
                    ErrorMessage = "⚠️ Ingresa tu contraseña";
                    return;
                }

                // Intentar login
                var (success, message) = await _authService.LoginAsync(trimmedUsername, trimmedPassword);

                if (success)
                {
                    await Shell.Current.Navigation.PopToRootAsync(false);
                    // Navegar al Dashboard
                    await Shell.Current.GoToAsync($"/{nameof(DashboardPage)}");
                }
                else
                {
                    ErrorMessage = $"❌ {message}";
                    Password = string.Empty; // Limpiar contraseña
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando para ir a la página de registro
        /// </summary>
        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        /// <summary>
        /// Limpia los campos del formulario
        /// </summary>
        public void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}