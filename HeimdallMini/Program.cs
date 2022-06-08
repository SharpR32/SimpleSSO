using HeimdallMini.Infrastructure.Services.Tokens;
using HeimdallMini.Persistance;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication("Token");
builder.Services.AddDbContext<LoginContext>();

builder.Services.AddSwaggerGen(ConfigureSwagger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

LoginContext.InitiateUserDatabase(app);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


static void ConfigureSwagger(SwaggerGenOptions c)
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Token", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert token into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Token"
            }
        },
        new string[] {}
    }
});
}