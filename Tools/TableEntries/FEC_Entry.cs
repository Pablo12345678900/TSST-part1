using System;

namespace Tools.Table_Entries
{
    public class FEC_Entry
    {
        public string destinationIP { get; set; }
        public int FEC { get; set; }

        public FEC_Entry(string destinationIp, int fec)
        {
            destinationIP = destinationIp;
            FEC = fec;
        }
        
        public FEC_Entry() { }
        
        public void print()
        {
            Console.WriteLine(" destinationIP: " + destinationIP + "FEC: " + FEC);
        }
    }
}