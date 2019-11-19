using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tools;
using System.Data;

namespace TSST
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public myHost hostSource { get; set; }
        public RestOfHosts hostdestination { get; set; }

        public int counterForMessageID = 0;
        public MultiSocket connectedSocket { get; set; }
        public enum LogType { Successful, Informative, Failure }
        public MainWindow()
        {
            var args = Environment.GetCommandLineArgs();
            InitializeComponent();
            try
            {
                if (args.Length > 1)
                {
                    hostSource = myHost.createHost(args[1]);
                    unableButton();
                    fillTheComboBox();
                }
            }
            catch(Exception e)
            {
                Environment.Exit(1);
            }
            Task.Run(() => GetConnectionWithCloud());
            
        }
        public void GetConnectionWithCloud()
        {
            Dispatcher.Invoke(() => AddLog("Attempt to connect with Cloud", LogType.Informative));
            try
            {
                connectedSocket = new MultiSocket(hostSource.cloudIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // Stream uses TCP protocol
                connectedSocket.Connect(new IPEndPoint(hostSource.cloudIP, hostSource.cloudPort)); //connect with server
                Task.Run(WaitForPackage); // after connecting with the cloud, host starts waiting for package
            }
            catch (SocketException e)
            {
                Dispatcher.Invoke(() => AddLog("Can't get connection with connection", LogType.Failure));
                Dispatcher.Invoke(() => AddLog("Connecting with Cloud one more time", LogType.Informative));
                Task.Run(GetConnectionWithCloud);
            }

        }
        public void WaitForPackage()
        {
            while (true) // host is waiting/listening for a package 
            {
                try
                {
                    Package package = connectedSocket.ReceivePackage();
                    // if we receive-> Add log in HostWindow
                }
                catch (SocketException e)
                {

                }
            }
        }

        public void fillTheComboBox()
        {
            comboBox1.Items.Clear();

            for(int i=0; i<hostSource.Neighbours.Count;i++)
            {
                comboBox1.Items.Add(hostSource.Neighbours[i]);
            }
        }
        public void sendPackage()
        {
            Package package = new Package();
            package.SourceAddress = hostSource.host_IP;

            package.TTL = package.TTL - 1;
            ++counterForMessageID;
            package.messageID = counterForMessageID;
            package.Port = hostSource.portOut;
            package.CurrentNode = hostSource.host_IP;

            var syncTask = new Task(() =>
            {
                package.payload = textBox1.Text;
                package.DestinationAddress = ((RestOfHosts)comboBox1.SelectedItem).ip;
            });
            syncTask.RunSynchronously();

            try
            {
                connectedSocket.Send(package.convertToBytes());
                Dispatcher.Invoke(() => AddLog($"Package has been sent: {package}", LogType.Successful));
                // add log that package has been sent
            }
            catch (SocketException e)
            {
               // Dispatcher.
            }
        }

        public void unableButton()
        {
            SendMessage.IsEnabled = (textBox1.Text != null && comboBox1.SelectedItem != null);
        }

        public void SendMessage_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                sendPackage();
                textBox1.Clear();
                comboBox1.SelectedItem = null;
                unableButton();
            });
            //throw new System.NotImplementedException();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hostdestination = (RestOfHosts)comboBox1.SelectedItem;
            unableButton();
        }
        public void AddLog(string info,  LogType logType)
        {
            // to complete
        }
    }
}
