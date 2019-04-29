using GTANetworkAPI;
using System;

namespace VNRPG.model
{
    public class HouseIplModel
    {
        public string ipl { get; set; }
        public Vector3 position { get; set; }

        public HouseIplModel(string ipl, Vector3 position)
        {
            this.ipl = ipl;
            this.position = position;
        }
    }
}
