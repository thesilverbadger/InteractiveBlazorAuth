using BlazorApp;
using BlazorApp.Client;
using BlazorApp.Components;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.Bff;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddScoped<IRenderModeExplainer, ServerRenderModeExplainer>();
builder.Services.AddScoped<ICallApi, CallApiFromServer>();
builder.Services.AddSingleton<IUserTokenStore, ServerSideTokenStore>();

builder.Services.AddBff()
    .AddRemoteApis();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddHttpClient("server-side-api-client", http =>
    {
        http.BaseAddress = new Uri("https://localhost:7001");
    });

builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = "cookie";
        opt.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("cookie", opt =>
    {
        opt.Cookie.Name = "__Host-auto-blazor";
        opt.EventsType = typeof(CookieEvents);
    })
    .AddOpenIdConnect("oidc", opt =>
    {
        opt.Authority = "https://localhost:55293";

        opt.ClientId = "blackbird";
        opt.ClientSecret = "b3881fad-d0ea-4cb7-a524-7d799dbda067";
        opt.ResponseType = "code";
        opt.ResponseMode = "query";

        opt.Scope.Clear();
        opt.Scope.Add("openid");
        opt.Scope.Add("profile");
        opt.Scope.Add("customer.read");
        opt.Scope.Add("customer.manage");
        opt.Scope.Add("roles");

        opt.MapInboundClaims = false;
        opt.TokenValidationParameters.NameClaimType = "name";
        opt.TokenValidationParameters.RoleClaimType = "role";

        opt.GetClaimsFromUserInfoEndpoint = true;
        opt.SaveTokens = false; // No need to save tokens in the cookie because they are saved in the server side token store.

        opt.EventsType = typeof(OidcEvents);
    });

// register events to customize authentication handlers
builder.Services.AddTransient<CookieEvents>();
builder.Services.AddTransient<OidcEvents>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapRemoteBffApiEndpoint("/api/weatherforecast", "https://localhost:7001/weatherforecast")
    .RequireAccessToken(TokenType.User);

app.MapRemoteBffApiEndpoint("/api/token-details", "https://localhost:7001/token-details")
    .RequireAccessToken(TokenType.User);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
