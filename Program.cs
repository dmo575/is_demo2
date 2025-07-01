using demo2;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonOptionalConverter<string>());
    options.JsonSerializerOptions.Converters.Add(new JsonOptionalConverter<string?>());
    options.JsonSerializerOptions.Converters.Add(new JsonOptionalConverter<DateOnly>());
    options.JsonSerializerOptions.Converters.Add(new JsonOptionalConverter<DateOnly?>());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DbnameContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("db")));
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<UserService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseExceptionHandler("/error");

app.UseAuthorization();

app.MapControllers();

app.Run();
