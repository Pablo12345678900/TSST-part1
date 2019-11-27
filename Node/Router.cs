using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Tools.Table_Entries;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tools;


namespace Node
{
    public class Router
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public List<NHLFE_Entry> NHLFE_Table { get; set; }
        public List<ILM_Entry> ILM_Table { get; set; }
        public List<FTN_Entry> FTN_Table { get; set; }
        public List<FEC_Entry> FEC_Table { get; set; }
        public List<FIB_Entry> FIB_Table { get; set; }
        
        
        public Socket SocketToForward { get; set; }
        public Socket SocketToManager { get; set; }
        public ushort Port { get; set; }
        
        public IPAddress cloudIp { get; set; }
        public ushort cloudPort { get; set; }
        
        public IPAddress ManagerIP { get; set; }
        public ushort ManagerPort { get; set; }

        public byte[] bufferForPacket = new byte[128];
        public byte[] bufferForManagement = new byte[128];
        
        
        PackageHandler _packageHandler=new PackageHandler();
        public Router(string n, IPAddress ip, ushort P)
        {
            Name = n;
            IpAddress = ip;
            Port = P;
            ILM_Table = new List<ILM_Entry>();
            FTN_Table = new List<FTN_Entry>();
            FEC_Table = new List<FEC_Entry>();
            FIB_Table = new List<FIB_Entry>();
        }

        public static Router createRouter(string conFile)
        {
            StreamReader streamReader=new StreamReader(conFile);
            string line = streamReader.ReadLine();
            string name = line.Split(' ')[0];
            IPAddress ipAddress = IPAddress.Parse(line.Split(' ')[1]);
            ushort Port = ushort.Parse(line.Split(' ')[2]);
            Router router=new Router(name, ipAddress,Port);

            line = streamReader.ReadLine();
            router.cloudIp = IPAddress.Parse(line.Split(' ')[1]);
            router.cloudPort = ushort.Parse(line.Split(' ')[2]);

            line = streamReader.ReadLine();
            router.ManagerIP=IPAddress.Parse(line.Split(' ')[1]);
            router.ManagerPort = ushort.Parse(line.Split(' ')[2]);
            
            
            
            return router;

        }

        public void ActivateRouter()
        {
            SocketToForward=new Socket(cloudIp.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
            SocketToManager=new Socket(ManagerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //SocketToForward.Connect(new IPEndPoint(cloudIp,cloudPort));
            SocketToManager.Connect(new IPEndPoint(ManagerIP, ManagerPort));

            byte[] buffer = new byte[256];
            Console.Write("Waiting for data from management system...");
            SocketToManager.Receive(buffer);
            // change buffer to tables...
            
            //
            SocketToManager.Send(Encoding.ASCII.GetBytes("ACK")); // confirmation
            
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
            
            _packageHandler.handlePackage(package);

            SocketToForward.Send(package.convertToBytes());
            Console.WriteLine("I sent to cable cloud");

        }

        public void WaitForCommands()
        {
            while (true)
            {
                SocketToManager.Receive(bufferForManagement);
                
            }
        }

       
    }
}