using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using XPos.Client;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API Adresi (HTTP)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5029") });

// Sepet Servisi
builder.Services.AddScoped<XPos.Client.Services.CartService>();

// MudBlazor Servisleri
builder.Services.AddMudServices();

await builder.Build().RunAsync();
