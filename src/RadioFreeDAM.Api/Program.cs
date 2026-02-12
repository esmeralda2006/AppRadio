using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data;
using RadioFreeDAM.Api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddHttpClient<RadioBrowserService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Repositories
builder.Services.AddScoped<RadioFreeDAM.Api.Data.Repositories.UserRepository>();
builder.Services.AddScoped<RadioFreeDAM.Api.Data.Repositories.StationRepository>();
builder.Services.AddScoped<RadioFreeDAM.Api.Data.Repositories.FavoriteRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
