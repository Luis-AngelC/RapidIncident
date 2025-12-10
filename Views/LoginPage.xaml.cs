using IncidentReportApp.ViewModels;

namespace IncidentReportApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Limpiar el formulario cada vez que aparece la página
            if (BindingContext is LoginViewModel vm)
            {
                vm.ClearForm();
            }
        }
    }
}