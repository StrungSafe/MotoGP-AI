using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace MotoGP.Predictor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services
                   .AddHttpClient(builder.Configuration["MotoGP:Name"],
                       client =>
                       {
                           client.BaseAddress = new Uri(builder.Configuration["MotoGP:BaseAddress"], UriKind.Absolute);
                       });

            IHost host = builder.Build();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}