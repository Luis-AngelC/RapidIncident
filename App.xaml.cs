using IncidentReportApp.Services;

namespace IncidentReportApp
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; private set; }

        public App(DatabaseService databaseService)
        {
            InitializeComponent();

            // Guardar referencia global a la base de datos
            Database = databaseService;

            // Inicializar la base de datos
            InitializeDatabaseAsync();

            // Usar AppShell como página principal
            MainPage = new AppShell();
        }

        private async void InitializeDatabaseAsync()
        {
            await Database.InitializeDatabaseAsync();
        }
    }
}