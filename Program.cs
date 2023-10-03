using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PEAS.Entities;
using PEAS.Middleware;
using PEAS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PEAS.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss'Z'";
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ITwilioService, TwilioService>();
builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

builder.Services.AddDbContext<DataContext>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (options.Events == null)
        {
            options.Events = new();
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("Secret").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
        };

        options.Events.OnTokenValidated = async context =>
        {
            var token = context.SecurityToken;
            var jwtToken = (JwtSecurityToken)token;
            _ = Guid.TryParse(jwtToken.Claims.First(x => x.Type == "id").Value, out Guid accountId);
            DataContext dbContext = context.HttpContext.RequestServices.GetService<DataContext>()!;
            context.HttpContext.Items["Account"] = await dbContext.Accounts.FindAsync(accountId);
        };
    });

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