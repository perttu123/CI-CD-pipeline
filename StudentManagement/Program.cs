using System.Security.Claims;
using System.Text;
using StudentManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentManagement.Data;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StudentContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "MyTestAuthServer", // Esim. https://my.authserver.com
        ValidAudience = "MyTestApiUsers", // Esim. https://my.apiusers.com
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("KeyVault:jwt-secret"))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy=>policy.RequireClaim(ClaimTypes.Role, "Admin"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
 builder.Services.AddSwaggerGen(c =>
 {
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
     {
         Name = "Authorization",
         Type = SecuritySchemeType.ApiKey,
         Scheme = "Bearer",
         BearerFormat = "JWT",
         In = ParameterLocation.Header,
         Description = "JWT Authorization header using the Bearer scheme."

     });
     c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
              new string[] {}
         }
     });
 });
builder.Services.AddSingleton<IKeyVaultSecretManager, KeyVaultSecretManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{

//}
var client = new SecretClient(new Uri("https://testivaultti.vault.azure.net/"), new DefaultAzureCredential());

KeyVaultSecret secret = client.GetSecret("jwt-secret");

Console.WriteLine($"Secret Value: {secret.Value}");

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();