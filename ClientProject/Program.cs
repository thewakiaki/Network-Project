using System;

namespace ClientProject
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Client client = new Client();

            if (client.Connect("127.0.0.1", 4444))
            {
                client.Run();
            }
            else
            {
                Console.WriteLine("Failed to connect to the server");
            }
        }
    }
}
