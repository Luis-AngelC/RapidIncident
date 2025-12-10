using System;
using System.IO;
using System.Threading.Tasks;

namespace IncidentReportApp.Services
{
    /// <summary>
    /// Servicio para manejar captura de fotos con la cámara
    /// </summary>
    public class CameraService
    {
        /// <summary>
        /// Toma una foto con la cámara
        /// </summary>
        /// <returns>Ruta del archivo de la foto guardada, o null si falló</returns>
        public async Task<string> TakePhotoAsync()
        {
            try
            {
                // Verificar si la cámara está disponible
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    System.Diagnostics.Debug.WriteLine("❌ La cámara no está disponible en este dispositivo");
                    return null;
                }

                // Capturar foto
                var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Tomar foto de la incidencia"
                });

                if (photo != null)
                {
                    // Guardar la foto en almacenamiento local
                    var localFilePath = await SavePhotoAsync(photo);
                    System.Diagnostics.Debug.WriteLine($"✅ Foto guardada en: {localFilePath}");
                    return localFilePath;
                }

                return null;
            }
            catch (FeatureNotSupportedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Cámara no soportada: {ex.Message}");
                return null;
            }
            catch (PermissionException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Permiso de cámara denegado: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al tomar foto: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Permite seleccionar una foto de la galería
        /// </summary>
        public async Task<string> PickPhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Seleccionar foto"
                });

                if (photo != null)
                {
                    var localFilePath = await SavePhotoAsync(photo);
                    System.Diagnostics.Debug.WriteLine($"✅ Foto seleccionada: {localFilePath}");
                    return localFilePath;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al seleccionar foto: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda la foto en el almacenamiento local de la app
        /// </summary>
        private async Task<string> SavePhotoAsync(FileResult photo)
        {
            try
            {
                // Crear carpeta para fotos si no existe
                var photoDirectory = Path.Combine(FileSystem.AppDataDirectory, "Photos");
                Directory.CreateDirectory(photoDirectory);

                // Generar nombre único para la foto
                var fileName = $"incident_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                var filePath = Path.Combine(photoDirectory, fileName);

                // Copiar el archivo a la ubicación permanente
                using (var stream = await photo.OpenReadAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar foto: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Elimina una foto del almacenamiento
        /// </summary>
        public bool DeletePhoto(string photoPath)
        {
            try
            {
                if (File.Exists(photoPath))
                {
                    File.Delete(photoPath);
                    System.Diagnostics.Debug.WriteLine($"✅ Foto eliminada: {photoPath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar foto: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe una foto en la ruta especificada
        /// </summary>
        public bool PhotoExists(string photoPath)
        {
            return !string.IsNullOrEmpty(photoPath) && File.Exists(photoPath);
        }
    }
}