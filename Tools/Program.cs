using System;

namespace Tools
{
    public class Program
    {
       public static void Main(string[] args)
        {
            Label label=new Label();
            label.labelNumber = 5;
            label.TTL = 2;
            label.GetLabelAsBytes();
        }
    }
}
