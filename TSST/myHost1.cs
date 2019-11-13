using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO; // READ FROM FILE
namespace TSST
{
    public class myHost
    {

        public List<RestOfHosts> Neighbours { get; set; } // rest of hosts in site
        public IPAddress host_IP { get; set; }
        public string hostName { get; set; } // H1, H2 etc...
        public IPAddress cloudIP { get; set; }

        public ushort cloudPort { get; set; }
        public ushort portOut { get; set; }
        public myHost()
        {
           Neighbours= new List<RestOfHosts>();
        }
       
        public static myHost createHost(string ConFile)
        {
            myHost host = new myHost();
            string line;
            StreamReader streamReader = new StreamReader(ConFile);

            line = streamReader.ReadLine();
            host.hostName = line.Split(' ')[1];

            line = streamReader.ReadLine();
            host.host_IP = IPAddress.Parse(line.Split(' ')[1]);

            line = streamReader.ReadLine();
            host.portOut = ushort.Parse(line.Split(' ')[1]);

            line = streamReader.ReadLine();
            host.cloudIP = IPAddress.Parse(line.Split(' ')[1]);

            line = streamReader.ReadLine();
            host.cloudPort = ushort.Parse(line.Split(' ')[1]);


            RestOfHosts neighbour;
            while ((line = streamReader.ReadLine()) != null)
            {
                neighbour = new RestOfHosts(line);
                host.Neighbours.Add(neighbour);
            }
            Console.WriteLine("Host has been created");
            return host;

        }

    }
}
