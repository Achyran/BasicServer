using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class Heartbeat : Package
    {
        public int _id { get; private set; } = Int32.MaxValue;
        public Heartbeat() 
        {
            key = PackageFactory.Keys.Heartbeat;
            protocoll = PackageFactory.Protocoll.tcp;
        }

        public Heartbeat(int id)
        {
            key = PackageFactory.Keys.Heartbeat;
            _id = id;
        }
    }
}
