using System;
using System.Net;

namespace Tools.Table_Entries
{
    public class FIB_Entry
    {
        public string destinationIP { get; set; }
        public ushort portOut { get; set; }

        public FIB_Entry(string destinationIp, ushort portOut)
        {
            destinationIP = destinationIp;
            this.portOut = portOut;
        }
        
        public FIB_Entry() { }
        
        public void print()
        {
            Console.WriteLine(" destinationIP: " + destinationIP + "portOut: " + portOut);
        }
    }
}