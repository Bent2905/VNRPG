using GTANetworkAPI;
using VNRPG.globals;
using VNRPG.model;
using VNRPG.vehicles;
using VNRPG.messages.general;
using VNRPG.messages.error;
using VNRPG.messages.information;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VNRPG.business
{
    public class CarShop : Script
    {
        private TextLabel carShopTextLabel;
        private TextLabel motorbikeShopTextLabel;
        private TextLabel shipShopTextLabel;

        private int GetClosestCarShop(Client player, float distance = 2.0f)
        {
            int carShop = -1;
            if (player.Position.DistanceTo(carShopTextLabel.Position) < distance)
            {
                carShop = 0;
            }
            else if (player.Position.DistanceTo(motorbikeShopTextLabel.Position) < distance)
            {
                carShop = 1;
            }
            else if (player.Position.DistanceTo(shipShopTextLabel.Position) < distance)
            {
                carShop = 2;
            }
            return carShop;
        }

        private List<CarShopVehicleModel> GetVehicleListInCarShop(int carShop)
        {
            // Get all the vehicles in the list
            return Constants.CARSHOP_VEHICLE_LIST.Where(vehicle => vehicle.carShop == carShop).ToList();
        }

        private int GetVehiclePrice(VehicleHash vehicleHash)
        {
            // Get the price of the vehicle
            CarShopVehicleModel carDealerVehicle = Constants.CARSHOP_VEHICLE_LIST.Where(vehicle => vehicle.hash == vehicleHash).FirstOrDefault();

            return carDealerVehicle == null ? 0 : carDealerVehicle.price;
        }

        private string GetVehicleModel(VehicleHash vehicleHash)
        {
            // Get the price of the vehicle
            CarShopVehicleModel carDealerVehicle = Constants.CARSHOP_VEHICLE_LIST.Where(vehicle => vehicle.hash == vehicleHash).FirstOrDefault();

            return carDealerVehicle == null ? string.Empty : carDealerVehicle.hash.ToString();
        }

        private bool SpawnPurchasedVehicle(Client player, List<Vector3> spawns, VehicleHash vehicleHash, int vehiclePrice, string firstColor, string secondColor)
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                // Check if the spawn point has a vehicle on it
                bool spawnOccupied = NAPI.Pools.GetAllVehicles().Where(veh => spawns[i].DistanceTo(veh.Position) < 2.5f).Any();

                if (!spawnOccupied)
                {
                    // Basic data for vehicle creation
                    VehicleModel vehicleModel = new VehicleModel();
                    {
                        vehicleModel.model = GetVehicleModel(vehicleHash);
                        vehicleModel.plate = string.Empty;
                        vehicleModel.position = spawns[i];
                        vehicleModel.rotation = new Vector3(0.0f, 0.0f, 218.0f);
                        vehicleModel.owner = player.GetData(EntityData.PLAYER_NAME);
                        vehicleModel.colorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
                        vehicleModel.firstColor = firstColor;
                        vehicleModel.secondColor = secondColor;
                        vehicleModel.pearlescent = 0;
                        vehicleModel.price = vehiclePrice;
                        vehicleModel.parking = 0;
                        vehicleModel.parked = 0;
                        vehicleModel.engine = 0;
                        vehicleModel.locked = 0;
                        vehicleModel.gas = 50.0f;
                        vehicleModel.kms = 0.0f;
                    }

                    // Creating the purchased vehicle
                    Vehicles.CreateVehicle(player, vehicleModel, false);

                    return true;
                }
            }

            return false;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            // Car dealer creation
            carShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Commands.COM_CATALOG, new Vector3(-215.6841f, 6219.168f, 31.49166f), 10.0f, 0.5f, 4, new Color(190, 235, 100));
            NAPI.TextLabel.CreateTextLabel(GenRes.catalog_help, new Vector3(-215.6841f, 6219.168f, 31.39166f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip carShopBlip = NAPI.Blip.CreateBlip(new Vector3(-215.6841f, 6219.168f, 31.49166f));
            carShopBlip.Name = GenRes.car_dealer;
            carShopBlip.ShortRange = true;
            carShopBlip.Sprite = 225;

            // Motorcycle dealer creation
            motorbikeShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Commands.COM_CATALOG, new Vector3(2129.325f, 4794.172f, 40.88499f), 10.0f, 0.5f, 4, new Color(190, 235, 100));
            NAPI.TextLabel.CreateTextLabel(GenRes.catalog_help, new Vector3(2129.325f, 4794.172f, 40.78499f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip motorbikeShopBlip = NAPI.Blip.CreateBlip(new Vector3(2129.325f, 4794.172f, 40.88499f));
            motorbikeShopBlip.Name = GenRes.motorcycle_dealer;
            motorbikeShopBlip.ShortRange = true;
            motorbikeShopBlip.Sprite = 226;

            // Boat dealer creation
            shipShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Commands.COM_CATALOG, new Vector3(1529.877f, 3778.535f, 34.51152f), 10.0f, 0.5f, 4, new Color(190, 235, 100));
            NAPI.TextLabel.CreateTextLabel(GenRes.catalog_help, new Vector3(1529.877f, 3778.535f, 34.41152f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip shipShopBlip = NAPI.Blip.CreateBlip(new Vector3(1529.877f, 3778.535f, 34.51152f));
            shipShopBlip.Name = GenRes.boat_dealer;
            shipShopBlip.ShortRange = true;
            shipShopBlip.Sprite = 455;
        }

        [RemoteEvent("purchaseVehicle")]
        public void PurchaseVehicleEvent(Client player, string hash, string firstColor, string secondColor)
        {
            int carShop = GetClosestCarShop(player);
            VehicleHash vehicleHash = (VehicleHash)uint.Parse(hash);
            int vehiclePrice = GetVehiclePrice(vehicleHash);

            if (vehiclePrice > 0 && player.GetSharedData(EntityData.PLAYER_BANK) >= vehiclePrice)
            {
                bool vehicleSpawned = false;

                switch (carShop)
                {
                    case 0:
                        // Create a new car
                        vehicleSpawned = SpawnPurchasedVehicle(player, Constants.CARSHOP_SPAWNS, vehicleHash, vehiclePrice, firstColor, secondColor);                        
                        break;
                    case 1:
                        // Create a new motorcycle
                        vehicleSpawned = SpawnPurchasedVehicle(player, Constants.BIKESHOP_SPAWNS, vehicleHash, vehiclePrice, firstColor, secondColor);
                        break;
                    case 2:
                        // Create a new ship
                        vehicleSpawned = SpawnPurchasedVehicle(player, Constants.SHIP_SPAWNS, vehicleHash, vehiclePrice, firstColor, secondColor);
                        break;
                }

                if(!vehicleSpawned)
                {
                    // Parking places are occupied
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.carshop_spawn_occupied);
                }
            }
            else
            {
                string message = string.Format(ErrRes.carshop_no_money, vehiclePrice);
                player.SendChatMessage(Constants.COLOR_ERROR + message);
            }
        }

        [RemoteEvent("testVehicle")]
        public void TestVehicleEvent(Client player, string hash)
        {
            // Check if the player is already testing a vehicle
            if(player.GetData(EntityData.PLAYER_TESTING_VEHICLE) != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_testing_vehicle);
                return;
            }

            Vehicle vehicle = null;
            Vector3 testFinishCheckpoint = null;
            VehicleHash vehicleModel = (VehicleHash)uint.Parse(hash);

            switch (GetClosestCarShop(player))
            {
                case 0:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(-238.6294f, 6196.433f, 31.48921f), 128.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = new Vector3(-239.7822f, 6231.539f, 30.70019f);
                    break;
                case 1:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(2150.572f, 4798.39f, 41.11817f), 180.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = new Vector3(2134.755f, 4777.85f, 40.97029f);
                    break;
                case 2:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(1352.241f, 3750.85f, 30.10234f), 180.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = new Vector3(1427.16f, 3761.225f, 30.42179f);
                    break;
            }

            // Vehicle variable initialization
            vehicle.SetData(EntityData.VEHICLE_KMS, 0.0f);
            vehicle.SetData(EntityData.VEHICLE_GAS, 50.0f);
            vehicle.SetData(EntityData.VEHICLE_TESTING, true);
            vehicle.SetSharedData(EntityData.VEHICLE_DOORS_STATE, NAPI.Util.ToJson(new List<bool> { false, false, false, false, false, false }));
            player.SetData(EntityData.PLAYER_TESTING_VEHICLE, vehicle);
            player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);
            vehicle.EngineStatus = true;

            // Adding the checkpoint
            player.SetData(EntityData.PLAYER_DRIVING_COLSHAPE, testFinishCheckpoint);
            player.TriggerEvent("showCarshopCheckpoint", testFinishCheckpoint);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_test_vehicle);
        }

        [RemoteEvent("deliverTestVehicle")]
        public void DeliverTestVehicleEvent(Client player)
        {
            // Get the current vehicle
            Vehicle vehicle = player.GetData(EntityData.PLAYER_TESTING_VEHICLE);

            if (player.Vehicle == vehicle)
            {
                // We destroy the vehicle
                player.WarpOutOfVehicle();
                vehicle.Delete();

                // Variable cleaning
                player.ResetData(EntityData.PLAYER_TESTING_VEHICLE);
                player.ResetData(EntityData.PLAYER_DRIVING_COLSHAPE);
            }
        }

        [Command(Commands.COM_CATALOG)]
        public void CatalogoCommand(Client player)
        {
            int carShop = GetClosestCarShop(player);

            if (carShop > -1)
            {
                // We get the vehicle list
                List<CarShopVehicleModel> carList = GetVehicleListInCarShop(carShop);

                // Getting the speed for each vehicle in the list
                foreach (CarShopVehicleModel carShopVehicle in carList)
                {
                    carShopVehicle.model = carShopVehicle.hash.ToString();
                    VehicleHash vehicleHash = NAPI.Util.VehicleNameToModel(carShopVehicle.model);
                    carShopVehicle.speed = (int)Math.Round(NAPI.Vehicle.GetVehicleMaxSpeed(vehicleHash) * 3.6f);
                }

                // We show the catalog
                player.TriggerEvent("showVehicleCatalog", NAPI.Util.ToJson(carList), carShop);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_carshop);
            }
        }
    }
}
