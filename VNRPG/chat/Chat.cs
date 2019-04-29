using GTANetworkAPI;
using VNRPG.globals;
using VNRPG.factions;
using VNRPG.messages.error;
using VNRPG.messages.general;
using VNRPG.messages.success;
using System;
using System.Linq;
using System.Collections.Generic;

namespace VNRPG.chat
{
    public class Chat : Script
    {
        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            // Deleting player's attached label
            if (player.GetData(EntityData.PLAYER_AME) != null)
            {
                TextLabel label = player.GetData(EntityData.PLAYER_AME);
                label.Detach();
                label.Delete();
            }
        }

        public static void SendMessageToNearbyPlayers(Client player, string message, int type, float range, bool excludePlayer = false)
        {
            // Calculate the different gaps for the chat
            float distanceGap = range / Constants.CHAT_RANGES;

            // Get the list of the connected players
            List<Client> targetList = NAPI.Pools.GetAllPlayers().Where(p => p.GetData(EntityData.PLAYER_PLAYING) != null && p.Dimension == player.Dimension).ToList();
            
            foreach (Client target in targetList)
            {
                if (player != target || (player == target && !excludePlayer))
                {
                    float distance = player.Position.DistanceTo(target.Position);

                    if (distance <= range)
                    {
                        // Getting message color
                        string chatMessageColor = GetChatMessageColor(distance, distanceGap, false);
                        string oocMessageColor = GetChatMessageColor(distance, distanceGap, true);

                        switch (type)
                        {
                            // We send the message
                            case Constants.MESSAGE_TALK:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_say + message);
                                break;
                            case Constants.MESSAGE_YELL:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_yell + message + "!");
                                break;
                            case Constants.MESSAGE_WHISPER:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_whisper + message);
                                break;
                            case Constants.MESSAGE_PHONE:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_phone + message);
                                break;
                            case Constants.MESSAGE_RADIO:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_radio + message);
                                break;
                            case Constants.MESSAGE_ME:
                                target.SendChatMessage(Constants.COLOR_CHAT_ME + player.Name + " " + message);
                                break;
                            case Constants.MESSAGE_DO:
                                target.SendChatMessage(Constants.COLOR_CHAT_DO + player.Name + "[ID: " + player.Value + "] " + message);
                                break;
                            case Constants.MESSAGE_OOC:
                                target.SendChatMessage(oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                                break;
                            case Constants.MESSAGE_DISCONNECT:
                                target.SendChatMessage(Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message);
                                break;
                            case Constants.MESSAGE_MEGAPHONE:
                                target.SendChatMessage(Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message);
                                break;
                            case Constants.MESSAGE_SU_TRUE:
                                message = string.Format(SuccRes.possitive_result, player.Name);
                                target.SendChatMessage(Constants.COLOR_SU_POSITIVE + message);
                                break;
                            case Constants.MESSAGE_SU_FALSE:
                                message = string.Format(ErrRes.negative_result, player.Name);
                                target.SendChatMessage(Constants.COLOR_ERROR + message);
                                break;
                        }
                    }
                }
            }
        }
        
        private static string GetChatMessageColor(float distance, float distanceGap, bool ooc)
        {
            string color = null;
            if (distance < distanceGap)
            {
                color = ooc ? Constants.COLOR_OOC_CLOSE : Constants.COLOR_CHAT_CLOSE;
            }
            else if (distance < distanceGap * 2)
            {
                color = ooc ? Constants.COLOR_OOC_NEAR : Constants.COLOR_CHAT_NEAR;
            }
            else if (distance < distanceGap * 3)
            {
                color = ooc ? Constants.COLOR_OOC_MEDIUM : Constants.COLOR_CHAT_MEDIUM;
            }
            else if (distance < distanceGap * 4)
            {
                color = ooc ? Constants.COLOR_OOC_FAR : Constants.COLOR_CHAT_FAR;
            }
            else
            {
                color = ooc ? Constants.COLOR_OOC_LIMIT : Constants.COLOR_CHAT_LIMIT;
            }
            return color;
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnChatMessage(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_PLAYING) == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_chat);
            }
            else if (player.GetSharedData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_ON_AIR) != null)
            {
                WeazelNews.SendNewsMessage(player, message);
            }
            else if (player.GetData(EntityData.PLAYER_PHONE_TALKING) != null)
            {
                // Target player of the message
                Client target = player.GetData(EntityData.PLAYER_PHONE_TALKING);

                // We send the message to the player and target
                player.SendChatMessage(Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);
                target.SendChatMessage(Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);

                // We send the message to nearby players
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_PHONE, player.Dimension > 0 ? 7.5f : 10.0f, true);
            }
            else
            {
                // Get the server time            
                string timeString = "[" + DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond + "] ";

                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, player.Dimension > 0 ? 7.5f : 10.0f);
                NAPI.Util.ConsoleOutput(timeString + "[ID:" + player.Value + "] " + player.Name + GenRes.chat_say + message);
            }
        }

        [RemoteEvent("playerNotLoggedCommand")]
        public void PlayerNotLoggedCommandEvent(Client player)
        {
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_command);
        }

        [RemoteEvent("logPlayerCommand")]
        public void LogPlayerCommandEvent(Client player, string command)
        {
            // Get the server time            
            string timeString = "[" + DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond + "] ";

            // Log the command used
            NAPI.Util.ConsoleOutput(timeString + string.Format(GenRes.command_used, player.Value, player.Name, command));
        }

       [Command(Commands.COM_SAY, Commands.HLP_SAY_COMMAND, GreedyArg = true)]
        public void DecirCommand(Client player, string message)
        {
            if (player.GetSharedData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, player.Dimension > 0 ? 7.5f : 10.0f);
            }
        }

        [Command(Commands.COM_YELL, Alias = Commands.COM_YELL_ALIAS, Description = Commands.HLP_YELL_COMMAND, GreedyArg = true)]
        public void GritarCommand(Client player, string message)
        {
            if (player.GetSharedData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_YELL, 45.0f);
            }
        }

        [Command(Commands.COM_WHISPER, Alias = Commands.COM_WHISPER_ALIAS, Description = Commands.HLP_WHISPER_COMMAND, GreedyArg = true)]
        public void SusurrarCommand(Client player, string message)
        {
            if (player.GetSharedData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_WHISPER, 3.0f);
            }
        }

        [Command(Commands.COM_ME, Commands.HLP_ME_COMMAND, GreedyArg = true)]
        public void MeCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COM_DO, Commands.HLP_DO_COMMAND, GreedyArg = true)]
        public void DoCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_DO, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COM_OOC, Alias = Commands.COM_OOC_ALIAS, Description = Commands.HLP_OOC_COMMAND, GreedyArg = true)]
        public void OocCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_OOC, player.Dimension > 0 ? 5.0f : 10.0f);
        }

        [Command(Commands.COM_LUCK)]
        public void SuCommand(Client player)
        {
            if (player.GetSharedData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Random random = new Random();
                int messageType = random.Next(0, 2) > 0 ? Constants.MESSAGE_SU_TRUE : Constants.MESSAGE_SU_FALSE;
                SendMessageToNearbyPlayers(player, string.Empty, messageType, 20.0f);
            }
        }

        [Command(Commands.COM_AME, Commands.HLP_AME_COMMAND, GreedyArg = true)]
        public void AmeCommand(Client player, string message = "")
        {
            if (player.GetData(EntityData.PLAYER_AME) != null)
            {
                // We get player's TextLabel
                TextLabel label = player.GetData(EntityData.PLAYER_AME);
                
                if (message.Length > 0)
                {
                    // We update label's text
                    label.Text = "*" + message + "*";
                }
                else
                {
                    // Deleting TextLabel
                    label.Detach();
                    label.Delete();
                    player.ResetData(EntityData.PLAYER_AME);
                }
            }
            else
            {
                TextLabel ameLabel = NAPI.TextLabel.CreateTextLabel("*" + message + "*", new Vector3(0.0f, 0.0f, 0.0f), 50.0f, 0.5f, 4, new Color(201, 90, 0, 255));
                ameLabel.AttachTo(player, "SKEL_Head", new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                player.SetData(EntityData.PLAYER_AME, ameLabel);
            }
        }

        [Command(Commands.COM_MEGAPHONE, Commands.HLP_MEGAPHONE_COMMAND, GreedyArg = true)]
        public void MegafonoCommand(Client player, string message)
        {
            if (!player.IsInVehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }

            if(player.Vehicle.Class != Constants.VEHICLE_CLASS_EMERGENCY)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_megaphone);
                return;
            }

            // Send the message with the megaphone
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_MEGAPHONE, 45.0f);
        }

        [Command(Commands.COM_PM, Commands.HLP_MP_COMMAND, GreedyArg = true)]
        public void MpCommand(Client player, string arguments)
        {
            Client target = null;
            string[] args = arguments.Trim().Split(' ');

            if (int.TryParse(args[0], out int targetId) == true)
            {
                // We get the player from the id
                target = Globals.GetPlayerById(targetId);
                args = args.Where(w => w != args[0]).ToArray();
                if (args.Length < 1)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_MP_COMMAND);
                    return;
                }
            }
            else if (args.Length > 2)
            {
                target = NAPI.Player.GetPlayerFromName(args[0] + " " + args[1]);
                args = args.Where(w => w != args[1]).ToArray();
                args = args.Where(w => w != args[0]).ToArray();
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_MP_COMMAND);
                return;
            }
            
            if (target != null && target.GetData(EntityData.PLAYER_PLAYING) != null)
            {
                if (player.GetData(EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && target.GetData(EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.mps_only_admin);
                }
                else
                {
                    string message = string.Join(" ", args);
                    string secondMessage = string.Empty;

                    // Sending messages to both players
                    player.SendChatMessage(Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_to + "[ID: " + target.Value + "] " + target.Name + ": " + message + "))");
                    target.SendChatMessage(Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_from + "[ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
            }
        }
    }
}
