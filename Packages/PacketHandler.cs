using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class PacketHandler
    {
        private List<byte> packetBuffer = new List<byte>();
        private int packetLength;
        public event Action<Package>? onPacketRecived;


        public void AddData(byte[] data)
        {
                packetBuffer.AddRange(data);
                CheckBuffer();
        }

        public void CheckBuffer()
        {
            PackageFactory.ReadInt(0, packetBuffer, out packetLength);
            if(packetBuffer.Count >= packetLength) 
            {
                //WE have a compleat packate;
                byte[] data = new byte[packetLength];
                byte[] buffer = new byte[packetBuffer.Count - packetLength -4];
                for (int i = 4; i < packetBuffer.Count; i++)
                {
                    if (i < packetLength + 4)
                    {
                        data[i-4] = packetBuffer[i];
                    }
                    else
                    {
                        
                        buffer[i - packetLength -4] = packetBuffer[i];
                    }


                }
                    if(onPacketRecived  != null)
                    {
                    Package? package = PackageFactory.TryDecode(data.ToList());

                        if(package != null) 
                        { 
                            onPacketRecived(PackageFactory.TryDecode(data.ToList())!);
                        }

                    }
                    packetBuffer.Clear();
                    packetBuffer.AddRange(buffer);
            }
        }
    }
}
