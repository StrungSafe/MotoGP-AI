using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MotoGP.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder AddHelpers(this HostApplicationBuilder builder)
        {
            builder.Services
                   .AddSingleton<IDataWriter, JsonDataService>()
                   .AddSingleton<IDataReader, JsonDataService>();

            return builder;
        }

        public static HostApplicationBuilder AddMotoGp(this HostApplicationBuilder builder)
        {
            builder.Services
                   .AddSingleton<IDataRepository, DataRepository>()
                   .AddSingleton<IDataLoader, DataLoader>()
                   .AddSingleton<IDataWriter, JsonDataService>()
                   .AddSingleton<IDataReader, JsonDataService>();

            builder.Services
                   .AddHttpClient(builder.Configuration["MotoGP:Name"],
                       client =>
                       {
                           client.BaseAddress = new Uri(builder.Configuration["MotoGP:BaseAddress"], UriKind.Absolute);
                       });
            return builder;
        }
    }
}