using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Carregar .env para desenvolvimento local (opcional)
if (File.Exists(".env"))
{
    foreach (var line in File.ReadAllLines(".env"))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2) Environment.SetEnvironmentVariable(parts[0], parts[1]);
    }
}

var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? builder.Configuration["Supabase:Url"];
var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? builder.Configuration["Supabase:Key"];
var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true };

// Injeta o Client diretamente para uso nos Controllers
builder.Services.AddSingleton(_ => new Supabase.Client(supabaseUrl, supabaseKey, options));

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("CamaraApi", client =>
{
    client.BaseAddress = new Uri("https://dadosabertos.camara.leg.br/api/v2/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Explorar}/{action=Index}/{id?}");

app.Run();