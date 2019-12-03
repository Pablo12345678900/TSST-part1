using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Tools.Table_Entries;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tools;
using System.Xml.Serialization;
using ManagerApp;


namespace Node
{
    public class Routing
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }

        public Socket SocketToForward { get; set; }
        public Socket SocketToManager { get; set; }
        public ushort Port { get; set; }
        
        public IPAddress cloudIp { get; set; }
        public ushort cloudPort { get; set; }
        
        public IPAddress ManagerIP { get; set; }
        public ushort ManagerPort { get; set; }

        public byte[] bufferForPacket = new byte[128];
        public byte[] bufferForManagement = new byte[128];
        
        
        PackageHandler packageHandler = new PackageHandler();
        public Routing(string n, IPAddress ip, ushort P)
        {
            Name = n;
            IpAddress = ip;
            Port = P;
            
        }

        public static Routing createRouter(string conFile)
        {
            StreamReader streamReader=new StreamReader(conFile);
            string line = streamReader.ReadLine();
            string name = line.Split(' ')[0];
            IPAddress ipAddress = IPAddress.Parse(line.Split(' ')[1]);
            ushort Port = ushort.Parse(line.Split(' ')[2]);
            Routing routing=new Routing(name, ipAddress,Port);

            line = streamReader.ReadLine();
            routing.cloudIp = IPAddress.Parse(line.Split(' ')[1]);
            routing.cloudPort = ushort.Parse(line.Split(' ')[2]);

            line = streamReader.ReadLine();
            routing.ManagerIP=IPAddress.Parse(line.Split(' ')[1]);
            routing.ManagerPort = ushort.Parse(line.Split(' ')[2]);
            
            
            
            return routing;

        }

        public void ActivateRouter()
        {
            SocketToForward=new Socket(cloudIp.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
            SocketToManager=new Socket(ManagerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            SocketToForward.Connect(new IPEndPoint(cloudIp,cloudPort));
            SocketToForward.Send(Encoding.ASCII.GetBytes("First Message " + this.IpAddress.ToString()));
            SocketToManager.Connect(new IPEndPoint(ManagerIP, ManagerPort));

            byte[] buffer = new byte[256];
            Console.Write("Waiting for data from management system...");
            byte[] msg = Encoding.ASCII.GetBytes(Name);

            int bytesSent = SocketToManager.Send(msg);

            String data = null;
            while (true)
            {
               int bytesRec = SocketToManager.Receive(buffer);
               data += Encoding.ASCII.GetString(buffer, 0, bytesRec);
               if (data.IndexOf("</R_config>") > -1)
                  {
                     break;
                   }
              }

                    XmlSerializer serializer = new XmlSerializer(typeof(R_config));
                    R_config result;

                    using (TextReader reader = new StringReader(data))
                    {
                        result = (R_config)serializer.Deserialize(reader);
                    }
               
            packageHandler.ILM_Table = result.ILM;
            packageHandler.FTN_Table = result.FTN;
            packageHandler.FEC_Table = result.FEC;
            packageHandler.FIB_Table = result.FIB;
            packageHandler.NHLFE_Table = result.NHLFE;

            packageHandler.displayTables();
            
            
            SocketToForward.Connect(new IPEndPoint(cloudIp,cloudPort));
            Thread forwardingThread=new Thread(WaitForPackage);
            Thread managementThread=new Thread(WaitForCommands);
            managementThread.Start();
            forwardingThread.Start();

        }

        public void WaitForPackage()
        {
            while (true)
            {
                try
                {
                    SocketToForward.Receive(bufferForPacket);
                    Console.WriteLine(" I received package");
                    ForwardPacket(bufferForPacket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public void ForwardPacket(byte[] bytes)
        {
            Package package = Package.returnToPackage(bytes);
            
            package.printInfo();

            packageHandler.handlePackage(package);

            SocketToForward.Send(package.convertToBytes());
            Console.WriteLine("I sent to cable cloud");

        }

        public void WaitForCommands()
        {
            while (true)
            {
                String data = null;


                    while (true)
                    {
                        int bytesRec = SocketToManager.Receive(bufferForManagement);
                        data += Encoding.ASCII.GetString(bufferForManagement, 0, bytesRec);
                        if (data.IndexOf("</R_config>") > -1)
                        {
                            break;
                        }
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(R_config));
                    R_config result;

                    using (TextReader reader = new StringReader(data))
                    {
                        result = (R_config)serializer.Deserialize(reader);
                    }
            packageHandler.ILM_Table = result.ILM;
            packageHandler.FTN_Table = result.FTN;
            packageHandler.FEC_Table = result.FEC;
            packageHandler.FIB_Table = result.FIB;
            packageHandler.NHLFE_Table = result.NHLFE;
            
            packageHandler.displayTables();
                
            }
        }
        
     
    }
}
