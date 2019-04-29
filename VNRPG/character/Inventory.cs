using GTANetworkAPI;
using VNRPG.model;
using VNRPG.globals;
using VNRPG.database;
using System.Collections.Generic;
using System.Linq;

namespace VNRPG.character
{
    public class Inventory : Script
    {
        public static void LoadDatabaseItems()
        {
            // Create the item list
            Globals.itemList = Database.LoadAllItems();

            // Get the objects on the ground
            List<ItemModel> groundItems = Globals.itemList.Where(it => it.ownerEntity == Constants.ITEM_ENTITY_GROUND).ToList();

            foreach (ItemModel item in groundItems)
            {
                // Get the hash from the object
                WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);
                uint hash = weaponHash == 0 ? uint.Parse(item.hash) : NAPI.Util.GetHashKey(Constants.WEAPON_ITEM_MODELS[weaponHash]);

                // Create each of the items on the ground
                item.objectHandle = NAPI.Object.CreateObject(hash, item.position, new Vector3(), 255, item.dimension);
            }
        }
    }
}
