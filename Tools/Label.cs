using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
   public class Label
    {
        //length of MPLS Label is 4 bytes where TTL is 1 byte 1 byte is informative and rest belongs to the Label
        public int labelNumber { get; set; }

        public Label()
        {
        }
        
        public Label(int labelNumber)
        {
            this.labelNumber = labelNumber;
        }
        public byte[] GetLabelAsBytes()
        {
            List<byte> bytedLabel = new List<byte>();
            bytedLabel.AddRange(BitConverter.GetBytes(labelNumber));
            return bytedLabel.ToArray();
        }
        public Label GetBytesAsLabel(byte[] bytes)
        {
            Label label = new Label();

            label.labelNumber = (short)((bytes[1] << 8) + bytes[0]);

            return label;
        }

    }
}
