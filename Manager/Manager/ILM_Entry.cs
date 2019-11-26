namespace Manager
{
    public class ILM_Entry
    {
        public int portIn { get; set; }
        public int labelIn { get; set; }
        public int NHLFE_ID { get; set; }

        public ILM_Entry(int portIn, int labelIn, int nhlfeId)
        {
            this.portIn = portIn;
            this.labelIn = labelIn;
            NHLFE_ID = nhlfeId;
        }
        public ILM_Entry() { }
    }
}