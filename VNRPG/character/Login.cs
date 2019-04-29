using GTANetworkAPI;
using VNRPG.model;
using VNRPG.database;
using VNRPG.globals;
using VNRPG.messages.information;
using VNRPG.messages.general;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace VNRPG.character
{
    public class Login : Script
    {     
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            // Set the default skin and transparency
            NAPI.Player.SetPlayerSkin(player, PedHash.Strperf01SMM);
            player.Transparency = 255;

            // Initialize the player data
            Character.InitializePlayerData(player);

            Task.Factory.StartNew(() =>
            {
                AccountModel account = Database.GetAccount(player.SocialClubName);

                switch (account.status)
                {
                    case -1:
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.account_disabled);
                        player.Kick(InfoRes.account_disabled);
                        break;
                    case 0:
                        // Check if the account is registered or not
                        player.TriggerEvent(account.registered ? "accountLoginForm" : "showRegisterWindow");
                        break;
                    default:
                        // Welcome message
                        string welcomeMessage = string.Format(GenRes.welcome_message, player.SocialClubName);
                        player.SendChatMessage(welcomeMessage);
                        player.SendChatMessage(GenRes.welcome_hint);
                        player.SendChatMessage(GenRes.help_hint);
                        player.SendChatMessage(GenRes.ticket_hint);

                        if (account.lastCharacter > 0)
                        {
                            // Load selected character
                            PlayerModel character = Database.LoadCharacterInformationById(account.lastCharacter);
                            SkinModel skinModel = Database.GetCharacterSkin(account.lastCharacter);
                            
                            player.Name = character.realName;
                            player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
                            NAPI.Player.SetPlayerSkin(player, character.sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

                            Character.LoadCharacterData(player, character);
                            Customization.ApplyPlayerCustomization(player, skinModel, character.sex);
                            Customization.ApplyPlayerClothes(player);
                            Customization.ApplyPlayerTattoos(player);
                        }

                        // Activate the login window
                        player.SetSharedData(EntityData.SERVER_TIME, DateTime.Now.ToString("HH:mm:ss"));

                        break;
                }
            });
        }

        [RemoteEvent("loginAccount")]
        public void LoginAccountEvent(Client player, string password)
        {
            Task.Factory.StartNew(() =>
            {
                // Get the status of the account
                int status = Database.LoginAccount(player.SocialClubName, password);

                switch (status)
                {
                    case 0:
                        LoadApplicationEvent(player);
                        break;
                    case 1:
                        player.TriggerEvent("clearLoginWindow");
                        break;
                    default:
                        player.TriggerEvent("showLoginError");
                        break;
                }
            });
        }

        [RemoteEvent("registerAccount")]
        public void RegisterAccountEvent(Client player, string password)
        {
            Task.Factory.StartNew(() =>
            {
                // Register the account
                Database.RegisterAccount(player.SocialClubName, password);

                // Show the application for the player
                LoadApplicationEvent(player);
            });
        }

        [RemoteEvent("submitApplication")]
        public void SubmitApplicationEvent(Client player, string answers)
        {
            Task.Factory.StartNew(() =>
            {
                // Get all the question and answers
                Dictionary<int, int> application = NAPI.Util.FromJson<Dictionary<int, int>>(answers);

                // Check if all the answers are correct
                int mistakes = Database.CheckCorrectAnswers(application);

                if (mistakes > 0)
                {
                    // Tell the player his mistakes
                    player.TriggerEvent("failedApplication", mistakes);
                }
                else
                {
                    // Tell the player he passed the test
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.application_passed);

                    // Destroy the test window
                    player.TriggerEvent("clearApplication");

                    // Accept the account on the server
                    Database.ApproveAccount(player.SocialClubName);
                }

                // Register the attempt on the database
                Database.RegisterApplication(player.SocialClubName, mistakes);
            });
        }

        [RemoteEvent("changeCharacterSex")]
        public void ChangeCharacterSexEvent(Client player, int sex)
        {
            // Set the model of the player
            NAPI.Player.SetPlayerSkin(player, sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

            // Remove player's clothes
            player.SetClothes(11, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(8, 15, 0);

            // Save sex entity shared data
            player.SetData(EntityData.PLAYER_SEX, sex);

            // Force the player's animation
            player.PlayAnimation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", (int)Constants.AnimationFlags.Loop);
        }

        [RemoteEvent("createCharacter")]
        public void CreateCharacterEvent(Client player, string playerName, int playerAge, int playerSex, string skinJson)
        {
            PlayerModel playerModel = new PlayerModel();
            SkinModel skinModel = NAPI.Util.FromJson<SkinModel>(skinJson);

            playerModel.realName = playerName;
            playerModel.age = playerAge;
            playerModel.sex = playerSex;

            // Apply the skin to the character
            player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
            Customization.ApplyPlayerCustomization(player, skinModel, playerSex);

            Task.Factory.StartNew(() =>
            {
                int playerId = Database.CreateCharacter(player, playerModel, skinModel);

                if (playerId > 0)
                {
                    Character.InitializePlayerData(player);
                    player.Transparency = 255;
                    player.SetData(EntityData.PLAYER_SQL_ID, playerId);
                    player.SetData(EntityData.PLAYER_NAME, playerName);
                    player.SetData(EntityData.PLAYER_AGE, playerAge);
                    player.SetData(EntityData.PLAYER_SEX, playerSex);
                    player.SetData(EntityData.PLAYER_SPAWN_POS, new Vector3(-136.0034f, 6198.949f, 32.38448f));
                    player.SetData(EntityData.PLAYER_SPAWN_ROT, new Vector3(0.0f, 0.0f, 180.0f));

                    Database.UpdateLastCharacter(player.SocialClubName, playerId);

                    player.TriggerEvent("characterCreatedSuccessfully");
                }
            });
        }

        [RemoteEvent("setCharacterIntoCreator")]
        public void SetCharacterIntoCreatorEvent(Client player)
        {
            // Change player's skin
            NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeMale01);

            // Remove clothes
            player.SetClothes(11, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(8, 15, 0);

            // Remove all the tattoos
            Customization.RemovePlayerTattoos(player);

            // Set player's position
            player.Transparency = 255;
            player.Rotation = new Vector3(0.0f, 0.0f, 180.0f);
            player.Position = new Vector3(402.9364f, -996.7154f, -99.00024f);

            // Force the player's animation
            player.PlayAnimation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", (int)Constants.AnimationFlags.Loop);
        }

        [RemoteEvent("loadCharacter")]
        public void LoadCharacterEvent(Client player, string name)
        {
            Task.Factory.StartNew(() =>
            {
                PlayerModel playerModel = Database.LoadCharacterInformationByName(name);
                SkinModel skinModel = Database.GetCharacterSkin(playerModel.id);

                // Load player's model
                player.Name = playerModel.realName;
                player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
                NAPI.Player.SetPlayerSkin(player, playerModel.sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

                Character.LoadCharacterData(player, playerModel);
                Customization.ApplyPlayerCustomization(player, skinModel, playerModel.sex);
                Customization.ApplyPlayerClothes(player);
                Customization.ApplyPlayerTattoos(player);

                // Update last selected character
                Database.UpdateLastCharacter(player.SocialClubName, playerModel.id);
            });
        }

        [RemoteEvent("loadApplication")]
        public void LoadApplicationEvent(Client player)
        {
            Task.Factory.StartNew(() =>
            {
                // Get random questions
                List<TestModel> applicationQuestions = Database.GetRandomQuestions(Constants.APPLICATION_TEST, 10);

                // Get the ids from each question
                List<int> questionIds = applicationQuestions.Select(q => q.id).Distinct().ToList();

                // Get the answers from the questions
                List<TestModel> applicationAnswers = Database.GetQuestionAnswers(questionIds);

                player.TriggerEvent("showApplicationTest", NAPI.Util.ToJson(applicationQuestions), NAPI.Util.ToJson(applicationAnswers));
            });
        }
    }
}
