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
using System.IO;
using System.Threading;

namespace CableCloud
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    ///
    //
    /// </summary>
// State object for reading client data asynchronously  
    public class StateObject {  
        // Client  socket.  
        public Socket workSocket = null;  
        // Size of receive buffer.  
        public const int BufferSize = 1024;  
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];  
// Received data string.  
        public StringBuilder sb = new StringBuilder();    
    } 
    public partial class MainWindow : Window
    {
        public ManualResetEvent thread1=new ManualResetEvent(false);
        public Cloud cableCloud { get; set; }
        public Socket socketServer { get; set; }
        public Dictionary<Socket, IPAddress> SocketFromIP=new Dictionary<Socket, IPAddress>();
        public Dictionary<IPAddress,Socket> IPFromSocket=new Dictionary<IPAddress, Socket>();
        public MainWindow()
        {
            var args = Environment.GetCommandLineArgs();
            InitializeComponent();
            try
            {

                Console.WriteLine("Creating...");

                cableCloud = Cloud.createCloud("DataForCloud.txt");

            }
            catch (Exception e)
            {
                Console.WriteLine("Failure, wrong arguments");
                Environment.Exit(1);
            }

            Task.Run(RunCloudServer);
        }

        public void RunCloudServer()
        {
            // cloud is waiting for events            

            socketServer = new Socket(cableCloud.cloudIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Run Server1 " + cableCloud.cloudIp);
            
            //Logs.Items.Add("Run Server2" + cableCloud.cloudIp + " " + cableCloud.cloudPort);
            try
            {
                socketServer.Bind(new IPEndPoint(cableCloud.cloudIp, cableCloud.cloudPort)); //cloud is the server
                
                Console.WriteLine("Binded");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            socketServer.Listen(50);
            while (true)
            {
                thread1.Reset();
                Console.WriteLine("Waiting for connection...");
                socketServer.BeginAccept(new AsyncCallback(AcceptCallBack), socketServer);
                thread1.WaitOne();
            }
            
        }



        public void AcceptCallBack(IAsyncResult asyncResult)
        {
            thread1.Set();
            Socket listener = (Socket) asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);
            StateObject stateObject=new StateObject();
            stateObject.workSocket = handler;
            
            handler.BeginReceive(stateObject.buffer,0, stateObject.buffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallBack), stateObject );
            
        }

        public void ReceiveCallBack(IAsyncResult asyncResult)
        {
           StateObject state = (StateObject) asyncResult.AsyncState;
           Socket handler = state.workSocket; //socket of client
           int ReadBytes;
           try
           {
                ReadBytes=handler.EndReceive(asyncResult);
           }
           catch (Exception e)
           {
               Console.WriteLine(e);
               throw;
           }

           state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, ReadBytes));
           var message = state.sb.ToString().Split(' ');
           // first message must be send to get information about connected socket: First Message <Ip address>
           if (message[0].Equals("First") && message[1].Equals("Message"))
           {
               IPAddress node=IPAddress.Parse(message[2]);
               SocketFromIP.TryAdd(handler, node);
               IPFromSocket.TryAdd(node, handler);
               Console.WriteLine("Everything is correct, connection from " + node.ToString());
           }
           else
           {
               ForwardPackage(state, handler, asyncResult);
           }

           state.sb.Clear();
           handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
        }

        public void ForwardPackage(StateObject stateObject, Socket handler, IAsyncResult asyncResult)
        {
            Package recPackage=Package.returnToPackage(stateObject.buffer);
            IPAddress node1 = SocketFromIP[handler];
            ushort port1 = recPackage.Port;
            Console.WriteLine(node1.ToString() + " " +port1.ToString());
            for (int i = 0; i < cableCloud.cables.Count; i++)
            {
                if ((cableCloud.cables[i].Node1.Equals(node1) &&  cableCloud.cables[i].port1.Equals(port1)))
                {

                    recPackage.CurrentNodeIP = cableCloud.cables[i].Node2;
                    recPackage.Port = cableCloud.cables[i].port2;
                    Socket socket = IPFromSocket[recPackage.CurrentNodeIP];
                    socket.BeginSend(recPackage.convertToBytes(), 0, recPackage.convertToBytes().Length, 0,
                        new AsyncCallback(SendCallBack), socket);
                    Console.WriteLine("Found cable and sent " + cableCloud.cables[i].Node2.ToString() + " " +cableCloud.cables[i].port2.ToString());
                    break;


                }

                if ( cableCloud.cables[i].Node2.Equals(node1) &&   cableCloud.cables[i].port2.Equals(port1))
                {
                    recPackage.CurrentNodeIP = cableCloud.cables[i].Node1;
                    recPackage.Port = cableCloud.cables[i].port1;
                    Socket socket = IPFromSocket[recPackage.CurrentNodeIP];
                    socket.BeginSend(recPackage.convertToBytes(), 0, recPackage.convertToBytes().Length, 0,
                        new AsyncCallback(SendCallBack), socket);
                    Console.WriteLine("Found cable and sent " + cableCloud.cables[i].Node1.ToString() + " " +cableCloud.cables[i].port1.ToString());
                    break;
                }
            }
            
        }

        public void SendCallBack(IAsyncResult asyncResult)
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)asyncResult.AsyncState;
            // Complete sending the data to the remote device.  
            handler.EndSend(asyncResult);
        }
    }
    
}

    

