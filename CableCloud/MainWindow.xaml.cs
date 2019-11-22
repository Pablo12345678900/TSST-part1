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
            byte[] buffor = new byte[128];
            while(true)
            {
                connectedSocketServer.Listen(50); // max number of connections is 50, waiting for actions
                Socket handler=connectedSocketServer.Accept();
                handler.Receive(buffor);

              //  Package package = new Package();
                //package = package.returnToPackage(buffor);


            }


        }
        public void ReceiveAndSendPacket()
        {
            MultiSocket receiver = new MultiSocket(cableCloud.cloudIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Package recPackage = receiver.ReceivePackage();
        }
    }
}
