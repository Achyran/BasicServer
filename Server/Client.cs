using Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class Client : IDisposable
    {
        public int id { private set; get; }
        public Tcp tcp { private set; get; }
        public bool connectionCompleat { private set; get; }

        public Queue<Package> packages;
        private bool _processingIsStarted;
        private bool _stopProcessing;
        public delegate void QueueCallback(Package package);
        private QueueCallback _addToSendQueue;

        public Client(int _id, QueueCallback serverQueue )
        {
            id = _id;
            tcp = new Tcp(id);

            _addToSendQueue = serverQueue;
            _stopProcessing = false;
            connectionCompleat = false;
            packages = new Queue<Package>();
            _processingIsStarted = false;
            tcp.handler.onPacketRecived += RecevedPackage;
        }

        public void RecevedPackage(Package package)
        {
            packages.Enqueue(package);
        }

        public void Dispose()
        {
            if (tcp != null)
                tcp.handler.onPacketRecived -= RecevedPackage;
            _stopProcessing = true;
        }

        public void ProcessPackage()
        {
            if (packages.Count == 0) return;

            Package package = packages.Dequeue();

            switch (package.key)
            {
                case PackageFactory.Keys.WelcomeReceved:
                    ProcessPackage(package as WelcomeReceved);
                    break;

                case PackageFactory.Keys.Message:
                    ProcessPackage(package as Message);
                    break;
                default:
                    Console.WriteLine($"Unexpected Package Recived:{package.key}");
                    break;
            }


        }

        private void ProcessPackage(Message? package)
        {
            if (package == null) return;
            _addToSendQueue(package);
        }

        private void ProcessPackage(WelcomeReceved? package)
        {
            if (package == null) return;
            connectionCompleat = true;
            Console.WriteLine($"Connection Compleated on User: {id}");
        }


        

        public void StartProcessing()
        {
            if (_processingIsStarted) return;

            _processingIsStarted = true;
            new Thread(() => {
                while (true)
                {
                    ProcessPackage();
                    if (_stopProcessing) break;
                }
            }).Start();
        }
    }
}
