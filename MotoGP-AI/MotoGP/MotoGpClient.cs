namespace MotoGP
{
    public class MotoGpClient : HttpClient
    {
        private readonly HttpClient client;

        public MotoGpClient(HttpClient client)
        {
            this.client = client;
        }
    }
}