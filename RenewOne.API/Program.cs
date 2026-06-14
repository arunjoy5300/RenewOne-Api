using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RenewOne.API.OpenApi;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.LogicService.Auth;
using RenewOne.Application.Interfaces.LogicService.Class;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.LogicService.User;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Application.Interfaces.Repositories.Documents;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.External.R2;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Application.Interfaces.Services.Jwt;
using RenewOne.Application.Interfaces.Services.RC;
using RenewOne.Application.LogicService.Accounts;
using RenewOne.Application.LogicService.AdminEntry.PricingMaster;
using RenewOne.Application.LogicService.Auth;
using RenewOne.Application.LogicService.Class;
using RenewOne.Application.LogicService.Enquiry;
using RenewOne.Application.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.LogicService.Users;
using RenewOne.Infrastructure.Options;
using RenewOne.Infrastructure.Services.External.R2;
using RenewOne.Infrastructure.Services.Jwt;
using RenewOne.Infrastructure.Services.RC;
using RenewOne.Persistence;
using RenewOne.Persistence.Repositories.Accounts;
using RenewOne.Persistence.Repositories.AdminEntry.PricingMaster;
using RenewOne.Persistence.Repositories.AdminEntry.PricingQoute;
using RenewOne.Persistence.Repositories.Class;
using RenewOne.Persistence.Repositories.Document;
using RenewOne.Persistence.Repositories.Enquiry;
using RenewOne.Persistence.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Persistence.Repositories.Users;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build())
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddEndpointsApiExplorer();


// Dependency Injections
builder.Services.AddScoped<IAuthLogicService, AuthLogicService>();
builder.Services.AddScoped<IAccountLogicService, AccountLogicService>();
builder.Services.AddScoped<IAgentClassLogicService, AgentClassLogicService>();
builder.Services.AddScoped<IUserLogicService, UserLogicService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.Configure<RcServiceOptions>(
    builder.Configuration.GetSection("RcValidation"));

builder.Services.AddHttpClient<IRCIDSService, RCIDSService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAgentClassRepository, AgentClassRepository>();
builder.Services.AddScoped<IVehicleEnquiryLogicService, VehicleEnquiryLogicService>();
builder.Services.AddScoped<IVehicleEnquiryRepository, VehicleEnquiryRepository>();
builder.Services.AddScoped<IPricingMasterLogicService, PricingMasterLogicService>();
builder.Services.AddScoped<IPricingMasterRepository, PricingMasterRepository>();
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IFileStorageService, CloudflareR2StorageService>();
builder.Services.AddScoped<IEnquiryQuoteRepository, EnquiryQuoteRepository>();
builder.Services.AddScoped<IEnquiryLogicService,EnquiryLogicService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Host.UseSerilog();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "RenewOne API";
        options.Theme = ScalarTheme.Moon;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();