using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using qlockAPI.Core.AutoMapperConfig;
using qlockAPI.Core.Database;
using qlockAPI.Core.Services.KeyGenerationService;
using qlockAPI.Core.Services.KeyService;
using qlockAPI.Core.Services.UserService;
using qlockAPI.Websocket;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<QlockContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionQLockDB")));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddTransient<IKeyGenerationService, KeyGenerationService>();
builder.Services.AddSingleton<WebSocketHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


#region Authentication
var singingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Z91S0y5tNKB6IXL7wlCTHUZAywnnoX/KckPZ0YonZeQ="));


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
        ValidIssuer = "https://localhost:7010",
        ValidAudience = "qlockAPI",
        IssuerSigningKey = singingKey
    };
});
#endregion

var app = builder.Build();

#region WebSocket
app.UseWebSockets();
app.Use(async (context, next) =>
{
    var webSocketHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketHandler.HandleWebSocketAsync(socket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});
#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapControllers().AllowAnonymous();
}
else
{
    app.MapControllers();
}
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();


app.Run();