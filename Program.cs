using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Middleware;
using PEAS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITwilioService, TwilioService>();

builder.Services.AddDbContext<DataContext>();

var app = builder.Build();

if (builder.Configuration["Environment"] == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Migrate Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();