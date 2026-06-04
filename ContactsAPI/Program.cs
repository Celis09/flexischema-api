using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Behaviors;
using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Contacts.Validators;
using ContactsAPI.Application.Helper;
using ContactsAPI.Infrastructure.Http;
using ContactsAPI.Data;
using ContactsAPI.Middleware;
using ContactsAPI.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CORS POLICY
// ==========================================
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Any())
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for secure frontend tokens/cookies
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("ServiceBClient")
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IConfigService, ConfigService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContactExportService, ContactExportService>();

// Configure and Register Groq IChatClient using Microsoft.Extensions.AI
var groqApiKey = builder.Configuration["AI_Providers:Groq_ApiKey"];
if (string.IsNullOrEmpty(groqApiKey))
{
    throw new InvalidOperationException("Groq API Key is missing from configuration!");
}
builder.Services.AddChatClient(new OpenAIClient(
    new System.ClientModel.ApiKeyCredential(groqApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") }
).GetChatClient("llama-3.1-8b-instant").AsIChatClient());

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAud = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAud,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Read JWT from HTTP-Only cookie first, fall back to Authorization header (Swagger/Postman)
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"]
    ?? throw new InvalidOperationException("Elasticsearch:Uri is not configured.");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUri))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "contactsapi-logs-{0:yyyy.MM}"
    })
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] (CorrelationId: {CorrelationId}, User: {UserId}, Role: {Role}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FlexiSchema API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Editor", "Admin"));
});

// ==========================================
// INFRASTRUCTURE & PERSISTENCE
// ==========================================
// Register Entity Framework Core DbContextFactory. Using a factory allows behaviors 
// (like AuditLoggingBehavior) to spin up independent DbContext instances when needed.
builder.Services.AddDbContextFactory<ContactsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)),
    lifetime: ServiceLifetime.Scoped);

builder.Services.AddValidatorsFromAssemblyContaining<CreateContactCommandValidator>();
builder.Services.AddTransient<IValidator<ContactExtraFieldRequest>, ContactExtraFieldValidator>();

// ==========================================
// CQRS & MEDIATR PIPELINE CONFIGURATION
// ==========================================
// Register MediatR to implement the CQRS pattern. Handlers are discovered automatically.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateContactCommand).Assembly));
builder.Services.AddMemoryCache();

// Register MediatR Pipeline Behaviors. The order of registration matters:
// requests go through Logging -> Correlation -> Validation -> Authorization -> Metrics -> Audit Logging -> Exception Handling
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationIdBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RoleAuthorizationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);

    options.AddPolicy("Default", policy =>
    {
        policy.Expire(TimeSpan.FromSeconds(60));
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "system" })
    .AddDbContextCheck<ContactsDbContext>("Database", tags: new[] { "db" });

var app = builder.Build();

// Automatically apply any pending EF Core migrations on startup.
// This ensures the database schema is always up to date when deployed.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

// ==========================================
// HTTP MIDDLEWARE PIPELINE
// ==========================================
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

// Enable Swagger in all environments for portfolio demonstration purposes
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlexiSchema API v1");
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message ?? "none"
            }),
            timestamp = PhilippineTime.Now
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapControllers();

app.Run();

public partial class Program { }