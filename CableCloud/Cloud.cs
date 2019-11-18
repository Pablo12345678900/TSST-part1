using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
namespace CableCloud
{
   public class Cloud
    {
        List<Cable> cables { get; set; }
        public IPAddress cloudIp { get; set; }
        public ushort cloudPort { get; set; } // one port for cloud is enough, many sockets can operate on one port
        

        public Cloud(IPAddress adr, ushort cp)
        {
            cables = new List<Cable>();
            cloudIp = adr;
            cloudPort = cp;
        }
        public static Cloud createCloud(string conFile)
        {
            string line;
            StreamReader streamReader = new StreamReader(conFile);

            line = streamReader.ReadLine();
            IPAddress address = IPAddress.Parse(line.Split(' ')[1]);
            line = streamReader.ReadLine();
            ushort port = ushort.Parse(line.Split(' ')[1]);
            Cloud cloud = new Cloud(address, port);
            while((line=streamReader.ReadLine())!=null)
            {
                // read data about cables and add them to cloud's list
            }
            return cloud;
        }
    }
}
