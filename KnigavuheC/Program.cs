using Knigavuhe.Clients;
using Knigavuhe.Models;
using Knigavuhe.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Config>();
builder.Services.AddSingleton<KnigavuheClient>();
builder.Services.AddSingleton<KnigavuheService>();
builder.Services.AddSingleton<KnigavuheParser>();
builder.Services.AddSingleton<CsvService>();

var app = builder.Build();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();