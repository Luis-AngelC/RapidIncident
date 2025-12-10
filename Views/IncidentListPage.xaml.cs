using IncidentReportApp.ViewModels;

namespace IncidentReportApp.Views
{
    public partial class IncidentListPage : ContentPage
    {
        private readonly IncidentListViewModel _viewModel;

        public IncidentListPage(IncidentListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Cargar incidencias cada vez que aparece la página
            await _viewModel.LoadIncidentsAsync();
        }
    }
}