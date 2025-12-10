using SQLite;
using System;

namespace IncidentReportApp.Models
{
    /// <summary>
    /// Modelo para representar un reporte de incidencia
    /// </summary>
    public class Incident
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Relación con User
        public int UserId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pendiente";

        [MaxLength(50)]
        public string Priority { get; set; } = "Media";

        // Ruta de la foto capturada
        [MaxLength(500)]
        public string? PhotoPath { get; set; }

        // Ubicación GPS
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(200)]
        public string? LocationName { get; set; }

        // Fechas
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Sincronización con API
        public bool ApiSynced { get; set; } = false;
        public int? ApiId { get; set; }

        // Constructor sin parámetros
        public Incident() { }

        /// <summary>
        /// Obtiene un resumen corto de la incidencia para mostrar en listas
        /// </summary>
        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                    return string.Empty;

                return Description.Length > 100
                    ? Description.Substring(0, 100) + "..."
                    : Description;
            }
        }

        /// <summary>
        /// Retorna un string con la ubicación formateada
        /// </summary>
        public string FormattedLocation
        {
            get
            {
                if (Latitude.HasValue && Longitude.HasValue)
                    return $"{Latitude:F6}, {Longitude:F6}";
                return "Sin ubicación";
            }
        }
    }
}