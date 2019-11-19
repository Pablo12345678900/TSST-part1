using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using Tools;
namespace CableCloud
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Cloud cableCloud { get; set; }



        public MainWindow()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                cableCloud = Cloud.createCloud(args[1]);
                InitializeComponent();
            }
            Task.Run(RunCloudServer);
        }
        public void RunCloudServer()
        {
            // cloud is waiting for events
            Socket connectedSocketServer = new Socket(cableCloud.cloudIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectedSocketServer.Bind(new IPEndPoint(cableCloud.cloudIp, cableCloud.cloudPort)); //cloud is the server
            
            while(true)
            {
                byte[] buffer = new byte[128];
                connectedSocketServer.Listen(1); 
                Socket handler=connectedSocketServer.Accept();
                handler.Receive(buffer);

                Package package = new Package();
                package = package.returnToPackage(buffer);
                IPAddress node1 = package.CurrentNode;
                int port1 = package.Port;
                //find the propoer end point of cable
                for (int i=0; i<cableCloud.cables.Count;i++)
                {
                    if((node1== cableCloud.cables[i].Node1 && port1==cableCloud.cables[i].port1))
                    {
                        if (cableCloud.cables[i].stateOfCable == "WORKING")
                        {
                            package.CurrentNode = cableCloud.cables[i].Node2;
                            package.Port = cableCloud.cables[i].port2;
                            break;
                        }
                        else
                        {
                            AddLog();
                        }
                    }
                    if(node1 == cableCloud.cables[i].Node2 && port1 == cableCloud.cables[i].port2)
                    {
                        package.CurrentNode = cableCloud.cables[i].Node1;
                        package.Port = cableCloud.cables[i].port1;
                        break;
                    }
                }
                 ((IPEndPoint)connectedSocketServer.RemoteEndPoint).Port = package.Port;
                 ((IPEndPoint)connectedSocketServer.RemoteEndPoint).Address = package.CurrentNode;
                 handler.Send(buffer);
            }
        }
        public void AddLog()
        {
            // to complete
        }
    }
}
