using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;

namespace MotoGP.Predictor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.AddHelpers();

            builder.Services.AddSingleton<IPredictor, Predictor>();

            IHost host = builder.Build();

            var predictor = host.Services.GetRequiredService<IPredictor>();

            await predictor.Predict();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}