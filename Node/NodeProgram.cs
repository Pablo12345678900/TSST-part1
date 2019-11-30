using System;
using System.Collections.Generic;
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

        private PackageHandler packageHandler;

        private short managerPort;
        private short cloudPort;

        public NodeProgram()
        {
            packageHandler = new PackageHandler();
        }
        
            
        static void Main(string[] args)
        {
            test();
        }

        static void test()
        {
            NodeProgram node = new NodeProgram();
            node.test1();
            node.test2();
            node.test3();
            node.test4();
            node.test5();
            node.test6();
            node.test7();
            node.test8();
            node.test9();
        }


        /*
         * test 1 - package from host ip 127.0.0.15  leaving with label = 5 and port 423
        */
        void test1()
        {
            Console.WriteLine("Test 1");
            
            packageHandler.FEC_Table.Add(new FEC_Entry(IPAddress.Parse("127.0.0.15"), 2));
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(1, "push", new List <int> {5}, 423,0));
            packageHandler.FTN_Table.Add(new FTN_Entry(1, 2));
            
            Package package = new Package();
            package.DestinationAddress = IPAddress.Parse("127.0.0.15");
            
            packageHandler.handlePackage(package);

            printPackageInfo(package); 
        }

        
        
        /*
        * test 2 - package sent between routers, swap label 1 to label 6
         * 
        */
        void test2()
        {
            Console.WriteLine("Test 2");
            
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(2, "swap", new List<int> {6}, 455,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(322,1,2));
            
            Package package = new Package();
            package.labelStack.labels.Push(new Label(1));
            package.Port = 322;
            packageHandler.handlePackage(package);
            
            
            printPackageInfo(package);  
        }
        
        /*
        * test 3 - penultimate hop of a path
         * Package coming with label 13, leaving with label 0 (meaning the tunnel is over)
        */
        void test3()
        {
            Console.WriteLine("Test 3");
            
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(3, "pop", new List<int> {0}, 555,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(499,13,3));

            Package package3 = new Package();
            package3.labelStack.labels.Push(new Label(13));
            package3.Port = 499;
            packageHandler.handlePackage(package3);

            
            printPackageInfo(package3);  
        }


        /*
        * test 4 - ultimate hop of a path
         * Package coming with label 0 (path finished) and is directed by FIB Table to port 480
        */
        void test4()
        {
            Console.WriteLine("Test 4");
            
            packageHandler.FIB_Table.Add(new FIB_Entry(IPAddress.Parse("123.123.123.123"), 480 ));

            Package package4 = new Package();
            package4.DestinationAddress = IPAddress.Parse("123.123.123.123");

            package4.labelStack.labels.Push(new Label(0));
            package4.Port = 499;
            packageHandler.handlePackage(package4);

            
            printPackageInfo(package4);        
        }



        /*
        * test 5 - adding multiple labels
         * Package coming with label 20, leaving with 6,4,7
        */
        void test5()
        {
            Console.WriteLine("Test 5");
                        
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(5, "push", new List<int> {6,4,7}, 578,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(511,20,5));

            Package package5 = new Package();
            package5.labelStack.labels.Push(new Label(20));
            package5.Port = 511;
            packageHandler.handlePackage(package5);

            
            printPackageInfo(package5);
        }

        /*
        * test 6 - penultimate hop of a nested tunnel
        * Package coming with labels 20, 21 and leaving with label 27, 0
        */
        void test6()
        {
            Console.WriteLine("Test 6");
            
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(6, "pop", new List<int> {27}, 627,1));
            packageHandler.ILM_Table.Add(new ILM_Entry(600,21,6));

            Package package6 = new Package();
            package6.labelStack.labels.Push(new Label(20));
            package6.labelStack.labels.Push(new Label(21));

            package6.Port = 600;
            packageHandler.handlePackage(package6);
            
            printPackageInfo(package6);
        }

        /*
        * Test 7 - ultimate hop of a nested tunnel
        * Package coming with labels 30, 0 and leaving with 35
        */
        
        void test7()
        {
            Console.WriteLine("Test 7");

            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(7, "swap", new List<int> {35}, 730,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(700,30,7));

            Package package7 = new Package();
            package7.labelStack.labels.Push(new Label(30));
            package7.labelStack.labels.Push(new Label(0));

            package7.Port = 700;
            packageHandler.handlePackage(package7);
            
            printPackageInfo(package7);

        }

        /*
        * test 8 - ultimate hop of a nested tunnel - multiple tunnels ending at the same time
         * Package entering with labels 40, 41, 42, leaving two tunnels with label 47
        */
        
        void test8()
        {
            Console.WriteLine("Test 8");
            
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(8, "swap", new List<int> {47}, 830,0));
            packageHandler.ILM_Table.Add(new ILM_Entry(800,40,8));

            Package package8 = new Package();
            package8.labelStack.labels.Push(new Label(40));
            package8.labelStack.labels.Push(new Label(0));
            package8.labelStack.labels.Push(new Label(0));

            package8.Port = 800;
            packageHandler.handlePackage(package8);
            
            printPackageInfo(package8);
        }
        
        /*
         * test 9 - penultimate hop of a nested tunnel - multiple tunnels ending at the same time
         * Package is about to exit two tunnels at the same time and change labels 57, 58, 59     to    51, 0, 0 
         */
        public void test9()
        {
            Console.WriteLine("Test 9");
            
            packageHandler.NHLFE_Table.Add(new NHLFE_Entry(9, "pop", new List<int> {50}, 910,2));
            packageHandler.ILM_Table.Add(new ILM_Entry(900,59,9));
            
            Package package9 = new Package();
            package9.labelStack.labels.Push(new Label(57));
            package9.labelStack.labels.Push(new Label(58));
            package9.labelStack.labels.Push(new Label(59));

            package9.Port = 900;
            packageHandler.handlePackage(package9);
            
            printPackageInfo(package9);
            
        }

        void printPackageInfo(Package package)
        {
            Console.Write("Labels: ");

            if (package.labelStack.labels != null)
            {
                int labelsNumber = package.labelStack.labels.Count;
                for (int i = 0; i < labelsNumber; i++)
                {
                    Console.Write(package.labelStack.labels.Pop().labelNumber + " ");
                }
            }

            Console.WriteLine("Port " + package.Port );
        }

    }
}