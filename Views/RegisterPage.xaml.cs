using IncidentReportApp.Services;

namespace IncidentReportApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly AuthService _authService;

        public RegisterPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                ErrorLabel.IsVisible = false;

                var username = UsernameEntry.Text?.Trim();
                var password = PasswordEntry.Text?.Trim();
                var fullName = FullNameEntry.Text?.Trim();
                var email = EmailEntry.Text?.Trim();

                var (success, message) = await _authService.RegisterAsync(username, password, fullName, email);

                if (success)
                {
                    await DisplayAlert("✅ Éxito", "Usuario registrado correctamente", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorLabel.Text = message;
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}