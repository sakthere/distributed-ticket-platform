
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Features.Authentication.Login;
using TicketManagement.Application.Features.Authentication.Register;
using TicketManagement.Application.Interfaces;
using TicketManagement.Infrastructure.Authentication.Hashing;
using TicketManagement.Infrastructure.Authentication.Jwt;
using TicketManagement.Persistence.Context;
using TicketManagement.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using TicketManagement.Api.Middleware;
using TicketManagement.Infrastructure.Authentication.RefreshTokens;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Features.Authentication.RefreshToken;
using TicketManagement.Application.Features.Authentication.Logout;
using TicketManagement.Application.Features.Tickets.Create;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer Token"
    });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }});
});
builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();



builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var JwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = JwtSettings!.Issuer,
        ValidAudience = JwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.Configure<RefreshTokenSettings>(builder.Configuration.GetSection("RefreshTokenSettings"));

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IAuthSessionIssuer, AuthSessionIssuer>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<CreateTicketCommandHandler>();
builder.Services.AddScoped<RefreshCommandHandler>();
builder.Services.AddScoped<LogoutCommandHandler>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
