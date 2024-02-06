using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ServerBase;
using Packages;


namespace BasicServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Server server = new Server(5000, 10);
            server.Start();
            server.logLevel = Server.LogLevel.All;


            Console.ReadLine();
        }
    }
}
