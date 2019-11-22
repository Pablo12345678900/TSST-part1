using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public class LabelStack
    {
        public Stack<Label> labels { get; set; }

        public const byte stackEmpty = 0x00;
        public const byte stackNotEmpty = 0xff;

        public LabelStack()
        {
            labels = new Stack<Label>();
        }
        

        public byte[] GetStackInBytes()
        {
            List<byte> bytes = new List<byte>();
            byte flag;
            if (this.Empty())
                flag = stackEmpty;
            else
                flag = stackNotEmpty;


            bytes.Add(flag);
            
            for(int i=0; i<labels.Count;i++)
            {
                bytes.AddRange((labels.ToArray())[i].GetLabelAsBytes());
                if (i != labels.Count - 1)
                    bytes.Add(0x00);
                else
                    bytes.Add(0xff);  // end of stack
            }
            return bytes.ToArray();
        }
        public LabelStack returnToStack(byte [] bytes)
        {
            LabelStack my_Stack = new LabelStack();
            int id = 1;
            if (bytes[0] == stackNotEmpty) // look at flag
            {

                while (true)
                {
                    Label label = new Label //construction of label: | label_Number 2 bytes | TTL 1 byte | INFO 1 byte |
                    {
                        labelNumber = (short)((bytes[id + 1] << 8) + bytes[id]),
                    };

                    my_Stack.labels.Push(label);
                    if (bytes[id + 3] == 0xff) // check the bottom of Stack
                        break;

                    id = id + 4; // size of label=4 bytes
                }
            }   
            
            return my_Stack;

        }
        public bool Empty()
        {
            if (labels.Count == 0)
                return true;
            else
                return false;
        }
        public int GetLengthOfStack() // in bytes
        {
            if(this.Empty())
            {
                return 1;
            }
            else
            {
                return 1 + labels.Count * 4;
            }
        }
    }
}
