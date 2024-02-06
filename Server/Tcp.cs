using Packages;
using System.Net.Sockets;

namespace ServerBase
{
    public class Tcp
    {
        public static int defaultBufferSize = 4096;


        public TcpClient? socket;
        private readonly int _id;

        private NetworkStream? _stream;
        private byte[]? _reciveBuffer;
        public PacketHandler handler;

        public Tcp(int id)
        {
            handler = new PacketHandler();
            _id = id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = defaultBufferSize;
            socket.SendBufferSize = defaultBufferSize;

            _stream = socket.GetStream();

            _reciveBuffer = new byte[defaultBufferSize];

            _stream.BeginRead(_reciveBuffer, 0, defaultBufferSize, ReciveCallback, null);

        }

        private void ReciveCallback(IAsyncResult _result)
        {
            if (_stream == null ||
                _reciveBuffer == null) return;

            try
            {
                int byteLength = _stream.EndRead(_result);
                if (byteLength <= 0)
                {
                    //To Do disconnect client
                    return;
                }

                byte[] _data = new byte[byteLength];
                Array.Copy(_reciveBuffer, _data, byteLength);
                handler.AddData(_data);

                _stream.BeginRead(_reciveBuffer, 0, defaultBufferSize, ReciveCallback, null);
            }
            catch (Exception e)
            {
                //To Do disconnect client
                Console.WriteLine(e.Message);
            }
        }

        public void SendPackage(Package package)
        {
            if (socket == null || _stream == null)
            {
                Console.WriteLine("Failed to Send Pachage due to: No Connection is established");
                return;
            }

            byte[] data = PackageFactory.Encode(package);
            try
            {
                _stream.BeginWrite(data, 0, data.Length,null,null);
            }
            catch (Exception e)
            {

                Console.WriteLine($"Failed to Send Pachage due to: {e.Message}");
            }
        }

    }
}
