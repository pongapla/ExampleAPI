
using DPowerAPI.Data;
using DPowerAPI.models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DPowerAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DPowerAPIContext") ?? throw new InvalidOperationException("Connection string 'DPowerAPIContext' not found.")));

builder.Services.AddControllers();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"] ?? throw new InvalidOperationException("JWT key is not configured.")))
    };
});



var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
    policy =>
    {
        policy.WithOrigins("https://localhost",
                            "https://localhost:5188",
                            "https://localhost:53318",
                            "http://localhost:58915",
                            "https://203.154.60.148:53318",
                            "http://203.154.60.148:58915",
                            "https://localhost:7169")
                            .AllowAnyOrigin()
                            .AllowAnyHeader().AllowAnyMethod();
    });
});



var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
