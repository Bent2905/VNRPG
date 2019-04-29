using System;

namespace VNRPG.model
{
    public class PermissionModel
    {
        public int playerId { get; set; }
        public string command { get; set; }
        public string option { get; set; }
    }
}
