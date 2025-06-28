using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UrlShortener.API.Data;
using UrlShortener.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Register the URL Shortening Service
builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin",
        policyBuilder => policyBuilder.WithOrigins("http://localhost:5173")
                                   .AllowAnyHeader()
                                   .AllowAnyMethod());
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});

// Clear default logging providers and add console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontendOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/{shortCode}", async (string shortCode, AppDbContext db, HttpContext http) =>
{
    var url = await db.ShortenedUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    if (url == null)
    {
        http.Response.StatusCode = 404;
        await http.Response.WriteAsync("URL not found");
        return;
    }
    http.Response.Redirect(url.OriginalUrl);
});

app.Run();
