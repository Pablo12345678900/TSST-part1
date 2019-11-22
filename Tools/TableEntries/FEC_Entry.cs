using System.Net;

namespace Tools.Table_Entries
{
    public class FEC_Entry
    {
        public IPAddress destinationIP { get; set; }
        public int FEC { get; set; }

        public FEC_Entry(IPAddress destinationIp, int fec)
        {
            destinationIP = destinationIp;
            FEC = fec;
        }
    }
}