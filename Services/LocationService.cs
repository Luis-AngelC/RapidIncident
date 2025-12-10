using System;
using System.Threading.Tasks;

namespace IncidentReportApp.Services
{
    /// <summary>
    /// Servicio para manejar la ubicación GPS del dispositivo
    /// </summary>
    public class LocationService
    {
        /// <summary>
        /// Obtiene la ubicación actual del dispositivo
        /// </summary>
        /// <returns>Objeto Location con coordenadas, o null si falló</returns>
        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                // Verificar y solicitar permisos
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Permiso de ubicación denegado");
                    return null;
                }

                // Obtener ubicación con timeout de 10 segundos
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Ubicación obtenida: {location.Latitude}, {location.Longitude}");
                    return location;
                }

                return null;
            }
            catch (FeatureNotSupportedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GPS no soportado: {ex.Message}");
                return null;
            }
            catch (PermissionException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Permiso de ubicación denegado: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener ubicación: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el nombre del lugar basado en coordenadas (Geocoding inverso)
        /// </summary>
        public async Task<string> GetLocationNameAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    var locationName = $"{placemark.Thoroughfare}, {placemark.Locality}, {placemark.AdminArea}";
                    System.Diagnostics.Debug.WriteLine($"✅ Nombre de ubicación: {locationName}");
                    return locationName;
                }

                return $"Lat: {latitude:F6}, Lon: {longitude:F6}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener nombre de ubicación: {ex.Message}");
                return $"Lat: {latitude:F6}, Lon: {longitude:F6}";
            }
        }

        /// <summary>
        /// Verifica si los servicios de ubicación están habilitados
        /// </summary>
        public async Task<bool> IsLocationEnabledAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch
            {
                return false;
            }
        }
    }
}
