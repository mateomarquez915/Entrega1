// Importaciones necesarias para componentes Blazor, JavaScript y funcionalidades base
using Microsoft.AspNetCore.Components;  // Para heredar de ComponentBase y usar inyección de dependencias
using Microsoft.JSInterop;             // Para interactuar con JavaScript (sessionStorage)
using System;                          // Para uso de Exception y otros tipos básicos
using System.Collections.Generic;      // Para usar List<string> en las rutas
using System.Threading.Tasks;          // Para manejo de operaciones asíncronas

namespace BlazorFrontEnd.Services
{
    // Componente base para validar acceso a páginas. Se usa con @inherits ValidacionAcceso
    public class ValidacionAcceso : ComponentBase
    {
        // Inyección de dependencias para JavaScript y navegación
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;        // Para acceder a sessionStorage
        [Inject] protected NavigationManager Navigation { get; set; } = default!; // Para manejar navegación y rutas
        
        // Variables de control para el estado de la validación
        private bool yaValidado = false;           // Evita validaciones duplicadas
        private bool renderizadoPermitido = false; // Controla si se permite mostrar la página

        // Se ejecuta después del renderizado inicial del componente
        // firstRender: true si es el primer renderizado, false en los siguientes
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Solo valida en el primer renderizado y si no se ha validado antes
            // Esto evita bucles infinitos de validación
            if (firstRender && !yaValidado)
            {
                await ValidarAcceso();
            }
            // Llama al método base para mantener la funcionalidad heredada
            await base.OnAfterRenderAsync(firstRender);
        }

        // Controla si se debe renderizar el componente
        // Retorna: true si se debe renderizar, false si no
        protected override bool ShouldRender()
        {
            // Extrae la ruta actual sin la base URL y sin parámetros
            if (Navigation.Uri.Replace(Navigation.BaseUri, "/").Split('?')[0].Equals("/login", StringComparison.OrdinalIgnoreCase))
                return true; // Siempre permite renderizar la página de login
            
            return renderizadoPermitido; // Para otras páginas, solo si está permitido
        }

        // Método principal de validación de acceso
        // Verifica usuario, permisos y maneja redirecciones
        private async Task ValidarAcceso()
        {
            try
            {
                // Obtiene el email del usuario de sessionStorage
                // Este email se guarda durante el login
                var usuarioEmail = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "usuarioEmail");
                
                // Obtiene la ruta actual limpia (sin base URL y sin parámetros)
                var rutaActual = Navigation.Uri.Replace(Navigation.BaseUri, "/").Split('?')[0];

                // Permite acceso a login sin más validaciones
                // Esto evita bucles infinitos de redirección
                if (rutaActual.Equals("/login", StringComparison.OrdinalIgnoreCase))
                {
                    yaValidado = true;
                    renderizadoPermitido = true;
                    return;
                }

                // Verifica si hay un usuario logueado
                // Si no hay usuario, redirige al login
                if (string.IsNullOrEmpty(usuarioEmail))
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Sesión no válida. Redirigiendo al login...");
                    Navigation.NavigateTo("/login", true); // true fuerza recarga completa
                    return;
                }

                // Obtiene las rutas permitidas del sessionStorage usando JavaScript
                // Busca todas las keys que empiezan con 'ruta_' y obtiene sus valores
                var rutas = await JSRuntime.InvokeAsync<List<string>>("eval", @"
                    Object.keys(sessionStorage)
                        .filter(key => key.startsWith('ruta_'))
                        .map(key => sessionStorage.getItem(key))");

                // Verifica si la ruta actual está en las rutas permitidas
                // Si no tiene permiso, redirige a la página inicial
                if (!rutas.Contains(rutaActual))
                {
                    await JSRuntime.InvokeVoidAsync("alert", "No tienes permisos para acceder a esta página");
                    Navigation.NavigateTo("/", true); // true fuerza recarga completa
                    return;
                }

                // Si llegó hasta aquí, el acceso está permitido
                yaValidado = true;
                renderizadoPermitido = true;
                StateHasChanged(); // Notifica a Blazor que debe actualizar la UI
            }
            catch (Exception ex)
            {
                // Manejo de errores durante la validación
                Console.WriteLine($"Error en validación de acceso: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error en la validación de acceso");
                Navigation.NavigateTo("/", true); // Redirige a inicio en caso de error
            }
        }
    }
}
