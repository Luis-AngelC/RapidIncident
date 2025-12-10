using IncidentReportApp.ViewModels;

namespace IncidentReportApp.Views
{
    public partial class CreateIncidentPage : ContentPage
    {
        private readonly CreateIncidentViewModel _viewModel;

        public CreateIncidentPage(CreateIncidentViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Limpiar el formulario cada vez que aparece la página
            _viewModel.ClearForm();
        }
    }
}