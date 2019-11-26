using System.Net;

namespace Manager
{
    public class FIB_Entry_c
    {
        public string destinationIP { get; set; }
        public int portOut { get; set; }

        public FIB_Entry_c(string destinationIp, int portOut)
        {
            destinationIP = destinationIp;
            this.portOut = portOut;
        }
        public FIB_Entry_c() { }
    }
}