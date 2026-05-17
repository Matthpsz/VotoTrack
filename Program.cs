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

// Middleware de Telemetria Integrado para Auditoria e Monitoramento em Produção
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    
    // Ignorar caminhos de arquivos estáticos (CSS, JS, Imagens, Libs, etc.)
    if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || path.StartsWith("/images") || path.Contains("."))
    {
        await next();
        return;
    }

    // Retorna o último erro registrado nos cabeçalhos HTTP para diagnóstico remoto
    try
    {
        context.Response.Headers["X-Telemetry-Last-Error"] = TelemetryDebug.LastError;
    }
    catch { }

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();

        try
        {
            var clientSupabase = context.RequestServices.GetRequiredService<Supabase.Client>();
            var emailUsuario = "Anonimo";

            try
            {
                if (clientSupabase.Auth.CurrentUser != null)
                {
                    emailUsuario = clientSupabase.Auth.CurrentUser.Email ?? clientSupabase.Auth.CurrentUser.Id;
                }
            }
            catch { }

            var pathStr = path;
            var methodStr = context.Request.Method;
            var statusInt = context.Response.StatusCode;
            var elapsedLong = stopwatch.ElapsedMilliseconds;
            var dateStr = DateTime.UtcNow;

            // Salva de forma assíncrona em segundo plano para não travar a resposta do usuário
            _ = Task.Run(async () =>
            {
                try
                {
                    // Conexão direta de alto desempenho com o banco de dados Postgres do Gateway (usando o Connection Pooler IPv4)
                    string connStr = "Server=aws-1-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;User Id=postgres.kyraduhxzrxbgwrsblqe;Password=yJWK4Nkfh&GUpn*;Ssl Mode=Require;Trust Server Certificate=true;";
                    
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        await conn.OpenAsync();
                        
                        using (var cmd = new Npgsql.NpgsqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = "INSERT INTO \"LogsTelemetria\" (\"Rota\", \"MetodoHttp\", \"StatusCode\", \"TempoExecucaoMs\", \"DataRequisicao\", \"ChaveApi\", \"Projeto\") VALUES (@rota, @metodo, @status, @tempo, @data, @chave, @projeto)";
                            
                            cmd.Parameters.AddWithValue("rota", pathStr);
                            cmd.Parameters.AddWithValue("metodo", methodStr);
                            cmd.Parameters.AddWithValue("status", statusInt);
                            cmd.Parameters.AddWithValue("tempo", elapsedLong);
                            cmd.Parameters.AddWithValue("data", dateStr);
                            cmd.Parameters.AddWithValue("chave", (object)emailUsuario ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("projeto", "VotoTrack");
                            
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryDebug.LastError = $"{ex.GetType().Name}: {ex.Message}";
                    Console.WriteLine($"[Telemetria] Erro ao gravar log no Supabase Postgres do Gateway: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Telemetria] Falha geral no processador de logs: {ex.Message}");
        }
    }
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

var cultureInfo = new System.Globalization.CultureInfo("pt-BR");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Explorar}/{action=Index}/{id?}");

app.Run();

// Classe Auxiliar para Diagnóstico Remoto
public static class TelemetryDebug
{
    public static string LastError = "Nenhum erro registrado ainda";
}