using CineTrack.Web.Components;
using CineTrack.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 1024 * 1024);
builder.Services.AddScoped<WebMovieStore>();
builder.Services.AddHttpClient<CineTrack.Services.TmdbClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
