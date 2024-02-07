using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using ClientBase;
using Packages;


namespace BasicClient
{
    internal class Program
    {
        
        static void Main(string[] args)
        {

            Client client = new Client("127.0.0.1", 5000);
            client.Connect();

            while (true) 
            {
                Console.WriteLine($"Connected:{client.IsConnected()}");
                if(client.recivedQueue.Count > 0) 
                { 
                    Package package = client.recivedQueue.Dequeue();
                    Console.WriteLine($"Received: {package.key}");
                }
            }

        }
    }
}
