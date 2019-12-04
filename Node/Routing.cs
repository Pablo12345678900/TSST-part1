using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Tools.Table_Entries;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tools;
using System.Xml.Serialization;
using ManagerApp;
using System.Globalization;


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
        public byte[] bufferForManagement = new byte[4096];
        
        
        PackageHandler packageHandler=new PackageHandler();
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
            routing.cloudIp = IPAddress.Parse(line.Split(' ')[0]);
            routing.cloudPort = ushort.Parse(line.Split(' ')[1]);

            line = streamReader.ReadLine();
            routing.ManagerIP=IPAddress.Parse(line.Split(' ')[0]);
            routing.ManagerPort = ushort.Parse(line.Split(' ')[1]);
            
            
            
            return routing;

        }

        public void ActivateRouter()
        {
            Console.WriteLine("My name is: " + this.Name);
            SocketToForward=new Socket(cloudIp.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
            SocketToManager=new Socket(ManagerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            SocketToForward.Connect(new IPEndPoint(cloudIp,cloudPort));
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            System.Globalization.CultureInfo.InvariantCulture) + "] " + "Connected with cloud :)");
            SocketToForward.Send(Encoding.ASCII.GetBytes("First Message " + this.IpAddress.ToString()));
            SocketToManager.Connect(new IPEndPoint(ManagerIP, ManagerPort));
            
            byte[] buffer = new byte[4096];
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture) + "] " + "Connected with manager :)");
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
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture) + "] " + "I got MPLS Tables :) ");
            
            packageHandler.displayTables();
           // SocketToForward.Connect(new IPEndPoint(cloudIp,cloudPort));
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
                    Package package = Package.returnToPackage(bufferForPacket);
                    Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture) + "] " + "I received package at port: " + package.Port);
                    package.printInfo();

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
            
            packageHandler.handlePackage(package);

            SocketToForward.Send(package.convertToBytes());
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                  CultureInfo.InvariantCulture) + "] " + "I sent package by port: "
                              + package.Port);
            package.printInfo();

        }

        public void WaitForCommands()
        {
            while (true)
            {
                String data = null;


                while (true)
                {
                    int bytesRec = SocketToManager.Receive(bufferForManagement);
                Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                     CultureInfo.InvariantCulture) + "] " + "I got new configuration!!!");

                 data += Encoding.ASCII.GetString(bufferForManagement, 0, bytesRec);
                    if (data.IndexOf("</R_config>") > -1)
                    {
                        break;
                    }
                }
                
                packageHandler.displayTables();

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
                
                Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                                CultureInfo.InvariantCulture) + "] " + "I updated my MPLS tables! :) ");


            }
        }

    }
}
