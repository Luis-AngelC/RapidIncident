using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IncidentReportApp.Models;
using IncidentReportApp.Services;
using IncidentReportApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentReportApp.ViewModels
{
    /// <summary>
    /// ViewModel para la lista de incidencias
    /// </summary>
    public partial class IncidentListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;
        private List<Incident> _allIncidents = new();

        public IncidentListViewModel(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;

            // Inicializar filtros
            StatusFilters = new List<string>
            {
                "Todos",
                "Pendiente",
                "En Proceso",
                "Resuelto"
            };

            SelectedStatusFilter = StatusFilters[0]; // Todos
        }

        [ObservableProperty]
        private ObservableCollection<Incident> incidents = new();

        [ObservableProperty]
        private List<string> statusFilters = new();

        [ObservableProperty]
        private string selectedStatusFilter = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isEmpty = false;

        [ObservableProperty]
        private string emptyMessage = "No hay incidencias registradas";

        [ObservableProperty]
        private int totalCount = 0;

        /// <summary>
        /// Carga todas las incidencias
        /// </summary>
        public async Task LoadIncidentsAsync()
        {
            try
            {
                IsLoading = true;

                _allIncidents = await _databaseService.GetAllIncidentsAsync();

                ApplyFilters();

                System.Diagnostics.Debug.WriteLine($"✅ Cargadas {_allIncidents.Count} incidencias");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar incidencias: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Aplica los filtros de búsqueda y estado
        /// </summary>
        private void ApplyFilters()
        {
            var filteredList = _allIncidents.AsEnumerable();

            // Filtrar por estado
            if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "Todos")
            {
                filteredList = filteredList.Where(i => i.Status == SelectedStatusFilter);
            }

            // Filtrar por búsqueda de texto
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filteredList = filteredList.Where(i =>
                    i.Title.ToLower().Contains(search) ||
                    i.Description.ToLower().Contains(search) ||
                    (i.Category?.ToLower().Contains(search) ?? false));
            }

            // Actualizar colección
            Incidents.Clear();
            foreach (var incident in filteredList)
            {
                Incidents.Add(incident);
            }

            TotalCount = Incidents.Count;
            IsEmpty = Incidents.Count == 0;

            if (IsEmpty)
            {
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    EmptyMessage = $"No se encontraron resultados para '{SearchText}'";
                }
                else if (SelectedStatusFilter != "Todos")
                {
                    EmptyMessage = $"No hay incidencias con estado '{SelectedStatusFilter}'";
                }
                else
                {
                    EmptyMessage = "No hay incidencias registradas.\nCrea tu primera incidencia.";
                }
            }
        }

        /// <summary>
        /// Comando que se ejecuta cuando cambia el filtro de estado
        /// </summary>
        partial void OnSelectedStatusFilterChanged(string value)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Comando que se ejecuta cuando cambia el texto de búsqueda
        /// </summary>
        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Comando para refrescar la lista (pull to refresh)
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadIncidentsAsync();
        }

        /// <summary>
        /// Comando para navegar al detalle de una incidencia
        /// </summary>
        [RelayCommand]
        private async Task GoToDetailAsync(Incident incident)
        {
            if (incident != null)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "Incident", incident }
                };

                await Shell.Current.GoToAsync(nameof(IncidentDetailPage), navigationParameter);
            }
        }

        /// <summary>
        /// Comando para crear nueva incidencia
        /// </summary>
        [RelayCommand]
        private async Task CreateNewAsync()
        {
            await Shell.Current.GoToAsync(nameof(CreateIncidentPage));
        }

        /// <summary>
        /// Comando para eliminar una incidencia
        /// </summary>
        [RelayCommand]
        private async Task DeleteIncidentAsync(Incident incident)
        {
            try
            {
                var confirm = await Shell.Current.DisplayAlert(
                    "Confirmar",
                    $"¿Estás seguro de eliminar la incidencia '{incident.Title}'?",
                    "Sí, eliminar",
                    "Cancelar");

                if (confirm)
                {
                    var result = await _databaseService.DeleteIncidentAsync(incident.Id);

                    if (result > 0)
                    {
                        await Shell.Current.DisplayAlert("✅ Éxito", "Incidencia eliminada correctamente", "OK");
                        await LoadIncidentsAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("❌ Error", "No se pudo eliminar la incidencia", "OK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert("❌ Error", $"Error al eliminar: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Limpia la búsqueda
        /// </summary>
        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}