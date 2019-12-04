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
        public byte[] bufferForManagement = new byte[4096];
        
        
        PackageHandler _packageHandler=new PackageHandler();
        public Routing(string n, IPAddress ip, ushort P)
        {
            Name = n;
            IpAddress = ip;
            Port = P;
            ILM_Table = new List<ILM_Entry>();
            FTN_Table = new List<FTN_Entry>();
            FEC_Table = new List<FEC_Entry>();
            FIB_Table = new List<FIB_Entry>();
            NHLFE_Table = new List<NHLFE_Entry>();
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
               
            ILM_Table = result.ILM;
            FTN_Table = result.FTN;
            FEC_Table = result.FEC;
            FIB_Table = result.FIB;
            NHLFE_Table = result.NHLFE;
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture) + "] " + "I got MPLS Tables :) ");
            
            
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
                                            CultureInfo.InvariantCulture) + "] " + "I received package at port: " + package.Port + " ID-> " + package.messageID + " payload: " + package.payload);

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
            
            this.handlePackage(package);

            SocketToForward.Send(package.convertToBytes());
            Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture) + "] " + "I sent package by port: "
                                            + package.Port + " package ID: " + package.messageID + " payload: " + package.payload);
            if(package.labelStack.Empty())
                Console.WriteLine("Label stack  of that package is empty");
            else
            {
                Console.WriteLine("Stack of that package: ");
                for (int i = 0; i < package.labelStack.labels.Count; i++)
                {
                    Console.Write(package.labelStack.labels.ToArray()[i].labelNumber + " ");
                }
            }
            

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

                    XmlSerializer serializer = new XmlSerializer(typeof(R_config));
                    R_config result;

                    using (TextReader reader = new StringReader(data))
                    {
                        result = (R_config)serializer.Deserialize(reader);
                    }
            ILM_Table = result.ILM;
            FTN_Table = result.FTN;
            FEC_Table = result.FEC;
            FIB_Table = result.FIB;
            NHLFE_Table = result.NHLFE;
                Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff",
                                                CultureInfo.InvariantCulture) + "] " + "I updated my MPLS tables! :) ");


            }
        }
        
        
        
        public void handlePackage(Package package)
        {
            NHLFE_Entry nhlfeEntry = null;
            FIB_Entry fibEntry = null;
            
            if (package.labelStack.labels.Any())                         //check if stack has any elements
            {
                if (package.labelStack.labels.Peek().labelNumber == 0)    // check if i'm last hop
                {
                    //getting rid of all '0' labels (pushed in penultimate router)
                    while (package.labelStack.labels.Peek().labelNumber == 0)
                    {
                        package.labelStack.labels.Pop();
                        if (!package.labelStack.labels.Any())
                        {
                            break;
                        }
                    }

                    //if there are no labels left all tunnels are over. check fib table and return
                    if (!package.labelStack.labels.Any())
                    {
                        fibEntry = findFibEntry(package.DestinationAddress);
                        package.Port = (ushort) fibEntry.portOut;
                        return;
                    }

                }
                
                ILM_Entry ilmEntry = findIlmEntry(package.Port, package.labelStack.labels.Peek().labelNumber);
                nhlfeEntry = findNhlfeEntry(ilmEntry.NHLFE_ID);
                
            }
            else
            {
                FEC_Entry fecEntry = findFecEntry(package.DestinationAddress);
                
                if (fecEntry != null)                         //adding label
                {
                    FTN_Entry ftnEntry = findFtnEntry(fecEntry.FEC);
                    nhlfeEntry = findNhlfeEntry(ftnEntry.NHLFE_ID);
                }
                else                                         //forwarding by IPAddress
                {
                    fibEntry = findFibEntry(package.DestinationAddress);
                    
                }
            }

            modifyPackage(package, nhlfeEntry, fibEntry);
            
        }


        private void modifyPackage(Package package, NHLFE_Entry nhlfeEntry, FIB_Entry fibEntry)
        {
            if (nhlfeEntry != null)
            {

                package.Port = (ushort)nhlfeEntry.portOut;
                switch (nhlfeEntry.action)
                {
                    case "swap":
                    {
                        package.labelStack.labels.Pop();

                        Label newLabel = new Label();
                        newLabel.labelNumber = nhlfeEntry.labelsOut[0];
                        package.labelStack.labels.Push(newLabel);
                        break;
                    }
                    case "push":
                    {
                        if (package.labelStack.labels.Any())
                        {
                            package.labelStack.labels.Pop();
                        }
                        
                        foreach (ushort label in nhlfeEntry.labelsOut)
                        {
                            Label newLabel = new Label();
                            newLabel.labelNumber = label;
                            package.labelStack.labels.Push(newLabel);
                        }

                        break;
                    }
                    case "pop":
                    {
                        //pop labels to replace them with '0'
                        for (int i = 0; i < nhlfeEntry.popDepth; i++)
                        {
                            package.labelStack.labels.Pop();
                        }
                        
                        //swap
                        if (package.labelStack.labels.Any())
                        {
                            package.labelStack.labels.Pop();

                            foreach (ushort label in nhlfeEntry.labelsOut)
                            {
                                Label newLabel = new Label();
                                newLabel.labelNumber = label;
                                package.labelStack.labels.Push(newLabel);
                            }
                        }

                        // add '0' labels 
                        for (int i = 0; i < nhlfeEntry.popDepth; i++)
                        {
                            package.labelStack.labels.Push(new Label(0));
                        }

                        break;
                    }


                    default:
                    {
                        Console.WriteLine("Wrong value in nhlfe.action field for portOut " + package.Port +
                                          " and labelOut " + nhlfeEntry.labelsOut[0]);
                        return;
                    }
                }
            }
            else if (fibEntry != null)
            {
                package.Port = (ushort) fibEntry.portOut;
            }
            else
            {
                Console.WriteLine("Couldn't find forwarding information of package coming from port " + package.Port);
            }
        }
        
        
        public FEC_Entry findFecEntry(IPAddress destinationAddress)
        {
            FEC_Entry fecEntry = null;
            
            foreach(FEC_Entry item in FEC_Table )
            {
                if (destinationAddress.ToString().Equals(item.destinationIP))
                {
                    fecEntry = item;
                    break;
                }
            }

            if (fecEntry == null)
            {
                Console.WriteLine("No fec found for IP " + destinationAddress);
            }

            return fecEntry;
        }
        
        public FTN_Entry findFtnEntry(int FEC)
        {
            FTN_Entry ftnEntry = null;
            
            foreach(FTN_Entry item in FTN_Table )
            {
                if (FEC.Equals(item.FEC))
                {
                    ftnEntry = item;
                    break;
                }
            }

            if (ftnEntry == null)
            {
                Console.WriteLine("No ftn found for fec " + FEC);
            }

            return ftnEntry;
        }

        public NHLFE_Entry findNhlfeEntry(int nhlfeId)
        {
            NHLFE_Entry nhlfeEntry = null;
            foreach(NHLFE_Entry item in NHLFE_Table )
            {
                if (nhlfeId.Equals(item.NHLFE_ID))
                {
                    nhlfeEntry = item;
                    break;
                }
            }

            if (nhlfeEntry == null)
            {
                Console.WriteLine("No nhlfeId " + nhlfeId);
            }

            return nhlfeEntry;
        }

        public ILM_Entry findIlmEntry(int portIn, int labelIn)
        {
            ILM_Entry ilmEntry = null;
            foreach(ILM_Entry item in ILM_Table)
            {
                if (portIn.Equals(item.portIn) && labelIn.Equals(item.labelIn))
                {
                    ilmEntry = item;
                    break;
                }
            }

            if (ilmEntry == null)
            {
                Console.WriteLine("No Ilm for portIn " + portIn + " and label " + labelIn);
            }

            return ilmEntry;
        }
        
        public FIB_Entry findFibEntry(IPAddress ipAddress)
        {
            FIB_Entry fibEntry = null;
            foreach(FIB_Entry item in FIB_Table)
            {
                if (ipAddress.ToString().Equals(item.destinationIP) )
                {
                    fibEntry = item;
                    break;
                }
            }

            if (fibEntry == null)
            {
                Console.WriteLine("No fib found for IP  " + ipAddress );
            }

            return fibEntry;
        }

       
    }
}
