using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Analyzer;
using MotoGP.Configuration;
using MotoGP.Data;
using MotoGP.Predictor;
using MotoGP.Repositories;
using MotoGP.Scraper;
using MotoGP.Trainer;
using MotoGP.Utilities;

namespace MotoGP.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder AddAll(this HostApplicationBuilder builder)
        {
            return builder.AddMotoGp().AddScraper().AddAnalyzer().AddTrainer().AddPredictor();
        }

        public static HostApplicationBuilder AddAnalyzer(this HostApplicationBuilder builder)
        {
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

            builder.Services.AddSingleton<IDataAnalyzer, DataAnalyzer>();
            return builder;
        }

        public static HostApplicationBuilder AddMotoGp(this HostApplicationBuilder builder)
        {
            builder.AddUtilities()
                   .Services
                   .AddOptions()
                   .Configure<RepositorySettings>(builder.Configuration.GetSection(nameof(RepositorySettings)))
                   .AddSingleton<IDataRepository, DataRepository>()
                   .AddSingleton<IDataLoader, DataLoader>()
                   .AddSingleton<ThrottlingDelegatingHandler>()
                   .AddHttpClient(builder.Configuration["RepositorySettings:Name"],
                       client =>
                       {
                           client.BaseAddress = new Uri(builder.Configuration["RepositorySettings:BaseAddress"],
                               UriKind.Absolute);
                       })
                   .AddHttpMessageHandler<ThrottlingDelegatingHandler>();

            return builder;
        }

        public static HostApplicationBuilder AddPredictor(this HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDataPredictor, DataPredictor>();
            return builder;
        }

        public static HostApplicationBuilder AddScraper(this HostApplicationBuilder builder)
        {
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

            builder.Services.AddSingleton<IDataScraper, DataScraper>();
            return builder;
        }

        public static HostApplicationBuilder AddTrainer(this HostApplicationBuilder builder)
        {
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

            builder.Services.AddSingleton<IDataTrainer, DataTrainer>()
                   .AddSingleton<IDataFormatter, SimpleDataFormatter>();
            return builder;
        }

        public static HostApplicationBuilder AddUtilities(this HostApplicationBuilder builder)
        {
            builder.Services
                   .AddSingleton<IDataWriter, JsonDataService>()
                   .AddSingleton<IDataReader, JsonDataService>();

            return builder;
        }
    }
}