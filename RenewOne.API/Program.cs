using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RenewOne.API.Hubs;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.LogicService.Auth;
using RenewOne.Application.Interfaces.LogicService.BankAccounts;
using RenewOne.Application.Interfaces.LogicService.Class;
using RenewOne.Application.Interfaces.LogicService.Dashboard;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.LogicService.Ledger;
using RenewOne.Application.Interfaces.LogicService.PolicyPayments;
using RenewOne.Application.Interfaces.LogicService.User;
using RenewOne.Application.Interfaces.LogicService.Vendors;
using RenewOne.Application.Interfaces.LogicService.Wallet;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.Repositories.AssignmentHistories;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Application.Interfaces.Repositories.Documents;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.PolicyDetails;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Repositories.Vendor;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Application.Interfaces.Services.External.Cashfree;
using RenewOne.Application.Interfaces.Services.External.R2;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Application.Interfaces.Services.Jwt;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Application.Interfaces.Services.SignalR;
using RenewOne.Application.LogicService.Accounts;
using RenewOne.Application.LogicService.AdminEntry;
using RenewOne.Application.LogicService.AdminEntry.PricingMaster;
using RenewOne.Application.LogicService.AdminEntry.QRManaging;
using RenewOne.Application.LogicService.Auth;
using RenewOne.Application.LogicService.BankAccounts;
using RenewOne.Application.LogicService.Class;
using RenewOne.Application.LogicService.Dashboard;
using RenewOne.Application.LogicService.Enquiry;
using RenewOne.Application.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.LogicService.Ledgers;
using RenewOne.Application.LogicService.PolicyPayments;
using RenewOne.Application.LogicService.Users;
using RenewOne.Application.LogicService.Vendors;
using RenewOne.Application.LogicService.Wallet;
using RenewOne.Infrastructure.Options;
using RenewOne.Infrastructure.Services.External;
using RenewOne.Infrastructure.Services.External.R2;
using RenewOne.Infrastructure.Services.Jwt;
using RenewOne.Infrastructure.Services.RC;
using RenewOne.Infrastructure.Services.SignalR;
using RenewOne.Infrastructure.Services.User;
using RenewOne.Persistence;
using RenewOne.Persistence.Common;
using RenewOne.Persistence.Repositories.Accounts;
using RenewOne.Persistence.Repositories.AdminEntry;
using RenewOne.Persistence.Repositories.AdminEntry.PricingMaster;
using RenewOne.Persistence.Repositories.AdminEntry.PricingQoute;
using RenewOne.Persistence.Repositories.AdminEntry.QRManaging;
using RenewOne.Persistence.Repositories.AssignmentHistories;
using RenewOne.Persistence.Repositories.BankAccounts;
using RenewOne.Persistence.Repositories.Class;
using RenewOne.Persistence.Repositories.Document;
using RenewOne.Persistence.Repositories.Enquiry;
using RenewOne.Persistence.Repositories.Enquiry.PolicyDetails;
using RenewOne.Persistence.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Persistence.Repositories.UserLedger;
using RenewOne.Persistence.Repositories.Users;
using RenewOne.Persistence.Repositories.Vendor;
using RenewOne.Persistence.Repositories.Wallet;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


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

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RenewOne API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    // v10.x uses a delegate here instead of a plain object
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletlogicService, WalletLogicService>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddHttpClient<ICashfreeService, CashfreeService>();
builder.Services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
builder.Services.AddScoped<IUserTaskAllocationRepository, UserTaskAllocationRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IPolicyPaymentRepository, PolicyPaymentRepository>();
builder.Services.AddScoped<IPolicyPaymentLogicService, PolicyPaymentLogicService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IBankAccountLogicService, BankAccountLogicService>();
builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
builder.Services.AddScoped<IVehicleTypeLogicService, VehicleTypeLogicService>();
builder.Services.AddScoped<IInsuranceTypeRepository, InsuranceTypeRepository>();
builder.Services.AddScoped<IInsuranceTypeLogicService, InsuranceTypeLogicService>();
builder.Services.AddScoped<IQRMasterRepository, QRMasterRepository>();
builder.Services.AddScoped<IQRMasterLogicService, QRMasterLogicService>();
builder.Services.AddScoped<IQRMappingRepository, QRMappingRepository>();
builder.Services.AddScoped<IQRMappingLogicService, QRMappingLogicService>();
builder.Services.AddScoped<IAssignmentHistoryRepository, AssignmentHistoryRepository>();
builder.Services.AddScoped<IUserModulePermissionRepository, UserModulePermissionRepository>();
builder.Services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
builder.Services.AddScoped<IInsuranceCompanyLogicService, InsuranceCompanyLogicService>();
builder.Services.AddScoped<ILedgerTransactionRepository, LedgerTransactionRepository>();
builder.Services.AddScoped<ILedgerLogicService, LedgerLogicService>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorLogicService,VendorLogicService>();
builder.Services.AddScoped<IVendorTransactionRepository, VendorTransactionRepository>();
builder.Services.AddScoped<IVendorTransactionLogicService, VendorTransactionLogicService>();
builder.Services.AddScoped<IPolicyDetailsRepository, PolicyDetailsRepository>();
builder.Services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
builder.Services.AddSingleton<IEnquiryReportDefinitionProvider,EnquiryReportDefinitionProvider>();
builder.Services.AddScoped<IEnquiryReportRepository,EnquiryReportRepository>();
builder.Services.AddScoped<IEnquiryReportLogicService,EnquiryReportLogicService>();
builder.Services.AddScoped<IDashboardLogicService, DashboardLogicService>();
builder.Services.AddScoped<IInsuranceProductRepository, InsuranceProductRepository>();

builder.Services.AddSignalR();


builder.Services.AddControllers();

builder.Host.UseSerilog();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJs", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true) // Replaces AllowAnyOrigin() to support credentials
            .AllowCredentials()            // REQUIRED for SignalR negotiation & auth
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("NextJs");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "RenewOne API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RenewOne API v1");
    });
//}

app.MapHub<NotificationHub>("/notificationHub");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();