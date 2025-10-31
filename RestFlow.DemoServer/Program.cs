using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestFlow.DemoServer.Authentication;
using RestFlow.DemoServer.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Settings
var jwtKey = "this-is-a-very-long-secret-key-for-jwt-token-generation-minimum-32-characters";
var jwtIssuer = "RestFlow.DemoServer";
var jwtAudience = "RestFlow.Client";

builder.Services.AddSingleton(new JwtSettings
{
    SecretKey = jwtKey,
    Issuer = jwtIssuer,
    Audience = jwtAudience,
    ExpirationSeconds = 120 // 2 minutes for testing
});

builder.Services.AddSingleton<TokenService>();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
    options.DefaultChallengeScheme = "MultiScheme";
})
.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", null)
.AddScheme<AuthenticationSchemeOptions, StaticBearerAuthenticationHandler>("StaticBearer", null)
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyHeader", null)
.AddScheme<AuthenticationSchemeOptions, ApiKeyQueryAuthenticationHandler>("ApiKeyQuery", null)
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // No clock skew for testing
    };
})
.AddPolicyScheme("MultiScheme", "Multi Scheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var path = context.Request.Path.Value ?? "";
        
        if (path.Contains("/api/basic-auth"))
            return "BasicAuth";
        if (path.Contains("/api/bearer-token"))
            return "StaticBearer";
        if (path.Contains("/api/api-key/header"))
            return "ApiKeyHeader";
        if (path.Contains("/api/api-key/query"))
            return "ApiKeyQuery";
        if (path.Contains("/api/oauth"))
            return "JwtBearer";
            
        return "JwtBearer";
    };
});

// Configure CORS - Allow all for testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Root endpoint - Health check
app.MapGet("/", () => new
{
    status = "success",
    message = "RestFlow.DemoServer is running",
    version = "1.0",
    timestamp = DateTime.UtcNow
});

app.Run();
