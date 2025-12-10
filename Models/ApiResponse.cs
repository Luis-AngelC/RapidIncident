using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentReportApp.Models
{
    /// <summary>
    /// Modelo genérico para manejar respuestas de la API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse()
        {
            Success = false;
            Message = string.Empty;
        }
    }

    /// <summary>
    /// Modelo para simular un post de JSONPlaceholder
    /// </summary>
    public class JsonPlaceholderPost
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; } = "";
        public string body { get; set; } = "";
   }
 }
 