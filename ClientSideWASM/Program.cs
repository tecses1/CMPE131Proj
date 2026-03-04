using ClientSideWASM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredLocalStorage();
//Initialize the network manager and make it global for all blazor objects in project.
//That way it remains 'static.' And can be accessed from the game and main page.
builder.Services.AddSingleton<NetworkManager>();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

var host = builder.Build();

var nm = host.Services.GetRequiredService<NetworkManager>();
//await nm.Initialize();

await host.RunAsync();
//Intialize and connect the NetworkManager


