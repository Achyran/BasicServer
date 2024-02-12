using Packages;
using System.Collections;
using System.Net.Mail;
using System.Net.Sockets;

namespace ServerBase
{
    public class Server : IDisposable
    {
        public int port { get; private set; }
        public int maxConnections { get; private set; }
        public TcpListener? tcpListener { get; private set; }
        public Dictionary<int, Client> clients = new Dictionary<int, Client>();


        public Queue<Package> sendQueue = new Queue<Package>();
        public Queue<Package> toBeProcessed = new Queue<Package>();
        private bool _stopProcssing = false;
        private bool _isProcessing = false;
        private bool _heartIsBeating = false;
        private bool _stopHeardBeat = false;

        public Server(int port, int maxConnections)
        {
            this.port = port;
            this.maxConnections = maxConnections;

        }

        public void Start()
        {

            Console.WriteLine("Starting Server");
            InizialisingData();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            StartProcessing();
            StartHeart();
            Console.WriteLine($"Server started on port {port}");


        }

        private void InizialisingData()
        {
            clients = new Dictionary<int, Client>();
            for (int i = 0; i <= maxConnections; i++)
            {
                clients.Add(i, new Client(i, AddToQueue));
            }
        }
        private void AddToQueue(Package package) 
        {
            toBeProcessed.Enqueue(package);
        }

        private void TcpConnectCallback(IAsyncResult _result)
        {
            if (tcpListener == null) return;

            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            Console.WriteLine($"Incomming Connection: {_client.Client.RemoteEndPoint}");

            for (int i = 0; i <= maxConnections; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    SendPackage(i, new Welcome(i, "Welcome to the server"));
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} Faild To Connect: Server is Full");

        }

        public void SendToAll(Package package)
        {
            foreach (var client in clients)
            {
                if (client.Value.IsConnected())
                {

                    client.Value.tcp.SendPackage(package);
                    LogSend(client.Key, package);
                }
            }
        }

        private void SendPackage(int user, Package package)
        {
            switch (package.protocoll)
            {
                case PackageFactory.Protocoll.tcp:
                    clients[user].tcp.SendPackage(package);
                    break;
                case PackageFactory.Protocoll.udp:
                    throw new NotImplementedException();
                default:
                    Console.WriteLine($"Protocoll not Supported: {package.protocoll}");
                    break;
            }

            LogSend(user, package);
        }

        private void LogSend(int id, Package package)
        {
            switch (ServerSettings.logLevel)
            {
                case ServerSettings.LogLevel.silent:
                    break;
                case ServerSettings.LogLevel.basic:
                    Console.WriteLine($"Sending to {id}: {package.key}");
                    break;
                case ServerSettings.LogLevel.All:
                    Console.WriteLine($"Sending to {id}: {package.key} Using {package.protocoll}");
                    break;
                default:
                    break;
            }
        }

        private void ProcessPackage()
        {
            if (sendQueue.Count <= 0) return;

            Package package = sendQueue.Dequeue();

            switch (package.key)
            {
                //TODO Fix Hardbeat Attack!
                case PackageFactory.Keys.Heartbeat:
                    SendHeadBeat();
                    break;
                case PackageFactory.Keys.Message:
                    SendMessage(package);
                    break;
                default:
                    Console.WriteLine($"Unexpected Package can not be Processed by the server. Package: {package.key}");
                    break;
            }
        }

        public void StartProcessing()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            new Thread(() => {
                while (true)
                {
                    if (_stopProcssing) break;
                    ProcessPackage();
                }
            }).Start();
        }

        public void StartHeart()
        {
            if (_heartIsBeating) return;

            _heartIsBeating = true;
            new Thread(() => {
                while (true)
                {
                    if (_stopHeardBeat) break;
                    sendQueue.Enqueue(new Heartbeat());
                    Thread.Sleep(ServerSettings.heartRate);
                }
            }).Start();
        }

        public void Dispose()
        {
            _stopProcssing = true;
        }

        #region SendPackages
        private void SendHeadBeat()
        {
            SendToAll(new Heartbeat());
        }

        private void SendMessage(Package package)
        {
            Message mesage = (Message) package;
            int[] targets = mesage.targetUserIds.ToArray();
            mesage.targetUserIds.Clear();

            foreach (int targetId in targets)
            {
                SendPackage(targetId, mesage);
            }

        }
        #endregion
    }
}
