using System;
using System.Collections.Generic;
using System.Text;


namespace Manager
{
    public class R_config
    {
         public String R_name;

        public List<FEC_Entry_c> FEC;
        public List<FIB_Entry_c> FIB;
        public List<FTN_Entry> FTN;
        public List<ILM_Entry> ILM;
        public List<NHLFE_Entry> NHLFE;

        public R_config()
        {

            FEC = new List<FEC_Entry_c>();
            FIB = new List<FIB_Entry_c>();
            FTN = new List<FTN_Entry>();
            ILM = new List<ILM_Entry>();
            NHLFE = new List<NHLFE_Entry>();
        
        }
    }
}
