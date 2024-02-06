using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class WelcomeReceved : Package
    {
        public WelcomeReceved() {
            key = PackageFactory.Keys.WelcomeReceved;
            protocoll = PackageFactory.Protocoll.tcp;
        }
    }
}
