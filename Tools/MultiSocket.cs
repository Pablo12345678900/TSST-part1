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
            // modyfikujemy coś tutaj?
        }
        public int SendPackage(Package package)
        {
            return Send(package.convertToBytes());
        }
        public Package ReceivePackage()
        {
            byte[] buffer = new byte[128];
            Receive(buffer);
            Package rec = new Package();
            return rec.returnToPackage(buffer);
        }
    }
}
