using Framework.Core.Caching;
using Framework.Core.Data.Repositories;
using Framework.Core.Data.Uow;
using Framework.Core.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Core.SharedServices.Services
{
    public static class CommonStartup
    {
        public static void ConfigureSharedApplicationServices(this IServiceCollection services, string connectionString)
        {
            services.TryAddScoped<ICacheManager, MemoryCacheManager>();

            services.AddDbContext<CommonsDbContext>(options => options.UseSqlServer(connectionString));

            services.AddLocalization();
            services.TryAddScoped(typeof(IUnitOfWorkBase<>), typeof(UnitOfWorkBase<>));
            services.TryAddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
            services.TryAddScoped<ICommonsDbContext, CommonsDbContext>();

            services.AddScoped<INotificationsManager, NotificationsManager>();
            services.AddScoped<NotificationTemplateService>();
            services.AddScoped<NotificationLogAppService>();
            services.AddScoped<AppSettingsService>();
            services.AddScoped<LogAppService>();

            services.TryAddScoped<IEmailService, SmtpEmailService>();
            services.TryAddScoped<AttachmentService, AttachmentService>();
            services.TryAddScoped<AttachmentHelperAppService, AttachmentHelperAppService>();

            // Caching
            services.AddMemoryCache();
            services.AddResponseCompression();
        }

        public static void UseSharedApplicationDBMigration(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
            {
                serviceScope.ServiceProvider.GetService<CommonsDbContext>().Database.Migrate();
            }
        }
    }
}