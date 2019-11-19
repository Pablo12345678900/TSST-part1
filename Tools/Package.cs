using System;
using System.Collections.Generic;
using System.Text;
using System.Net; // used for class: IPAddress
namespace Tools
{
    public class Package //definition of Package
    {
        public LabelStack labelStack { get; set; }

        public int TTL; // Time-To-Live
        public  int package_length { get; set; }
        public string payload { get; set; }
        public int messageID { get; set; }

        public IPAddress SourceAddress { get; set; }
        public IPAddress DestinationAddress { get; set; }
        // source adrress.length=4 bytes=destination    message_ID=1byte  TTL=1 byte  port=2 bytes labelstack=4bytes
        public const int headerLength = 16;
        public int Port { get; set; } // port from/to which package has been sent/received
        public IPAddress CurrentNode { get; set; } 
        // port and node name are being changed in cable cloud

        public Package()
        {
            labelStack = new LabelStack();
            TTL = 255; // default value

        }

        public byte[] convertToBytes()
        {
            List<byte> package_in_Bytes = new List<byte>();

            package_length = payload.Length + headerLength;

            package_in_Bytes.AddRange(labelStack.GetStackInBytes());
            package_in_Bytes.AddRange(BitConverter.GetBytes(messageID));
           // package_in_Bytes.AddRange(BitConverter.GetBytes(package_length));
            package_in_Bytes.AddRange(BitConverter.GetBytes(TTL));
            package_in_Bytes.AddRange(SourceAddress.GetAddressBytes());
            package_in_Bytes.AddRange(DestinationAddress.GetAddressBytes());
            package_in_Bytes.AddRange(BitConverter.GetBytes(Port));
            package_in_Bytes.AddRange(CurrentNode.GetAddressBytes());
            package_in_Bytes.AddRange(Encoding.ASCII.GetBytes(payload ?? "")); // if payload is null, return ""

            return package_in_Bytes.ToArray();

        }

        // package looks like : | length of label stack | message ID 1 BYTE | TTL 1 byte | Source Address 4 bytes | Dest Address 4 bytes | Port 2 bytes | CurrentNode 4 bytes| length of payload |
        public  Package returnToPackage(byte[] bytes)
        {
            Package my_Package = new Package();
            my_Package.labelStack = labelStack.returnToStack(bytes);
            var length_of_stack = my_Package.labelStack.GetLengthOfStack();

            my_Package.messageID = BitConverter.ToInt32(bytes, length_of_stack);
            my_Package.TTL = BitConverter.ToInt32(bytes, length_of_stack + 1);
            my_Package.SourceAddress = new IPAddress(new byte[] { bytes[length_of_stack + 2], bytes[length_of_stack + 3], bytes[length_of_stack + 4], bytes[length_of_stack + 5] });
            my_Package.DestinationAddress = new IPAddress(new byte[] { bytes[length_of_stack + 6], bytes[length_of_stack + 7], bytes[length_of_stack + 8], bytes[length_of_stack + 9] });
            my_Package.Port = (int)((bytes[length_of_stack + 11] << 8) + bytes[length_of_stack + 10]);
            my_Package.CurrentNode = new IPAddress(new byte[] { bytes[length_of_stack + 12], bytes[length_of_stack + 13], bytes[length_of_stack + 14], bytes[length_of_stack + 15] });
            List<byte> listForPayload = new List<byte>();
            for(int i=0; i<my_Package.package_length-headerLength;i++)
            {
                listForPayload.Add(bytes[package_length - (i + 1)]);
            }

            my_Package.payload = Encoding.ASCII.GetString(listForPayload.ToArray());

            return my_Package;
        }

    }
}
