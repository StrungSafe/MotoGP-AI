using Microsoft.Extensions.Hosting;

namespace MotoGP.Predictor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            IHost host = builder.Build();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}