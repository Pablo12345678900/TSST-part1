using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
   public class Label
    {
        //length of MPLS Label is 4 bytes where TTL is 1 byte 1 byte is informative and rest belongs to the Label
        public short label_Number { get; set; }
        public short TTL { get; set; }


        public Label()
        {

        }
        public byte[] GetLabelAsBytes()
        {
            List<byte> bytedLabel = new List<byte>();
            bytedLabel.AddRange(BitConverter.GetBytes(label_Number));
            bytedLabel.AddRange(BitConverter.GetBytes(TTL));
            return bytedLabel.ToArray();
        }
        public Label GetBytesAsLabel(byte[] bytes)
        {
            Label label = new Label();

            label.label_Number = (short)((bytes[1] << 8) + bytes[0]);
            label.TTL = (short)bytes[2];

            return label;
        }

    }
}
