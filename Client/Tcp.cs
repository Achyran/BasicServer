using Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientBase
{
    public class Tcp
    {
        public static int defaultBufferSize = 4096;

        public TcpClient? _socket { get; private set; }
        private NetworkStream? _stream;
        private byte[]? _recvBuffer;
        public PacketHandler handler;

        public Tcp() 
        {
            handler = new PacketHandler();
        }
       

        public void Connect(string ip, int port)
        {
            _socket = new TcpClient();
            _recvBuffer = new byte[defaultBufferSize];
            _socket.BeginConnect(ip, port,ConnectCallBack,_socket);
            
        }


        private void ConnectCallBack(IAsyncResult _result)
        {
            if (_recvBuffer == null) return;

            if(_socket == null) return;

            _socket.EndConnect( _result );

            if(! _socket.Connected )
            {
                return;
            }

            _stream = _socket.GetStream();

            _stream.BeginRead(_recvBuffer, 0, defaultBufferSize, ReciveCallback, null);


        }

        private void ReciveCallback(IAsyncResult _result) 
        {
            if (_stream == null ||
                   _recvBuffer == null) return;

            try
            {
                int byteLength = _stream.EndRead(_result);
                if (byteLength <= 0)
                {
                    //To Do disconnect client
                    return;
                }

                byte[] _data = new byte[byteLength];
                Array.Copy(_recvBuffer, _data, byteLength);
                handler.AddData(_data);

                _stream.BeginRead(_recvBuffer, 0, defaultBufferSize, ReciveCallback, null);


            }
            catch (Exception e)
            {
                //To Do disconnect client
                Console.WriteLine(e.Message);
            }
        }

        public void SendPackage(Package package)
        {
            if (_socket == null || _stream == null)
            {
                Console.WriteLine("Failed to Send Pachage due to: No Connection is established");
                return;
            }

            byte[] data = PackageFactory.Encode(package);
            try
            {
                _stream.BeginWrite(data, 0, data.Length, null, null);
            }
            catch (Exception e)
            {

                Console.WriteLine($"Failed to Send Pachage due to: {e.Message}");
            }
        }

    }
}
