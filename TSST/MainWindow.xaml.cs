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
using System.Threading;

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
            Task.Run(GetConnectionWithCloud);
            
        }
         
         
         
        public void GetConnectionWithCloud()
        {
            //ListBox12.Items.Add("Trying getting connection");
            try
            {
                //ListBox12.Items.Add("Dupa");
                
                connectedSocket = new MultiSocket(hostSource.cloudIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // Stream uses TCP protocol
                connectedSocket.Connect(new IPEndPoint(hostSource.cloudIP, hostSource.cloudPort)); //connect with server
                connectedSocket.Send(Encoding.ASCII.GetBytes("First Message " + hostSource.host_IP.ToString()));

            }
            catch (SocketException e)
            {
                ListBox12.Items.Add("Cant get connection");
                Task.Run(GetConnectionWithCloud);
            }
          
            Task.Run(WaitForPackage);

        }
        public void WaitForPackage()
        {
            //ListBox12.Items.Add("DUPA");
            int i = 0;
            while (true) // host is waiting/listening for a package 
            {
                
                try
                {
                    Dispatcher.Invoke(() =>ListBox12.Items.Add(i));
                    i += 1;
                    Package package= connectedSocket.ReceivePackage();
                    Dispatcher.Invoke(() => ListBox12.Items.Add(package.payload));
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

                package.TTL = (ushort) (package.TTL - 1);
                ++counterForMessageID;
                package.messageID = counterForMessageID;
                package.Port = hostSource.portOut;
                package.CurrentNodeIP = hostSource.host_IP;
                //ListBox12.Items.Add("Give attributes...");
                Dispatcher.Invoke(() =>
                {
                    package.payload = textBox1.Text;
                    package.DestinationAddress = ((RestOfHosts) comboBox1.SelectedItem).ip;
                });
                    
               
            
            try
            {
                    Dispatcher.Invoke(() => ListBox12.Items.Add("TRY SEND"));
                    connectedSocket.SendPackage(package);
                    Dispatcher.Invoke(() => ListBox12.Items.Add("Package sent"));
                    
            }
            catch (Exception e)
                {
                    ListBox12.Items.Add("Problem with sending");
                }
            
        }

        public void unableButton()
        {
            SendMessage.IsEnabled = (comboBox1.SelectedItem != null);
        }

        public  void SendMessage_Click(object sender, EventArgs e)
        {
           // ListBox12.Items.Add("Clicked");
          
                sendPackage();
                comboBox1.SelectedItem = null;
                textBox1.Text = null;
                unableButton();
                //throw new System.NotImplementedException();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hostdestination = (RestOfHosts)comboBox1.SelectedItem;
            unableButton();
        }
       
    }
}
