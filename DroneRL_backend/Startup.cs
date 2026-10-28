using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DroneRL_backend.Services;

namespace DroneRL_backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Сервисы: вся работа с базой данных идёт через них
            services.AddScoped<DroneService>();
            services.AddScoped<TrainingSessionService>();
            services.AddScoped<FlightResultService>();
            services.AddScoped<AnalyticsService>();
            services.AddScoped<AuthService>();

            var jwt = Configuration.GetSection("Jwt").Get<JwtSettings>();
            services.AddSingleton(jwt);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = jwt.GetSigningKey(),
                        ValidateLifetime = true,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    // Понятные сообщения для 401 (нет/неверный токен) и 403 (нет прав)
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return context.Response.WriteAsJsonAsync(new { message = "Unauthorized: token is missing or invalid" });
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return context.Response.WriteAsJsonAsync(new { message = "Forbidden: admin role required" });
                        }
                    };
                });
            services.AddAuthorization();

            services.AddCors();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Кнопка Authorize в Swagger для ввода токена
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Вставьте токен из POST /api/Auth/login"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        new string[0]
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Создаём базу данных с тестовыми данными, если её ещё нет
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (Configuration.GetValue<bool>("RecreateDatabaseOnStartup"))
                    db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Swagger включён и в Release-сборке, чтобы показывать API на сдаче
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors(cpb => cpb
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
