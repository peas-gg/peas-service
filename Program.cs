using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PEAS.Entities;
using PEAS.Entities.Authentication;
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
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ITwilioService, TwilioService>();
builder.Services.AddScoped<IMapService, MapService>();

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
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
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