using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class Message: Package
    {
        public List<int> targetUserIds { get; private set; }
        public int originId { get; private set; }
        public string content {  get; private set; }
        public Message(int[] targets, int origin , string message) 
        {
            originId = origin;
            key = PackageFactory.Keys.Message;
            protocoll = PackageFactory.Protocoll.tcp;

            targetUserIds = new List<int>();
            content = message;

            foreach (int i in targets) 
            { 
                targetUserIds.Add(i);
            }
        }

        public Message(int targetId, int origin, string message)
        {
            originId = origin;
            key = PackageFactory.Keys.Message;
            protocoll = PackageFactory.Protocoll.tcp;

            targetUserIds = new List<int>();
            content = message;

            targetUserIds.Add(targetId);
        }

    }
}
