using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
namespace CableCloud
{

    public class Cable // nodes must be "physically" connected because of TCP, cables are connected between ports, 
                         //to completely specify node we need his name and his port
    {
        public string name1 { get; set; }
        public string name2 { get; set; }
        public ushort port1 { get; set; }
        public ushort port2 { get; set; }
        public enum stateOfCable { Disabled, Working};

        public Cable(string n1, string n2, ushort p1, ushort p2)
        {
            name1 = n1;
            name2 = n2;
            port1 = p1;
            port2 = p2;
            
        }
    }
}
