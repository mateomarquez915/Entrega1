using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace BlazorFrontEnd.Services
{
    /// <summary>
    /// Servicio para manejar las operaciones CRUD con una API externa.
    /// Proporciona métodos para obtener, añadir, editar y eliminar entidades.
    /// </summary>
    public class ApiService
    {
        // Cliente HTTP para realizar las peticiones a la API
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor que inicializa el servicio con un cliente HTTP.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP inyectado por el contenedor de dependencias.</param>
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Obtiene datos de la API de forma asíncrona.
        /// </summary>
        /// <param name="endpoint">URL del endpoint de la API.</param>
        /// <returns>Una lista de diccionarios representando los datos obtenidos.</returns>
        public async Task<List<Dictionary<string, object>>> GetDataAsync(string endpoint)
        {
            try
            {
                // Realiza una petición GET al endpoint especificado
                var response = await _httpClient.GetAsync(endpoint);
                // Asegura que la respuesta sea exitosa (código 2xx)
                response.EnsureSuccessStatusCode();
                // Lee el contenido de la respuesta como string
                var content = await response.Content.ReadAsStringAsync();
                
                // Configura opciones para la deserialización JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Permite que los nombres de propiedades sean case-insensitive
                };
                
                // Deserializa el contenido JSON a una lista de diccionarios
                // Si la deserialización falla, devuelve una lista vacía
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content, options) 
                    ?? new List<Dictionary<string, object>>();
            }
            catch (HttpRequestException e)
            {
                // Registra el error en la consola
                Console.WriteLine($"Error al obtener datos: {e.Message}");
                // Re-lanza la excepción para que pueda ser manejada en un nivel superior
                throw;
            }
        }

        /// <summary>
        /// Añade una nueva entidad a través de la API.
        /// </summary>
        /// <param name="endpoint">URL del endpoint de la API.</param>
        /// <param name="entity">Diccionario que representa la entidad a añadir.</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
        public async Task<bool> AddEntityAsync(string endpoint, Dictionary<string, object> entity)
        {
            try
            {
                // Realiza una petición POST al endpoint especificado con la entidad como contenido JSON
                var response = await _httpClient.PostAsJsonAsync(endpoint, entity);
                // Asegura que la respuesta sea exitosa (código 2xx)
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException e)
            {
                // Registra el error en la consola
                Console.WriteLine($"Error al añadir entidad: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Edita una entidad existente a través de la API.
        /// </summary>
        /// <param name="endpoint">URL base del endpoint de la API.</param>
        /// <param name="id">Identificador de la entidad a editar.</param>
        /// <param name="entity">Diccionario que representa la entidad actualizada.</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
        public async Task<bool> EditEntityAsync(string endpoint, string id, Dictionary<string, object> entity)
        {
            try
            {
                // Realiza una petición PUT al endpoint especificado con la entidad como contenido JSON
                var response = await _httpClient.PutAsJsonAsync($"{endpoint}/{id}", entity);
                // Asegura que la respuesta sea exitosa (código 2xx)
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException e)
            {
                // Registra el error en la consola
                Console.WriteLine($"Error al editar entidad: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina una entidad a través de la API.
        /// </summary>
        /// <param name="endpoint">URL base del endpoint de la API.</param>
        /// <param name="id">Identificador de la entidad a eliminar.</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
        public async Task<bool> DeleteEntityAsync(string endpoint, string id)
        {
            try
            {
                // Realiza una petición DELETE al endpoint especificado
                var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
                // Asegura que la respuesta sea exitosa (código 2xx)
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException e)
            {
                // Registra el error en la consola
                Console.WriteLine($"Error al eliminar entidad: {e.Message}");
                return false;
            }
        }
    }
}