using IncidentReportApp.Models;
using System;
using System.Threading.Tasks;

namespace IncidentReportApp.Services
{
    /// <summary>
    /// Servicio para manejar la autenticación de usuarios
    /// </summary>
    public class AuthService
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Usuario actualmente autenticado
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        /// <summary>
        /// Verifica si hay un usuario autenticado
        /// </summary>
        public bool IsAuthenticated => CurrentUser != null;

        /// <summary>
        /// Intenta autenticar a un usuario
        /// </summary>
        /// <returns>True si el login fue exitoso, False si falló</returns>
        public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(username))
                {
                    return (false, "El nombre de usuario es requerido");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return (false, "La contraseña es requerida");
                }

                // Validar credenciales contra la base de datos
                var user = await _databaseService.ValidateUserAsync(username, password);

                if (user != null)
                {
                    CurrentUser = user;
                    System.Diagnostics.Debug.WriteLine($"✅ Login exitoso: {user.Username}");
                    return (true, "Login exitoso");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Login fallido para: {username}");
                    return (false, "Usuario o contraseña incorrectos");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en login: {ex.Message}");
                return (false, $"Error al iniciar sesión: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string fullName, string email)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(username))
                {
                    return (false, "El nombre de usuario es requerido");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return (false, "La contraseña es requerida");
                }

                if (password.Length < 6)
                {
                    return (false, "La contraseña debe tener al menos 6 caracteres");
                }

                // Verificar si el usuario ya existe
                var existingUser = await _databaseService.GetUserByUsernameAsync(username);
                if (existingUser != null)
                {
                    return (false, "El nombre de usuario ya está en uso");
                }

                // Crear nuevo usuario
                var newUser = new User
                {
                    Username = username,
                    Password = password, // En producción debería estar encriptada
                    FullName = fullName,
                    Email = email,
                    CreatedAt = DateTime.Now
                };

                var result = await _databaseService.SaveUserAsync(newUser);

                if (result > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Usuario registrado: {username}");
                    return (true, "Usuario registrado exitosamente");
                }
                else
                {
                    return (false, "Error al registrar el usuario");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en registro: {ex.Message}");
                return (false, $"Error al registrar: {ex.Message}");
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        public void Logout()
        {
            if (CurrentUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Logout: {CurrentUser.Username}");
                CurrentUser = null;
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario actual
        /// </summary>
        public int GetCurrentUserId()
        {
            return CurrentUser?.Id ?? 0;
        }

        /// <summary>
        /// Obtiene el nombre completo del usuario actual
        /// </summary>
        public string GetCurrentUserFullName()
        {
            return CurrentUser?.FullName ?? "Usuario";
        }

        /// <summary>
        /// Actualiza los datos del usuario actual
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateCurrentUserAsync(string fullName, string email)
        {
            try
            {
                if (CurrentUser == null)
                {
                    return (false, "No hay usuario autenticado");
                }

                CurrentUser.FullName = fullName;
                CurrentUser.Email = email;

                var result = await _databaseService.SaveUserAsync(CurrentUser);

                if (result > 0)
                {
                    return (true, "Datos actualizados correctamente");
                }
                else
                {
                    return (false, "Error al actualizar los datos");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar usuario: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}