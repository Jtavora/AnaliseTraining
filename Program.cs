using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Data;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Serilog para logs em arquivos diferentes
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Logs no console
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day) // Logs gerais
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("SourceContext", sc => sc.Contains("Controllers")))
        .WriteTo.File("Logs/controller-.log", rollingInterval: RollingInterval.Day)) // Logs específicos das controllers
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", sc => sc.Contains("Controllers")))
        .WriteTo.File("Logs/infra-.log", rollingInterval: RollingInterval.Day)) // Logs de infraestrutura
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); // Usando o Serilog para logging

// Restante da configuração permanece igual
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();