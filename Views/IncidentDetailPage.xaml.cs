using IncidentReportApp.ViewModels;

namespace IncidentReportApp.Views
{
    public partial class IncidentDetailPage : ContentPage
    {
        public IncidentDetailPage(IncidentDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}