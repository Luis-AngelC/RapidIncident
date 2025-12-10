using SQLite;
using System;

namespace IncidentReportApp.Models
{
    /// <summary>
    /// Modelo para representar un usuario del sistema
    /// </summary>
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Constructor sin parámetros (requerido por SQLite)
        public User() { }
    }
}