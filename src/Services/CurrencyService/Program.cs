using CurrencyService.Contracts;
using CurrencyService.Hubs;
using CurrencyService.Policies;
using CurrencyService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder
    .Services.AddHttpClient<IExchangeRateService, ExchangeRateService>()
    .AddPolicyHandler(HttpPolicies.GetRetryPolicy(builder));
builder.Services.AddSignalR();
builder.Services.AddHostedService<RateUpdaterService>();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "CorsPolicy",
        builder =>
            builder
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<RatesHub>("/ws/rates");
app.UseSerilogRequestLogging();

app.Run();
