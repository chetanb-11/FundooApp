using System.Reflection;
using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using FundooApp.Middlewares;
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
using Swashbuckle.AspNetCore.SwaggerUI;

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
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
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
                    Description = "Fundoo Notes & User Management with JWT Authentication",
                    Contact = new OpenApiContact()
                    {
                        Name = "Chetan Bansal",
                        Email = "chetan6516@gmail.com",
                        Url = new Uri("https://github.com/chetanb-11/fundooapp")
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
                
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
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundoo App API v1");
                    options.DocumentTitle = "FundooApp API Docs";
                    options.DisplayRequestDuration();
                    options.DocExpansion(DocExpansion.None);
                    options.EnableFilter();
                    options.EnableTryItOutByDefault();
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
