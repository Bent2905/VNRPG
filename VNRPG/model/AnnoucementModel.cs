using System;

namespace VNRPG.model
{
    public class AnnoucementModel
    {
        public int id { get; set; }
        public int winner { get; set; }
        public int journalist { get; set; }
        public int amount { get; set; }
        public string annoucement { get; set; }
        public bool given { get; set; }
    }
}
