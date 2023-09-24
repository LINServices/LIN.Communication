global using LIN.Types.Communication.Models;
global using Microsoft.EntityFrameworkCore;
global using LIN.Communication;
global using LIN.Types.Enumerations;
global using LIN.Types.Responses;
global using LIN.Modules;
global using Microsoft.AspNetCore.Mvc;
global using LIN.Types.Auth.Abstracts;
global using LIN.Communication.Services;
global using Http.ResponsesList;
global using Microsoft.AspNetCore.SignalR;
global using LIN.Types.Auth.Enumerations;
using LIN.Communication.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string sqlConnection = builder.Configuration["ConnectionStrings:release"] ?? string.Empty;



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

Conexión.SetStringConnection(sqlConnection);


if (sqlConnection.Length > 0)
{
    // SQL Server
    builder.Services.AddDbContext<Context>(options =>
    {
        options.UseSqlServer(sqlConnection);
    });
}


var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseCors("AllowAnyOrigin");



try
{
    // Si la base de datos no existe
    using var scope = app.Services.CreateScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<Context>();
    var res = dataContext.Database.EnsureCreated();
}
catch
{ }

Jwt.Open();


app.UseHttpsRedirection();


app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat");
app.UseAuthorization();

app.MapControllers();

app.Run();
