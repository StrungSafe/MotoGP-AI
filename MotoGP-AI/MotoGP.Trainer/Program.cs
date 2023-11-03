using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;
using MotoGP.Interfaces;

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

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var reader = host.Services.GetRequiredService<IDataReader>();
            var trainer = host.Services.GetRequiredService<ITrainer>();
            var writer = host.Services.GetRequiredService<IDataWriter>();

            Season[] trainingData = await reader.Read<Season[]>(configuration["FilePath"]);
            object model = await trainer.TrainModel(trainingData);
            //writer.Write("", model);

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}