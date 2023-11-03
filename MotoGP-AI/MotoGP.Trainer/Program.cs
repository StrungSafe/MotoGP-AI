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

            builder.AddMotoGp();

            //builder.Services.AddSingleton<IDataReader, DataReader>();
            builder.Services.AddSingleton<IMlTrainer, MlTrainer>();

            IHost host = builder.Build();

            //var reader = host.Services.GetRequiredService<IDataReader>();
            var trainer = host.Services.GetRequiredService<IMlTrainer>();

            //Season[] trainingData = await reader.ReadTrainingData();
            //object model = await trainer.TrainModel(trainingData);

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}