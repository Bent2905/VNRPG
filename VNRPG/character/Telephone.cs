using GTANetworkAPI;
using VNRPG.database;
using VNRPG.globals;
using VNRPG.model;
using VNRPG.factions;
using VNRPG.messages.information;
using VNRPG.messages.general;
using VNRPG.messages.error;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VNRPG.character
{
    public class Telephone : Script
    {
        public static List<ContactModel> contactList;

        private ContactModel GetContactFromId(int contactId)
        {
            // Get the contact matching the selected identifier
            return contactList.Where(contact => contact.id == contactId).FirstOrDefault();
        }

        private int GetNumerFromContactName(string contactName, int playerPhone)
        {
            // Get the contact matching the name
            ContactModel contactModel = contactList.Where(contact => contact.owner == playerPhone && contact.contactName == contactName).FirstOrDefault();

            return contactModel == null ? 0 : contactModel.contactNumber;
        }

        private List<ContactModel> GetTelephoneContactList(int number)
        {
            // Get the contact list from a phone number
            return contactList.Where(contact => contact.owner == number).ToList();
        }

        private string GetContactInTelephone(int phone, int number)
        {
            // Get the contact name from a number
            ContactModel contactModel = contactList.Where(contact => contact.owner == phone && contact.contactNumber == number).FirstOrDefault();

            return contactModel == null ? string.Empty : contactModel.contactName;
        }

        [RemoteEvent("addNewContact")]
        public void AddNewContactEvent(Client player, int contactNumber, string contactName)
        {
            // Create the model for the new contact
            ContactModel contact = new ContactModel();
            {
                contact.owner = player.GetData(EntityData.PLAYER_PHONE);
                contact.contactNumber = contactNumber;
                contact.contactName = contactName;
            }

            Task.Factory.StartNew(() =>
            {
                // Add contact to database
                contact.id = Database.AddNewContact(contact);
                contactList.Add(contact);
            });
            
            string actionMessage = string.Format(InfoRes.contact_created, contactName, contactNumber);
            player.SendChatMessage(Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("modifyContact")]
        public void ModifyContactEvent(Client player, int contactIndex, int contactNumber, string contactName)
        {
            // Modify contact data
            ContactModel contact = GetContactFromId(contactIndex);
            contact.contactNumber = contactNumber;
            contact.contactName = contactName;

            Task.Factory.StartNew(() =>
            {
                // Modify the contact's data
                Database.ModifyContact(contact);
            });
            
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.contact_modified);
        }

        [RemoteEvent("deleteContact")]
        public void DeleteContactEvent(Client player, int contactIndex)
        {
            ContactModel contact = GetContactFromId(contactIndex);
            string contactName = contact.contactName;
            int contactNumber = contact.contactNumber;

            Task.Factory.StartNew(() =>
            {
                // Delete the contact
                Database.DeleteContact(contactIndex);
                contactList.Remove(contact);
            });
            
            string actionMessage = string.Format(InfoRes.contact_deleted, contactName, contactNumber);
            player.SendChatMessage(Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("sendPhoneMessage")]
        public void SendPhoneMessageEvent(Client player, int contactIndex, string textMessage)
        {
            ContactModel contact = GetContactFromId(contactIndex);
            
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (target.GetData(EntityData.PLAYER_PHONE) == contact.contactNumber)
                {
                    // Check player's number
                    int phone = player.GetData(EntityData.PLAYER_PHONE);
                    string contactName = GetContactInTelephone(phone, contact.contactNumber);

                    if (contactName.Length == 0)
                    {
                        contactName = contact.contactNumber.ToString();
                    }

                    // Send the message to the target
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.sms_sent);
                    target.SendChatMessage(Constants.COLOR_INFO + "[" + GenRes.sms_from + contactName + "] " + textMessage);

                    Task.Factory.StartNew(() =>
                    {
                        // Add the SMS to the database
                        Database.AddSMSLog(phone, contact.contactNumber, textMessage);
                    });

                    return;
                }
            }

            // There's no player matching the contact
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
        }

        [Command(Commands.COM_CALL, Commands.HLP_PHONE_CALL_COMMAND, GreedyArg = true)]
        public void CallCommand(Client player, string called)
        {
            if (player.GetData(EntityData.PLAYER_PHONE_TALKING) != null || player.GetData(EntityData.PLAYER_CALLING) != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_phone_talking);
            }
            else
            {
                ItemModel item = Globals.GetItemInEntity(player.GetData(EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
                if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
                {
                    int peopleOnline = 0;

                    if (int.TryParse(called, out int number) == true)
                    {
                        switch (number)
                        {
                            case Constants.NUMBER_POLICE:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && Faction.IsPoliceMember(target))
                                    {
                                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.FACTION_POLICE);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_POLICE);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            case Constants.NUMBER_EMERGENCY:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.FACTION_EMERGENCY);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_EMERGENCY);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            case Constants.NUMBER_NEWS:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_NEWS)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.FACTION_NEWS);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_NEWS);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            case Constants.NUMBER_TAXI:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_TAXI_DRIVER)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.FACTION_TAXI_DRIVER);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_TAXI);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            case Constants.NUMBER_FASTFOOD:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && target.GetData(EntityData.PLAYER_JOB) == Constants.JOB_FASTFOOD)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.JOB_FASTFOOD + 100);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_FASTFOOD);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            case Constants.NUMBER_MECHANIC:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (target.GetData(EntityData.PLAYER_PLAYING) != null && target.GetData(EntityData.PLAYER_JOB) == Constants.JOB_MECHANIC)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);
                                        peopleOnline++;
                                    }
                                }
                                
                                if (peopleOnline > 0)
                                {
                                    player.SetData(EntityData.PLAYER_CALLING, Constants.JOB_MECHANIC + 100);
                                    
                                    string playerMessage = string.Format(InfoRes.calling, Constants.NUMBER_MECHANIC);
                                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                                }
                                break;
                            default:
                                if (number > 0)
                                {
                                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                                    {
                                        if (target.GetData(EntityData.PLAYER_PHONE) == number)
                                        {
                                            int playerPhone = player.GetData(EntityData.PLAYER_PHONE);

                                            // Check if the player has the number as contact
                                            int phone = target.GetData(EntityData.PLAYER_PHONE);
                                            string contact = GetContactInTelephone(phone, playerPhone);

                                            if (contact.Length == 0)
                                            {
                                                contact = playerPhone.ToString();
                                            }

                                            player.SetData(EntityData.PLAYER_CALLING, target);

                                            // Check if the player calling is a contact into target's contact list
                                            string playerMessage = string.Format(InfoRes.calling, number);
                                            string targetMessage = string.Format(InfoRes.call_from, contact.Length > 0 ? contact : contact.ToString());
                                            
                                            player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                           target.SendChatMessage(Constants.COLOR_INFO + targetMessage);

                                            return;
                                        }
                                    }
                                }

                                // The phone number doesn't exist
                                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                                break;
                        }
                    }
                    else
                    {
                        // Call a contact
                        int playerPhone = player.GetData(EntityData.PLAYER_PHONE);
                        int targetPhone = GetNumerFromContactName(called, playerPhone);
                        
                        if (targetPhone > 0)
                        {
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (target.GetData(EntityData.PLAYER_PHONE) == targetPhone)
                                {
                                    if (target.GetData(EntityData.PLAYER_CALLING) != null || target.GetData(EntityData.PLAYER_PHONE_TALKING) != null || player.GetData(EntityData.PLAYER_PLAYING) == null)
                                    {
                                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                                    }
                                    else
                                    {
                                        player.SetData(EntityData.PLAYER_CALLING, target);

                                        // Check if the player is in target's contact list
                                        string contact = GetContactInTelephone(target.GetData(EntityData.PLAYER_PHONE), playerPhone);

                                        string playerMessage = string.Format(InfoRes.calling, called);
                                        string targetMessage = string.Format(InfoRes.call_from, contact.Length > 0 ? contact : playerPhone.ToString());
                                        player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                                       target.SendChatMessage(Constants.COLOR_INFO + InfoRes.incoming_call);
                                    }
                                    return;
                                }
                            }
                        }

                        // The contact player isn't online
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                    }
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
                }
            }
        }

        [Command(Commands.COM_ANSWER)]
        public void AnswerCommand(Client player)
        {
            if (player.GetData(EntityData.PLAYER_CALLING) != null || player.GetData(EntityData.PLAYER_PHONE_TALKING) != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_phone_talking);
            }
            else
            {
                // Get the players calling
                List<Client> callingPlayers = NAPI.Pools.GetAllPlayers().Where(p => p.GetData(EntityData.PLAYER_CALLING) != null).ToList();

                foreach (Client target in callingPlayers)
                {
                    if (target.GetData(EntityData.PLAYER_CALLING) is int)
                    {
                        int factionJob = target.GetData(EntityData.PLAYER_CALLING);
                        int faction = player.GetData(EntityData.PLAYER_FACTION);
                        int job = player.GetData(EntityData.PLAYER_JOB);

                        if (factionJob == faction || (Faction.IsPoliceMember(player) && factionJob == Constants.FACTION_POLICE) || factionJob == job + 100)
                        {
                            // Link both players in the same call
                            target.ResetData(EntityData.PLAYER_CALLING);
                            player.SetData(EntityData.PLAYER_PHONE_TALKING, target);
                            target.SetData(EntityData.PLAYER_PHONE_TALKING, player);
                                
                            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_received);
                            target.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_taken);

                            // Store call starting time
                            target.SetData(EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                            return;
                        }
                    }
                    else if (target.GetData(EntityData.PLAYER_CALLING) == player)
                    {
                        // Link both players in the same call
                        target.ResetData(EntityData.PLAYER_CALLING);
                        player.SetData(EntityData.PLAYER_PHONE_TALKING, target);
                        target.SetData(EntityData.PLAYER_PHONE_TALKING, player);
                            
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_received);
                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_taken);

                        // Store call starting time
                        target.SetData(EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                        return;
                    }
                }

                // Nobody's calling the player
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_called);
            }
        }

        [Command(Commands.COM_HANG)]
        public void HangCommand(Client player)
        {
            if (player.GetData(EntityData.PLAYER_CALLING) != null)
            {
                // Hang up the call
                player.ResetData(EntityData.PLAYER_CALLING);
            }
            else if (player.GetData(EntityData.PLAYER_PHONE_TALKING) != null)
            {
                // Get the player he's talking with
                int elapsed = 0;
                Client target = player.GetData(EntityData.PLAYER_PHONE_TALKING);
                int playerPhone = player.GetData(EntityData.PLAYER_PHONE);
                int targetPhone = target.GetData(EntityData.PLAYER_PHONE);

                // Get phone call time
                if (player.GetData(EntityData.PLAYER_PHONE_CALL_STARTED) != null)
                {
                    elapsed = Globals.GetTotalSeconds() - player.GetData(EntityData.PLAYER_PHONE_CALL_STARTED);

                    Task.Factory.StartNew(() =>
                    {
                        // Update the elapsed time into the database
                        Database.AddCallLog(playerPhone, targetPhone, elapsed);
                    });
                }
                else
                {
                    elapsed = Globals.GetTotalSeconds() - target.GetData(EntityData.PLAYER_PHONE_CALL_STARTED);

                    Task.Factory.StartNew(() =>
                    {
                        // Update the elapsed time into the database
                        Database.AddCallLog(targetPhone, playerPhone, elapsed);
                    });
                }

                // Hang up the call for both players
                player.ResetData(EntityData.PLAYER_PHONE_TALKING);
                target.ResetData(EntityData.PLAYER_PHONE_TALKING);
                player.ResetData(EntityData.PLAYER_PHONE_CALL_STARTED);

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
                target.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_phone_talking);
            }
        }

        [Command(Commands.COM_SMS, Commands.HLP_SMS_COMMAND, GreedyArg = true)]
        public void SmsCommand(Client player, int number, string message)
        {
            ItemModel item = Globals.GetItemInEntity(player.GetData(EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (number > 0 && target.GetData(EntityData.PLAYER_PHONE) == number)
                    {
                        int playerPhone = player.GetData(EntityData.PLAYER_PHONE);

                        // Check if the player's in the contact list
                        int phone = target.GetData(EntityData.PLAYER_PHONE);
                        string contact = GetContactInTelephone(phone, playerPhone);

                        if (contact.Length == 0)
                        {
                            contact = playerPhone.ToString();
                        }

                        // Send the SMS warning to the player
                        target.SendChatMessage(Constants.COLOR_INFO + "[" + GenRes.sms_from + playerPhone + "] " + message);

                        foreach (Client targetPlayer in NAPI.Pools.GetAllPlayers())
                        {
                            if (targetPlayer.Position.DistanceTo(player.Position) < 20.0f)
                            {
                                string nearMessage = string.Format(InfoRes.player_texting, player.Name);
                                targetPlayer.SendChatMessage(Constants.COLOR_CHAT_ME + nearMessage);
                            }
                        }

                        Task.Factory.StartNew(() =>
                        {
                            // Add the SMS into the database
                            Database.AddSMSLog(playerPhone, number, message);
                        });

                        return;
                    }
                }

                // The phone doesn't exist
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
            }
        }

        [Command(Commands.COM_CONTACTS, Commands.HLP_CONTACTS_COMMAND)]
        public void AgendaCommand(Client player, string action)
        {
            ItemModel item = Globals.GetItemInEntity(player.GetData(EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                // Get the contact list
                int phoneNumber = player.GetData(EntityData.PLAYER_PHONE);
                List<ContactModel> contacts = GetTelephoneContactList(phoneNumber);

                switch (action.ToLower())
                {
                    case Commands.ARG_NUMBER:
                        string message = string.Format(InfoRes.phone_number, phoneNumber);
                        player.SendChatMessage(Constants.COLOR_INFO + message);
                        break;
                    case Commands.ARG_VIEW:
                        if (contacts.Count > 0)
                        {
                            player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_LOAD);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                        }
                        break;
                    case Commands.ARG_ADD:
                        player.TriggerEvent("addContactWindow", Constants.ACTION_ADD);
                        break;
                    case Commands.ARG_MODIFY:
                        if (contacts.Count > 0)
                        {
                            player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_RENAME);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                        }
                        break;
                    case Commands.ARG_REMOVE:
                        if (contacts.Count > 0)
                        {
                            player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_DELETE);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                        }
                        break;
                    case Commands.ARG_SMS:
                        if (contacts.Count > 0)
                        {
                            player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_SMS);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                        }
                        break;
                    default:
                        player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_CONTACTS_COMMAND);
                        break;
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
            }
        }
    }
}
