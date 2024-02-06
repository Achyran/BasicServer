using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Packages;

namespace ClientBase
{
    public class Client : IDisposable
    {
        public string ip { get; private set; }
        private int _port;
        public int id { get; private set; }
        public Tcp? _tcp;
        public Queue<Package> revicedQueue;
        public Queue<Package> sendingQueue;
        private bool _processingIsStarted;
        private bool _stopProcessing;
        private bool _isSending;
        private bool _stopSending;

        public Client(string ip, int port)
        {
            this.ip = ip;
            _port = port;
            revicedQueue = new Queue<Package>();
            sendingQueue = new Queue<Package>();
            _processingIsStarted = false;
            _stopProcessing = false;
            _isSending = false;
            _stopSending = false;

        }

        public void Connect()
        {
            StartProcessing();
            StartSendingProcess();
            _tcp = new Tcp();
            _tcp.handler.onPacketRecived += RecevedPackage;
            _tcp.Connect(ip, _port);
        }
        

        public void RecevedPackage(Package package) 
        {
            revicedQueue.Enqueue(package);
        }

        public void Dispose()
        {
            if (_tcp != null)
            _tcp.handler.onPacketRecived -= RecevedPackage;
            _stopProcessing = true;
        }

        public void ProcessPackage()
        {
            if (revicedQueue.Count == 0) return;

            Package package = revicedQueue.Dequeue();

            switch (package.key)
            {
                case PackageFactory.Keys.Welcome:
                    ProcessPackage(package as Welcome);
                    break;
                case PackageFactory.Keys.Heartbeat:
                    ProcessPackage(package as Heartbeat);
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
            Console.WriteLine($"Recived: {package.content}\n ->form: {package.originId}");

        }

        private void ProcessPackage(Heartbeat? package)
        {
            if (package == null) return;
            Console.WriteLine("Processed Heartbeat");
        }

        private void ProcessPackage(Welcome? package) 
        {
            if(package == null) return;

            id = package._id;
            if(_tcp != null)
            {

                sendingQueue.Enqueue(new WelcomeReceved());
                Console.WriteLine($"Processed Welcome {package._message}");
                return;
            }
            Console.WriteLine("Welcome message could not be responed to");
        }

        public void SendPackage(Package package)
        {   
            sendingQueue.Enqueue(package);
        }

        private void SendingPackage()
        {
            if(sendingQueue.Count <= 0 || _tcp == null) return;
            
            Package package = sendingQueue.Dequeue();
            switch (package.protocoll)
            {
                case PackageFactory.Protocoll.tcp:
                    _tcp.SendPackage(package);
                    break;
                case PackageFactory.Protocoll.udp:
                    throw new NotImplementedException();
                    break;
                default:
                    Console.WriteLine($"Protocoll not supported {package.protocoll}");
                    break;
            }
        }

        public void StartProcessing()
        {
            if (_processingIsStarted) return;

            _processingIsStarted = true;
            new Thread(() => {
               while (true)
               {
                    if (_stopProcessing) break;
                    ProcessPackage();
               }
           }).Start();
        }

        public void StartSendingProcess()
        {
            if(_isSending) return;
            _isSending = true;

            new Thread(() =>
            {
                while (true)
                {
                    if (_stopSending) break;
                    SendingPackage();
                }
            }).Start();
        }
    }
}
