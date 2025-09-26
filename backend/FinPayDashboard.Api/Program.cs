  using Microsoft.EntityFrameworkCore;
  using FinPayDashboard.Api.Data;

  var builder = WebApplication.CreateBuilder(args);

  // Add services to the container.
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  // Add Entity Framework
  builder.Services.AddDbContext<FinPayDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

  // Add CORS
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowFrontend",
          policy =>
          {
              policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
          });
  });

  var app = builder.Build();

  // Configure the HTTP request pipeline.
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }

  app.UseHttpsRedirection();
  app.UseCors("AllowFrontend");
  app.UseAuthorization();
  app.MapControllers();

  app.Run();