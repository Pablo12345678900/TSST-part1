using System.Net;

namespace Tools.Table_Entries
{
    public class FIB_Entry
    {
        public IPAddress destinationIP { get; set; }
        public int portOut { get; set; }

        public FIB_Entry(IPAddress destinationIp, int portOut)
        {
            destinationIP = destinationIp;
            this.portOut = portOut;
        }
    }
}