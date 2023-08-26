global using LIN.Types.Communication.Models;
global using Microsoft.EntityFrameworkCore;
global using LIN.Communication;
global using LIN.Types.Enumerations;
global using LIN.Types.Responses;
global using LIN.Modules;
using LIN.Inventory.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string sqlConnection = builder.Configuration["ConnectionStrings:release"] ?? string.Empty;


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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat");
app.UseAuthorization();

app.MapControllers();

app.Run();
