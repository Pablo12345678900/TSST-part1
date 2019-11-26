using System.Net;

namespace Manager
{
    public class FEC_Entry_c
    {
        public string destinationIP { get; set; }
        public int FEC { get; set; }

        public FEC_Entry_c(string destinationIp, int fec)
        {
            destinationIP = destinationIp;
            FEC = fec;
        }
        public FEC_Entry_c()
        { }
    }
}