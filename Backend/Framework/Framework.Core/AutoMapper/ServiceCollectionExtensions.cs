using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Framework.Core.AutoMapper
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAutoMapper(this IServiceCollection services)
        {
            //dynamically load all entity and query type configurations
            var mapperConfigurations = Assembly.GetCallingAssembly().GetTypes().Where(type =>
                (type.BaseType?.IsGenericType ?? false)
                && (type.BaseType.GetGenericTypeDefinition() == typeof(IMapperProfile)));

            //create and sort instances of mapper configurations
            var instances = mapperConfigurations
                .Select(mapperConfiguration => (IMapperProfile)Activator.CreateInstance(mapperConfiguration))
                .OrderBy(mapperConfiguration => mapperConfiguration.Order);

            //create AutoMapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var instance in instances)
                {
                    cfg.AddProfile(instance.GetType());
                }
            });

            //register
            AutoMapperConfiguration.Init(config);
        }
    }
}