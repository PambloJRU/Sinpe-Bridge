using ProyectoIngenieriaBACKEND_POS.Services;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();




//ESPACIOS PARA AGREGAR SERVICIOS
builder.Services.AddScoped<ISmsReceiverService, SmsReceiverService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StringSQL")));







var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
