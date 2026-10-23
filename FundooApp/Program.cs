using FundooApp.Extensions;
using FundooApp.Middlewares;
using ModelLayer.Model;
using ModelLayer.Model.Options;
using NLog;
using NLog.Web;

namespace FundooApp;

public class Program
{
    public static void Main(string[] args)
    {
        var log = LogManager.GetCurrentClassLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            log.Info("started building builder");

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddDatabaseContext(builder.Configuration);

            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddStackExchangeRedisCache(obj =>
            {
                obj.Configuration = builder.Configuration.GetConnectionString("Redis");
                obj.InstanceName = "fundooapp:";
            });

            builder.Services.AddApplicationServices();
            builder.Services.AddControllers();

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();

            var app = builder.Build();
            log.Info("building app");

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            log.Info("running app");
            app.Run();
        }
        catch (Exception ex)
        {
            log.Fatal(ex, "Application stopped because of the exception");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}
