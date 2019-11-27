using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tools;
using Tools.Table_Entries;

namespace Node
{
    public class PackageHandler
    {
        public List<NHLFE_Entry> NHLFE_Table { get; set; }
        public List<ILM_Entry> ILM_Table { get; set; }
        public List<FTN_Entry> FTN_Table { get; set; }
        public List<FEC_Entry> FEC_Table { get; set; }
        public List<FIB_Entry> FIB_Table { get; set; }

        public PackageHandler()
        {
            NHLFE_Table = new List<NHLFE_Entry>();
            ILM_Table = new List<ILM_Entry>();
            FTN_Table = new List<FTN_Entry>();
            FEC_Table = new List<FEC_Entry>();
            FIB_Table = new List<FIB_Entry>();
        }

        public  void handlePackage(Package package)
        {
            NHLFE_Entry nhlfeEntry = null;
            FIB_Entry fibEntry = null;
            
            if (package.labelStack.labels.Any())                         //check if stack has any elements
            {
                if (package.labelStack.labels.Peek().labelNumber == 0)    // check if i'm last hop
                {
                    while (package.labelStack.labels.Peek().labelNumber == 0)
                    {
                        package.labelStack.labels.Pop();
                        if (!package.labelStack.labels.Any())
                        {
                            break;
                        }
                    }

                    if (!package.labelStack.labels.Any())
                    {
                        fibEntry = findFibEntry(package.DestinationAddress);
                        package.Port = fibEntry.portOut;
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
                    Console.WriteLine("NHLFE " + ftnEntry.NHLFE_ID);

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

                package.Port = nhlfeEntry.portOut;
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
                        for (int i = 0; i < nhlfeEntry.popDepth; i++)
                        {
                            package.labelStack.labels.Pop();
                        }
                        
                        for (int i = 0; i < nhlfeEntry.popDepth; i++)
                        {
                            Label newLabel = new Label();
                            newLabel.labelNumber = 0;
                            package.labelStack.labels.Push(newLabel);
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
                package.Port = fibEntry.portOut;
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
                if (destinationAddress.Equals(item.destinationIP))
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
                if (ipAddress.Equals(item.destinationIP) )
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