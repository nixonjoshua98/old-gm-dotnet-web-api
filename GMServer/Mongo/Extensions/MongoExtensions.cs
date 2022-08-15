using GMServer.Mongo.Repositories;
using GMServer.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;

namespace GMServer.Mongo.Extensions
{
    public static class MongoExtensions
    {
        public static void AddMongoRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<ICurrenciesRepository, CurrenciesRepository>();
            services.AddSingleton<IPrestigeRepository, PrestigeRepository>();
            services.AddSingleton<IMercRepository, MercRepository>();
            services.AddSingleton<IArtefactRepository, ArtefactRepository>();
            services.AddSingleton<IBountyShopRepository, BountyShopRepository>();
            services.AddSingleton<IBountiesRepository, BountiesRepository>();
        }

        public static void AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoSettings>(configuration, "MongoDB");

            services.AddSingleton<IMongoClient, MongoClient>((services) =>
            {
                var settings = services.GetRequiredService<MongoSettings>();

                return new MongoClient(settings.ConnectionString);
            });

            services.AddSingleton<IMongoDatabase>(services =>
            {
                var client = services.GetRequiredService<IMongoClient>();

                return client.GetDatabase("GMDatabase");
            });
        }

        public static IMongoCollection<T> GetCollection<T>(this IMongoClient client, string name)
        {
            string[] parts = name.Split(".");

            if (parts.Length != 2)
                throw new ArgumentException($"Invalid dot notation collection name '{name}'");

            return client.GetDatabase(parts[0]).GetCollection<T>(parts[1]);
        }
    }
}
