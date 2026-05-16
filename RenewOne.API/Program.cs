using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.LogicService.Auth;
using RenewOne.Application.Interfaces.LogicService.Class;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.Jwt;
using RenewOne.Application.LogicService.Accounts;
using RenewOne.Application.LogicService.Auth;
using RenewOne.Application.LogicService.Class;
using RenewOne.Infrastructure.Services.Jwt;
using RenewOne.Persistence;
using RenewOne.Persistence.Repositories.Accounts;
using RenewOne.Persistence.Repositories.Class;
using RenewOne.Persistence.Repositories.Users;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddScoped<IAuthLogicService, AuthLogicService>();
builder.Services.AddScoped<IAccountLogicService,AccountLogicService>();
builder.Services.AddScoped<IAgentClassLogicService, AgentClassLogicService>();

builder.Services.AddScoped<IJwtService,JwtService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAgentClassRepository, AgentClassRepository>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
