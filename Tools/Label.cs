using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
   public class Label
    {
        //length of MPLS Label is 4 bytes where TTL is 1 byte 1 byte is informative and rest belongs to the Label
        public ushort labelNumber { get; set; }
        public short TTL { get; set; }


        public Label()
        {

        }
        public Label( ushort labelNumber)
        {
            this.labelNumber = labelNumber;
        }
        public byte[] GetLabelAsBytes()
        {
            List<byte> bytedLabel = new List<byte>();
            bytedLabel.AddRange(BitConverter.GetBytes(labelNumber));
            bytedLabel.AddRange(BitConverter.GetBytes(TTL));
            Console.WriteLine(bytedLabel.ToArray().Length);
            return bytedLabel.ToArray();
        }
        public Label GetBytesAsLabel(byte[] bytes)
        {
            Label label = new Label();

            label.labelNumber = (ushort)((bytes[1] << 8) + bytes[0]);
            label.TTL = (short)bytes[2];

            return label;
        }

    }
}
