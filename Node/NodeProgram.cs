using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Tools;
using Tools.Table_Entries;

namespace Node
{
    class NodeProgram
    {
        
        private string nodeName { get; set; }
        private IPAddress ipAddress { get; set; }
        private IPAddress cloudIpAddress { get; set; }
        private IPAddress managerIpAddress { get; set; }

        static PackageHandler packageHandler;

        private short managerPort;
        private short cloudPort;
        
            
        static void Main(string[] args)
        {
         Router router;
         try
         {
           router=Router.createRouter(args[1]);
         }
         catch (Exception e)
         {
          Console.WriteLine(e);
          throw;
         }
         router.ActivateRouter();
            

        }

        void test()
        {
//            //wczytywanie pliku csv
//            ArrayList NHLFE_Table = new ArrayList();
//            using(var reader = new StreamReader("/Users/Pawel/Documents/Pawel/Studia/sem 5/TSST/NHLFE_Table.csv"))
//            {
//                var line = reader.ReadLine();
//                
//                while (!reader.EndOfStream)
//                {
//                    line = reader.ReadLine();
//                    Console.WriteLine(line);
//                    var values = line.Split(';');
//                    NHLFE_Entry entry = new NHLFE_Entry(Convert.ToInt32(values[0]), values[1],new List <int> {Convert.ToInt32(values[2])}, Convert.ToInt32(values[3]) );
//                    NHLFE_Table.Add(entry);
//                }
//            }
//            
//
//            foreach (NHLFE_Entry item in NHLFE_Table)
//            {
//                Console.WriteLine(item.NHLFE_ID + " " + item.action + " " + item.labelsOut[0] + " " + item.portOut);
//            }
            
            // TEST CONFIGURATIONS
            //test1 
            packageHandler = new PackageHandler();
            packageHandler.FEC_Table.Add(new FEC_Entry(new IPAddress(123123), 2));
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(1, "push", new List <ushort> {5}, 423,0));
            packageHandler.FTN_Table.Add(new FTN_Entry(1, 2));
            
            //test 2
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(2, "swap", new List<ushort> {6}, 455,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(322,1,2));

            //test3
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(3, "pop", new List<ushort> {0}, 555,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(499,13,3));
            
            //test4
            packageHandler.FIB_Table.Add(new FIB_Entry(new IPAddress(123456), 480 ));
            
            //test 5
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(5, "push", new List<ushort> {6,4,7}, 578,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(511,20,5));
            
            //test 6
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(6, "pop", new List<ushort> {0}, 627,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(600,21,6));
            
            //test 7
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(7, "swap", new List<ushort> {30}, 730,1));
            packageHandler.ILM_Table.Add(new ILM_Entry(700,31,7));
            
            //test 8
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(8, "swap", new List<ushort> {30}, 830,2));
            packageHandler.ILM_Table.Add(new ILM_Entry(800,40,8));
            
            
            /*
             * test 1 - package from host
            */
            Package package1 = new Package();
            package1.DestinationAddress = new IPAddress(123123);
            
            packageHandler.handlePackage(package1);
            
            Console.WriteLine( "Test 1:   Label: " + package1.labelStack.labels.Peek().labelNumber + " Port " + package1.Port );

            
            /*
             * test 2 - package sent between routers, swap
             */
            Package package = new Package();
            Label label = new Label();
            label.labelNumber = 1;
            package.labelStack.labels.Push(label);
            package.Port = 322;
            packageHandler.handlePackage(package);
            
            Console.WriteLine( "Test 2:   Label: " + package.labelStack.labels.Peek().labelNumber + " Port " + package.Port );
            
            /*
             * test 3 - penultimate hop of a path
             */
            Package package3 = new Package();
            Label label3 = new Label();
            label3.labelNumber = 13;
            package3.labelStack.labels.Push(label3);
            package3.Port = 499;
            packageHandler.handlePackage(package3);
            
            Console.WriteLine( "Test 3:   Label: " + package3.labelStack.labels.Peek().labelNumber + " Port " + package3.Port );
            
            /*
             * test 4 - ultimate hop of a path
             */
            Package package4 = new Package();
            package4.DestinationAddress = new IPAddress(123456); //IPAddress.parse("123.123.123.123")

            Label label4 = new Label();
            label4.labelNumber = 0;
            package4.labelStack.labels.Push(label4);
            package4.Port = 499;
            packageHandler.handlePackage(package4);
            
            Console.WriteLine( "Test 4:   Label: " + " Port " + package4.Port );
            
            
            /*
             * test 5 - adding more labels
             */
            Package package5 = new Package();
            Label label5 = new Label();
            
            label5.labelNumber = 20;
            package5.labelStack.labels.Push(label5);
            package5.Port = 511;
            packageHandler.handlePackage(package5);
            
            Console.WriteLine( "Test 5:   Labels: " +package5.labelStack.labels.Pop().labelNumber + " " + package5.labelStack.labels.Pop().labelNumber + " " + package5.labelStack.labels.Pop().labelNumber+ " Port " + package5.Port );
            
            /*
             * test 6 - penultimate hop of a nested tunnel
             */
            Package package6 = new Package();
            Label label6 = new Label();
            label6.labelNumber = 20;
            package6.labelStack.labels.Push(label6);
            Label label61 = new Label();
            label61.labelNumber = 21;
            package6.labelStack.labels.Push(label61);

            package6.Port = 600;
            packageHandler.handlePackage(package6);
            
            Console.WriteLine( "Test 6:   Labels: " + package6.labelStack.labels.Pop().labelNumber + " " + package6.labelStack.labels.Pop().labelNumber+ " Port " + package6.Port );

            /*
             * test 7 - ultimate hop of a nested tunnel
             */
            Package package7 = new Package();
            Label label7 = new Label();
            label7.labelNumber = 30;
            package7.labelStack.labels.Push(label7);
            Label label71 = new Label();
            label71.labelNumber = 31;
            package7.labelStack.labels.Push(label71);

            package7.Port = 700;
            packageHandler.handlePackage(package7);
            
            Console.WriteLine( "Test 7:   Labels: " + package7.labelStack.labels.Pop().labelNumber +  " Port " + package7.Port );
            
            
            /*
             * test 8 - ultimate hop of a nested tunnel - multiple tunnels ending at the same time
             */
            Package package8 = new Package();
            Label label8 = new Label();
            label8.labelNumber = 40;
            package8.labelStack.labels.Push(label8);
            Label label81 = new Label();
            label71.labelNumber = 41;
            package8.labelStack.labels.Push(label81);
            Label label82 = new Label();
            label82.labelNumber = 42;
            package8.labelStack.labels.Push(label82);

            package8.Port = 800;
            packageHandler.handlePackage(package8);
            
            Console.WriteLine( "Test 8:   Labels: " + package8.labelStack.labels.Pop().labelNumber +  " Port " + package7.Port );

        }
    }
}