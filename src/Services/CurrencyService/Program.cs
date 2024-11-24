using System.Net;
using CurrencyService.Contracts;
using CurrencyService.Hubs;
using CurrencyService.Policies;
using CurrencyService.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configure the HTTP request pipeline.
app.UseRouting();
app.MapControllers();
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.MapHub<RatesHub>("/ws/rates");

app.Run();
