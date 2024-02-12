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
        public enum ConnectionStatus
        {
            connected = 0,
            timedout = 1,
            disconnected = 2
        }

        private ConnectionStatus _connectionStatus;

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
            packages = new Queue<Package>();
            _processingIsStarted = false;
            tcp.handler.onPacketRecived += RecevedPackage;
            _connectionStatus = ConnectionStatus.disconnected;
        }

        public void RecevedPackage(Package package)
        {
            switch (package.key)
            {
                case PackageFactory.Keys.WelcomeReceved:
                    _connectionExporation = DateTime.Now.AddMilliseconds(ServerSettings.timeOutMs);
                    _connectionStatus = ConnectionStatus.connected;
                    break;
                case PackageFactory.Keys.Heartbeat:
                    _connectionExporation = DateTime.Now.AddMilliseconds(ServerSettings.timeOutMs);
                    break;
                    //ToDo: Handle Disconnections
                default:
                    _addToServerQueue(package);
                    break;
            }
        }

        public ConnectionStatus GetConnectonStatus()
        {

            if (_connectionStatus == ConnectionStatus.connected && _connectionExporation < DateTime.Now)
                _connectionStatus = ConnectionStatus.connected;

            return _connectionStatus;
        }

        public void Dispose()
        {
            if (tcp != null)
                tcp.handler.onPacketRecived -= RecevedPackage;
            _stopProcessing = true;
        }
    }
}
