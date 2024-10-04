using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorFrontEnd;
using BlazorFrontEnd.Services;  // Asegúrate de tener este using si tu ApiService está en la carpeta Services

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient para apuntar a tu API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5184/") });

// Registrar ApiService
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
