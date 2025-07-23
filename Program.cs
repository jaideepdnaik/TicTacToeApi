using TicTacToeApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    
    options.AddPolicy("SignalRPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Add your frontend URLs
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials(); // Required for SignalR
        });
});

builder.Services.AddSingleton<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("SignalRPolicy");

app.MapControllers();

// Map SignalR Hub
app.MapHub<GameHub>("/gameHub");

app.Run();
