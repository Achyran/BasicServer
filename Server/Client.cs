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
        private bool _connected;

        //ToDo: Set This variable throgh the server and make it a config var
        public float acceptableHeardBeatResponseTime = 10;
        private DateTime _connectionExporation;

        public Queue<Package> packages;
        private bool _processingIsStarted;
        private bool _stopProcessing;
        public delegate void QueueCallback(Package package);
        private QueueCallback _addToServerQueue;

        public Client(int _id, QueueCallback serverQueue )
        {
            id = _id;
            tcp = new Tcp(id);

            _addToServerQueue = serverQueue;
            _stopProcessing = false;
            _connected = false;
            packages = new Queue<Package>();
            _processingIsStarted = false;
            tcp.handler.onPacketRecived += RecevedPackage;
        }

        public void RecevedPackage(Package package)
        {
            switch (package.key)
            {
                case PackageFactory.Keys.WelcomeReceved:
                    _connectionExporation = DateTime.Now.AddSeconds(acceptableHeardBeatResponseTime);
                    _connected = true;
                    break;
                case PackageFactory.Keys.Heartbeat:
                    _connectionExporation = DateTime.Now.AddSeconds(acceptableHeardBeatResponseTime);
                    break;
                    //ToDo: Handle Disconnections
                default:
                    _addToServerQueue(package);
                    break;
            }
        }

        public bool IsConnected()
        {
            if (_connected && _connectionExporation >= DateTime.Now)
                return true;
            Console.WriteLine($"Client {id} is not Connected");
            return false;
        }

        public void Dispose()
        {
            if (tcp != null)
                tcp.handler.onPacketRecived -= RecevedPackage;
            _stopProcessing = true;
        }
    }
}
