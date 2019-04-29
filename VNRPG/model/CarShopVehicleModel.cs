using GTANetworkAPI;

namespace VNRPG.model
{
    public class CarShopVehicleModel
    {
        public VehicleHash hash { get; set; }
        public int carShop { get; set; }
        public int type { get; set; }
        public int speed { get; set; }
        public int price { get; set; }
        public string model { get; set; }

        public CarShopVehicleModel(VehicleHash hash, int carShop, int type, int price)
        {
            this.hash = hash;
            this.carShop = carShop;
            this.type = type;
            this.price = price;
        }
    }
}
