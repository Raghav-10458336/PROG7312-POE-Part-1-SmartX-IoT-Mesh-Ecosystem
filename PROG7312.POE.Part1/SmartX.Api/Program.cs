using SmartX.Api.Services;

namespace SmartX.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<SensorRegistryService>();
            builder.Services.AddSingleton<TelemetryIngestionService>();
            builder.Services.AddSingleton<MockTelemetrySeeder>();
            builder.Services.AddSingleton<TelemetryAlertService>();
            builder.Services.AddSingleton<AttachmentStorageService>();

            // Register telemetry queues and their background processor.
            builder.Services.AddSingleton<TelemetryQueueService>();
            builder.Services.AddHostedService<TelemetryProcessingWorker>();

            builder.Services.AddSingleton<HistoricalTelemetryService>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SmartXClientPolicy", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7292")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();



            using (IServiceScope scope = app.Services.CreateScope())
            {
                MockTelemetrySeeder seeder =
                    scope.ServiceProvider.GetRequiredService<MockTelemetrySeeder>();

                seeder.Seed();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("SmartXClientPolicy");

            app.MapControllers();

            app.Run();
        }
    }
}
