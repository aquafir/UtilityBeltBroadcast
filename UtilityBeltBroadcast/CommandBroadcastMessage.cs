using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBeltBroadcast
{
    //TODO: Fix hacky copy.  For some reason this wasn't showing up when referenced
    [Serializable]
    class CommandBroadcastMessage
    {
        public string Command { get; set; }
        public int Delay { get; set; }

        public CommandBroadcastMessage() { }

        public CommandBroadcastMessage(string command, int delay = 0)
        {
            Command = command;
            Delay = delay;
        }
    }
}
