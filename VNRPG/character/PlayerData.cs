using GTANetworkAPI;
using VNRPG.model;
using VNRPG.globals;
using VNRPG.house;
using VNRPG.vehicles;
using VNRPG.parking;
using VNRPG.messages.general;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VNRPG.character
{
    public class PlayerData : Script
    {
        [RemoteEvent("retrieveBasicData")]
        public static void RetrieveBasicDataEvent(Client player, int targetId)
        {
            // Get the target player
            Client target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the basic data
            string age = target.GetData(EntityData.PLAYER_AGE) + GenRes.years;
            string sex = target.GetData(EntityData.PLAYER_SEX) == Constants.SEX_MALE ? GenRes.sex_male : GenRes.sex_female;
            string money = target.GetSharedData(EntityData.PLAYER_MONEY) + "$";
            string bank = target.GetSharedData(EntityData.PLAYER_BANK) + "$";
            string job = GenRes.unemployed;
            string rank = string.Empty;

            // Get the job
            JobModel jobModel = Constants.JOB_LIST.Where(j => target.GetData(EntityData.PLAYER_JOB) == j.job).First();

            if (jobModel.job == 0)
            {
                // Get the player's faction
                FactionModel factionModel = Constants.FACTION_RANK_LIST.Where(f => target.GetData(EntityData.PLAYER_FACTION) == f.faction && target.GetData(EntityData.PLAYER_RANK) == f.rank).First();

                if (factionModel.faction > 0)
                {
                    switch (factionModel.faction)
                    {
                        case Constants.FACTION_POLICE:
                            job = GenRes.police_faction;
                            break;
                        case Constants.FACTION_EMERGENCY:
                            job = GenRes.emergency_faction;
                            break;
                        case Constants.FACTION_NEWS:
                            job = GenRes.news_faction;
                            break;
                        case Constants.FACTION_TOWNHALL:
                            job = GenRes.townhall_faction;
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            job = GenRes.transport_faction;
                            break;
                        case Constants.FACTION_SHERIFF:
                            job = GenRes.sheriff_faction;
                            break;
                    }

                    // Set player's rank
                    rank = target.GetData(EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                }
            }
            else
            {
                // Set the player's job
                job = target.GetData(EntityData.PLAYER_SEX) == Constants.SEX_MALE ? jobModel.descriptionMale : jobModel.descriptionFemale;
            }

            // Show the data for the player
            player.TriggerEvent("showPlayerData", target.Value, target.Name, age, sex, money, bank, job, rank, player == target || player.GetData(EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE);
        }

        [RemoteEvent("retrievePropertiesData")]
        public static void RetrievePropertiesDataEvent(Client player, int targetId)
        {
            // Initialize the variables
            List<string> houseAddresses = new List<string>();
            string rentedHouse = string.Empty;

            // Get the target player
            Client target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the houses where the player is the owner
            List<HouseModel> houseList = House.houseList.Where(h => h.owner == target.Name).ToList();

            foreach (HouseModel house in houseList)
            {
                // Add the name of the house to the list
                houseAddresses.Add(house.name);
            }

            if (target.GetData(EntityData.PLAYER_RENT_HOUSE) > 0)
            {
                // Get the name of the rented house
                int houseId = target.GetData(EntityData.PLAYER_RENT_HOUSE);
                rentedHouse = House.houseList.Where(h => h.id == houseId).First().name;
            }

            // Show the data for the player
            player.TriggerEvent("showPropertiesData", NAPI.Util.ToJson(houseAddresses), rentedHouse);
        }

        [RemoteEvent("retrieveVehiclesData")]
        public static void RetrieveVehiclesDataEvent(Client player, int targetId)
        {
            // Initialize the variables
            List<string> ownedVehicles = new List<string>();
            List<string> lentVehicles = new List<string>();

            // Get the target player
            Client target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the vehicles in the game
            List<Vehicle> vehicles = NAPI.Pools.GetAllVehicles().Where(v => Vehicles.HasPlayerVehicleKeys(target, v)).ToList();
            List<ParkedCarModel> parkedVehicles = Parking.parkedCars.Where(v => Vehicles.HasPlayerVehicleKeys(target, v.vehicle)).ToList();
            
            foreach (Vehicle vehicle in vehicles)
            {
                // Get the vehicle name
                string vehicleName = vehicle.Model.ToString() + " LS-" + (vehicle.GetData(EntityData.VEHICLE_ID) + 1000);

                if (vehicle.GetData(EntityData.VEHICLE_OWNER) == target.Name)
                {
                    // Add the the owned vehicles
                    ownedVehicles.Add(vehicleName);
                }
                else
                {
                    // Add the the lent vehicles
                    lentVehicles.Add(vehicleName);
                }
            }

            foreach (ParkedCarModel parkedVehicle in parkedVehicles)
            {
                // Get the vehicle name
                string vehicleName = parkedVehicle.vehicle.model.ToString() + " LS-" + (parkedVehicle.vehicle.id + 1000);

                if (parkedVehicle.vehicle.owner == target.Name)
                {
                    // Add the the owned vehicles
                    ownedVehicles.Add(vehicleName);
                }
                else
                {
                    // Add the the lent vehicles
                    lentVehicles.Add(vehicleName);
                }
            }

            // Show the data for the player
            player.TriggerEvent("showVehiclesData", NAPI.Util.ToJson(ownedVehicles), NAPI.Util.ToJson(lentVehicles));
        }

        [RemoteEvent("retrieveExtendedData")]
        public static void RetrieveExtendedDataEvent(Client player, int targetId)
        {
            // Get the target player
            Client target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the played time
            TimeSpan played = TimeSpan.FromMinutes(player.GetData(EntityData.PLAYER_PLAYED));
            string playedTime = Convert.ToInt32(played.TotalHours) + "h " + Convert.ToInt32(played.Minutes) + "m";

            // Show the data for the player
            player.TriggerEvent("showExtendedData", playedTime);
        }
    }
}
