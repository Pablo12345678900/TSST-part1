using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net; // used for class: IPAddress
namespace Tools
{
    public class Package //definition of Package
    {
        public LabelStack labelStack { get; set; }

        public ushort TTL; // Time-To-Live
        public  int package_length { get; set; }
        public string payload { get; set; }
        public int messageID { get; set; }

        public IPAddress SourceAddress { get; set; }
        public IPAddress DestinationAddress { get; set; }
        public ushort Port { get; set; } // port from/to which package has been sent/received
        public IPAddress CurrentNodeIP { get; set; } 
        public Package()
        {
            labelStack = new LabelStack();
            TTL = 255; // default value
            
        }

        public byte[] convertToBytes()
        {
            List<byte> package_in_Bytes = new List<byte>();
           // const int headerLength=24+label
           int headerLength = 24 + labelStack.GetLengthOfStack();
            package_length = payload.Length + headerLength;
            
            package_in_Bytes.AddRange(labelStack.GetStackInBytes()); //length of stack
            package_in_Bytes.AddRange(BitConverter.GetBytes(messageID));// 4 bytes
            package_in_Bytes.AddRange(BitConverter.GetBytes(package_length)); //4bytes
            package_in_Bytes.AddRange(BitConverter.GetBytes(TTL)); // 2 bytes
            package_in_Bytes.AddRange(SourceAddress.GetAddressBytes()); //4 bytes
            package_in_Bytes.AddRange(DestinationAddress.GetAddressBytes()); // 4 bytes
            package_in_Bytes.AddRange(BitConverter.GetBytes(Port)); // 2 bytes
            package_in_Bytes.AddRange(CurrentNodeIP.GetAddressBytes()); //4 bytes
            package_in_Bytes.AddRange(Encoding.ASCII.GetBytes(payload ?? "")); // length of payload

            //package_length = package_in_Bytes.Count;
            //Console.WriteLine("Package length: " +package_length);
            return package_in_Bytes.ToArray();

        }

        // package looks like : | length of label stack | message ID 1 BYTE | TTL 1 byte | Source Address 4 bytes | Dest Address 4 bytes | Port 2 bytes | CurrentNode 4 bytes| length of payload |
        public static Package returnToPackage(byte[] bytes)
        {
            Package myPackage = new Package();
            
            myPackage.labelStack = LabelStack.returnToStack(bytes);

            var length_of_stack = myPackage.labelStack.GetLengthOfStack();

            int headerLength = 24 + length_of_stack;
            //myPackage.package_length = headerLength + myPackage.payload.Length;
            myPackage.messageID = BitConverter.ToInt32(bytes, length_of_stack);
            myPackage.package_length = BitConverter.ToInt32(bytes, length_of_stack + 4);
            myPackage.TTL = (ushort)((bytes[length_of_stack+9]<<8)+bytes[length_of_stack+8]);
            myPackage.SourceAddress = new IPAddress(new byte[] { bytes[length_of_stack + 10], bytes[length_of_stack +11], bytes[length_of_stack + 12], bytes[length_of_stack + 13] });
            myPackage.DestinationAddress = new IPAddress(new byte[] { bytes[length_of_stack + 14], bytes[length_of_stack + 15], bytes[length_of_stack + 16], bytes[length_of_stack + 17] });
            myPackage.Port = (ushort)((bytes[length_of_stack + 19] << 8) + bytes[length_of_stack + 18]);
            myPackage.CurrentNodeIP = new IPAddress(new byte[] { bytes[length_of_stack + 20], bytes[length_of_stack + 21], bytes[length_of_stack + 22], bytes[length_of_stack + 23] });

            List<byte> listForPayload = new List<byte>();
            listForPayload.AddRange(bytes.ToList()
                .GetRange(length_of_stack + 24, myPackage.package_length - headerLength));

            myPackage.payload = Encoding.ASCII.GetString(listForPayload.ToArray());
            //Console.WriteLine("Your payload: " +myPackage.payload);
            return myPackage;
        }
        
        public void printInfo()
        {
            Console.WriteLine("Message Id: " + messageID + " Source: " + SourceAddress + " Destination: " +
                              DestinationAddress);
            Console.WriteLine("Payload: " + payload);
                
            if (labelStack.labels != null)
            {
                Console.Write( "Labels: ");
                foreach (Label label in labelStack.labels)
                {
                    Console.Write(label.labelNumber + " ");
                }
            }
            else
            {
                Console.Write("LabelStack empty");
            }
            
            Console.WriteLine("\n ");

        }

    }
}
