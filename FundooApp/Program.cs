using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModelLayer.Model;
using ModelLayer.Model.Options;
using NLog;
using NLog.Web;
using Repository.Context;
using Repository.Interface;
using Repository.Service;

namespace FundooApp;

public class Program
{
    public static void Main(string[] args)
    {
        // static logger
        var log = LogManager.GetCurrentClassLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            log.Info("started building builder");

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.MapInboundClaims = false;
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = false,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),

                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };
                });
            builder.Services.AddSingleton(TimeProvider.System);


            builder.Services.AddDbContext<FunDooDbContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddStackExchangeRedisCache(obj =>
            {
                obj.Configuration = builder.Configuration.GetConnectionString("Redis");
                obj.InstanceName = "fundooapp:";
            });

            builder.Services.AddScoped<IUserBl, UserBl>();
            builder.Services.AddScoped<IUserRl, UserRl>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordHasher<UserDomainModel>, PasswordHasher<UserDomainModel>>();
            builder.Services.AddScoped<INoteBl, NoteBl>();
            builder.Services.AddScoped<INoteRl, NoteRl>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddControllers();

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Fundoo app API",
                    Version = "v1",
                    Description = "Fundoo Notes & User Management with JWT Authentication"
                });
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT Bearer token to authorize requests"
                });
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();
            log.Info("building app");
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionFeature?.Error != null)
                    {
                        var exceptionLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        exceptionLogger.LogError(exceptionFeature.Error, "Unhandled exception occurred during request execution: {Message}", exceptionFeature.Error.Message);
                    }

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new ResponseModel<string>
                        { Success = false, Message = "An unexpected error occurred. Please try again later." });
                });
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundoo App API v1");
                });
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
