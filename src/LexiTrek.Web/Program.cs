using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using LexiTrek.Web;
using LexiTrek.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<JwtAuthStateProvider>(sp =>
    (JwtAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddHttpClient("LexiTrek", client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("LexiTrek"));

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<DictionaryApiService>();
builder.Services.AddScoped<DictionaryStateService>();
builder.Services.AddScoped<GroupApiService>();
builder.Services.AddScoped<LanguageApiService>();
builder.Services.AddScoped<WordApiService>();
builder.Services.AddScoped<TagApiService>();
builder.Services.AddScoped<TrainingApiService>();

await builder.Build().RunAsync();
