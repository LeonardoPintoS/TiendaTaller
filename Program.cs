using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaTaller.src.Infrastructure.Data;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

#region Logging Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

#region Database Configuration
Log.Information("Configurando Base de datos sql lite");
string connectionStringDB = Environment.GetEnvironmentVariable("DATA_BASE_URL") ?? throw new ArgumentNullException("Database name not found in environment variables");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(connectionStringDB));
#endregion

var app = builder.Build();

#region Database Migration
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    // Pasamos el proveedor de servicios del scope directamente
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion

app.MapOpenApi();

app.Run();