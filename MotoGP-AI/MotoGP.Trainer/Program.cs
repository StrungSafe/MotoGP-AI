using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;

namespace MotoGP.Trainer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.AddHelpers();

            builder.Services.AddSingleton<ITrainer, Trainer>();

            IHost host = builder.Build();

            var trainer = host.Services.GetRequiredService<ITrainer>();

            await trainer.TrainAndSaveModel();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}