using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Configuration;

namespace MotoGP.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder AddHelpers(this HostApplicationBuilder builder)
        {
            builder.Services
                   .AddOptions()
                   .Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)))
                   .AddSingleton<IDataWriter, JsonDataService>()
                   .AddSingleton<IDataReader, JsonDataService>();

            return builder;
        }

        public static HostApplicationBuilder AddMotoGp(this HostApplicationBuilder builder)
        {
            builder.AddHelpers()
                   .Services
                   .AddSingleton<IDataRepository, DataRepository>()
                   .AddSingleton<IDataLoader, DataLoader>()
                   .AddSingleton<ThrottlingDelegatingHandler>()
                   .AddHttpClient(builder.Configuration["MotoGP:Name"],
                       client =>
                       {
                           client.BaseAddress = new Uri(builder.Configuration["MotoGP:BaseAddress"], UriKind.Absolute);
                       })
                   .AddHttpMessageHandler<ThrottlingDelegatingHandler>();

            return builder;
        }
    }
}