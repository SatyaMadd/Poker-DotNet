using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Rewrite;
using System.Text;
using Microsoft.EntityFrameworkCore;
using pokerapi.Models;
using Microsoft.Extensions.DependencyInjection;
using pokerapi.Interfaces;    
using pokerapi.Services;    
using pokerapi.Repositories;    

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PokerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add JWT Authentication
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJoinRepository, JoinRepository>();
builder.Services.AddScoped<IJoinService, JoinService>();
builder.Services.AddScoped<ILobbyRepository, LobbyRepository>();
builder.Services.AddScoped<ILobbyService, LobbyService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var rewriteOptions = new RewriteOptions()
    .AddRewrite(@"^join$", "join.html", skipRemainingRules: true)
    .AddRewrite(@"^home$", "index.html", skipRemainingRules: true)
    .AddRewrite(@"^lobby$", "lobby.html", skipRemainingRules: true);

app.UseRewriter(rewriteOptions);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapControllers();

app.Run();