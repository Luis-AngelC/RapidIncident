using IncidentReportApp.Views;

namespace IncidentReportApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas de navegación
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
            Routing.RegisterRoute(nameof(CreateIncidentPage), typeof(CreateIncidentPage));
            Routing.RegisterRoute(nameof(IncidentListPage), typeof(IncidentListPage));
            Routing.RegisterRoute(nameof(IncidentDetailPage), typeof(IncidentDetailPage));
        }
    }
}