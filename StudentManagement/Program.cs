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
    // options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    // {
    //     ValidateIssuer = true,
    //     ValidateAudience = true,
    //     ValidateLifetime = true,
    //     ValidateIssuerSigningKey = true,
    //     ValidIssuer = "MyTestAuthServer", // Esim. https://my.authserver.com
    //     ValidAudience = "MyTestApiUsers", // Esim. https://my.apiusers.com
    //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Lokaali-secret-jwt-tokeni-tähän-u12u3j1u3u123ju12-asdasdasdasdasdsa"))
    // };
    // Retrieve the JWT secret from Azure Key Vault
    var keyVaultUrl = builder.Configuration["KeyVault:BaseUrl"];
    var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    KeyVaultSecret secret = client.GetSecret("jwt-secret");  // Replace "jwt-secret" with the actual name of your secret
    
    var jwtSecret = secret.Value;  // This is the JWT secret fetched from Key Vault

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "MyTestAuthServer", // Your issuer URL
        ValidAudience = "MyTestApiUsers", // Your audience URL
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))  // Use the secret fetched from Key Vault
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

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
         .AddEnvironmentVariables();

    var keyVaultUrl = builder.Configuration["KeyVault:BaseUrl"];


    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}
builder.Services.AddSingleton<IKeyVaultSecretManager, KeyVaultSecretManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{

//}


app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();