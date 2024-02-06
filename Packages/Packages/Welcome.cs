using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class Welcome : Package
    {
        public int _id { get; private set; }
        public string _message { get; private set; }
        public Welcome(int id, string message) 
        { 
            _id = id;
            _message = message;
            key = PackageFactory.Keys.Welcome;
            protocoll = PackageFactory.Protocoll.tcp;
        }
    }
}
