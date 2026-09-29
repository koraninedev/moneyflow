using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoneyFlow.Api.Auth;
using MoneyFlow.Api.Common;
using MoneyFlow.Api.Data;
using MoneyFlow.Api.Repositories;
using MoneyFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MoneyFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<IMonthlyPeriodsRepository, MonthlyPeriodsRepository>();
builder.Services.AddScoped<IIncomeEntriesRepository, IncomeEntriesRepository>();
builder.Services.AddScoped<IExpenseEntriesRepository, ExpenseEntriesRepository>();
builder.Services.AddScoped<IBudgetAllocationsRepository, BudgetAllocationsRepository>();
builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddScoped<ISavingGoalsRepository, SavingGoalsRepository>();
builder.Services.AddScoped<ISavingContributionsRepository, SavingContributionsRepository>();
builder.Services.AddScoped<IRecurringTemplatesRepository, RecurringTemplatesRepository>();
builder.Services.AddScoped<ISummaryRepository, SummaryRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoriesService>();
builder.Services.AddScoped<MonthsService>();
builder.Services.AddScoped<IncomesService>();
builder.Services.AddScoped<ExpensesService>();
builder.Services.AddScoped<BudgetsService>();
builder.Services.AddScoped<TransactionsService>();
builder.Services.AddScoped<SavingsService>();
builder.Services.AddScoped<RecurringService>();
builder.Services.AddScoped<ReportsService>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "dev-only-change-me-please-use-32chars!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MoneyFlow";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = "sub"
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    deployment = "github-actions"
})).AllowAnonymous();
app.MapControllers();
app.Run();

public partial class Program;

