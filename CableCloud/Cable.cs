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
        public IPAddress Node1 { get; set; }
        public IPAddress Node2 { get; set; }
        public int port1 { get; set; }
        public int port2 { get; set; }
        public string stateOfCable { get; set; }

        public Cable(IPAddress n1, IPAddress n2, int p1, int p2, string st)
        {
            Node1 = n1;
            Node2 = n2;
            port1 = p1;
            port2 = p2;
            stateOfCable = st;
            
        }
    }
}
