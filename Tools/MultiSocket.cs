using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Tools
{
   public class MultiSocket : Socket 
    {
        public MultiSocket(AddressFamily addressFamily,SocketType socketType, ProtocolType protocolType) : base(addressFamily,socketType, protocolType)
        {
            ReceiveTimeout = 5000000; // 5 seconds wait for package
        }
        public int SendPackage(Package package)
        {
            return Send(package.convertToBytes());
        }
        public Package ReceivePackage()
        {
            byte[] buffer = new byte[128];
            Receive(buffer);
            //Package rec = new Package();
            return Package.returnToPackage(buffer);
        }
    }
}
