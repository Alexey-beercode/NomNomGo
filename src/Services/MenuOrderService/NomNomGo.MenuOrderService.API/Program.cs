using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.API.Hubs;
using NomNomGo.MenuOrderService.Infrastructure.Data;
using NomNomGo.MenuOrderService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<MenuOrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Clients для интеграции с другими микросервисами
builder.Services.AddHttpClient();

// Services
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();

// SignalR для real-time обновлений
builder.Services.AddSignalR();



builder.Services.AddCors(options =>
{
    // ПОЛНОСТЬЮ ОТКРЫТАЯ политика для разработки
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Разрешить ВСЕ домены
            .AllowAnyMethod()        // Разрешить ВСЕ методы (GET, POST, PUT, DELETE, OPTIONS)
            .AllowAnyHeader();       // Разрешить ВСЕ заголовки
    });

    // Политика для разработки с credentials (если нужны cookies)
    options.AddPolicy("Development", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)  // Разрешить ВСЕ origins
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // Политика для продакшена (когда будете готовы)
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200",
                "capacitor://localhost",
                "ionic://localhost",
                "http://localhost",
                "https://localhost"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// SignalR Hub
app.MapHub<TrackingHub>("/trackingHub");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MenuOrderDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    Console.WriteLine("✅ MenuOrderService database ready!");
    await MenuOrderSeeder.SeedAsync(context);
}

app.Run();