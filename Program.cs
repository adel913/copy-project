
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserProfileService.Contract;
using UserProfileService.Contract;
using UserProfileService.Data;
using UserProfileService.Features.Orchestrators;
using UserProfileService.Helper;
using UserProfileService.Features.Orchestrators;


namespace UserProfileService
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


            builder.Services.AddDbContext<UserProfileDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
            });
            //DI
            builder.Services.AddScoped<IImageHelper, ImageHelper>();
            builder.Services.AddScoped<IAddUserprofileQrcs, AddUserprofileQrcs>();
            builder.Services.AddScoped<IUpdateUserProfileQrccs, UpdateUserProfileQrccs>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpClient();
            builder.Services.AddMediatR(cfg =>
            {
                // تسجيل الـ Handlers من Assembly محددة
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:5001"; // URL السيرفر المحلي
                    options.Audience = "YourAudience";
                    options.RequireHttpsMetadata = false; // مهم للتطوير المحلي
                });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseStaticFiles();

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var _dbcontext = services.GetRequiredService<UserProfileDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                _dbcontext.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An Error Occurred During Apply the Migration");

            }

            app.UseHttpsRedirection();



            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
