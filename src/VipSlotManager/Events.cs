using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;

namespace PRoConEvents
{
    public partial class VipSlotManager
    {
        //////////////////////
        // Server Events
        //////////////////////

        public override void OnReservedSlotsList(List<String> soldierNames)
        {
            if (!this.fIsEnabled) { return; }
            DebugWrite("[OnReservedSlotsList] Receive VIP players from Gameserver (reservedSlotList)", 5);
            this.vipsGS = soldierNames;
            this.gotVipsGS = true;
        }

        public override void OnReservedSlotsListAggressiveJoin(Boolean isEnabled)
        {
            this.AggressiveJoin = isEnabled;
        }

        public override void OnSquadLeader(Int32 teamId, Int32 squadId, String soldierName)
        {
            if ((teamId > 0) && (squadId > 0))
            {
                if (this.SquadLederList.ContainsKey(teamId.ToString() + squadId.ToString()))
                {
                    this.SquadLederList[teamId.ToString() + squadId.ToString()] = soldierName;
                }
                else
                {
                    this.SquadLederList.Add(teamId.ToString() + squadId.ToString(), soldierName);
                }
                if (this.playerTeamID.ContainsKey(soldierName))
                {
                    this.playerTeamID[soldierName] = teamId;
                }
                else
                {
                    this.playerTeamID.Add(soldierName, teamId);
                }
                if (this.playerSquadID.ContainsKey(soldierName))
                {
                    this.playerSquadID[soldierName] = squadId;
                }
                else
                {
                    this.playerSquadID.Add(soldierName, squadId);
                }
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            if (!this.fIsEnabled) { return; }
            try
            {
                if (CPlayerSubset.PlayerSubsetType.All == subset.Subset)
                {
                    DebugWrite("[OnListPlayers] Receive playerlist with TeamID and SquadID", 5);
                    this.vipsCurrentlyOnline = 0;
                    foreach (CPlayerInfo playerinfo in players)
                    {
                        if (this.SqlVipsActive.ContainsKey(playerinfo.SoldierName)) { this.vipsCurrentlyOnline++; }
                        if (this.playerTeamID.ContainsKey(playerinfo.SoldierName))
                        {
                            this.playerTeamID[playerinfo.SoldierName] = playerinfo.TeamID;
                        }
                        else
                        {
                            this.playerTeamID.Add(playerinfo.SoldierName, playerinfo.TeamID);
                        }
                        if (this.playerSquadID.ContainsKey(playerinfo.SoldierName))
                        {
                            this.playerSquadID[playerinfo.SoldierName] = playerinfo.SquadID;
                        }
                        else
                        {
                            this.playerSquadID.Add(playerinfo.SoldierName, playerinfo.SquadID);
                        }
                        if (playerinfo.GUID.StartsWith("EA_"))
                        {
                            if (this.GetPlayerGuid.Contains(playerinfo.SoldierName))
                            {
                                if (!this.Guid2Check.ContainsKey(playerinfo.GUID))
                                {
                                    this.Guid2Check.Add(playerinfo.GUID, playerinfo.SoldierName);
                                }
                                this.GetPlayerGuid.Remove(playerinfo.SoldierName);
                            }
                            if (this.NameGuidList.ContainsKey(playerinfo.SoldierName))
                            {
                                if (this.NameGuidList[playerinfo.SoldierName] != playerinfo.GUID) { this.NameGuidList[playerinfo.SoldierName] = playerinfo.GUID; }
                            }
                            else
                            {
                                this.NameGuidList.Add(playerinfo.SoldierName, playerinfo.GUID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugWrite("[OnListPlayers] ^bERROR:^n Can not receive playerlist with TeamID and SquadID. ERROR: " + ex, 5);
            }
        }

        public override void OnPlayerJoin(String soldierName)
        {
            String tmp_remaing = String.Empty;
            String tmp_msg = String.Empty;
            Int32 tmp_playerTimestamp = -1;

            // NO SEEDERBOT JOIN MSG
            if ((soldierName.StartsWith("Seed")) || (soldierName.StartsWith("seed"))) { return; }

            if ((this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes) || (this.SettingNonVipJoin == enumBoolYesNo.Yes) || (this.SettingVipJoin == enumBoolYesNo.Yes)) { tmp_playerTimestamp = this.GetVipTimestamp(soldierName); }

            // aggressive join kick protection
            if (this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes)
            {
                if (tmp_playerTimestamp != -1)
                {
                    if (!this.AggressiveVips.ContainsKey(soldierName)) { this.AggressiveVips.Add(soldierName, 0); }
                    if (this.RoundTempVips.Contains(soldierName))
                    {
                        // vip rejoined without 'aggressive join kick' power
                        DebugWrite("[OnJoin] [VIP] [AggressiveJoinAbuseBlocked] Valid VIP player " + this.strGreen(soldierName) + " rejoined as an valid VIP without 'Aggressive Join Kick' privilege (counter: " + this.AggressiveVips[soldierName].ToString() + "/" + this.SettingAggressiveJoinKickAbuseMax.ToString() + ") till next round.", 3);
                        // add tmp vip slot
                        DebugWrite("[OnJoin] [VIP] [AggressiveJoinAbuseBlocked] Add VIP Slot " + this.strGreen(soldierName) + " to Gameserver.", 4);
                        this.ProconVipAdd(soldierName);
                        this.ProconVipSave();
                        this.ProconVipList();
                    }
                    else
                    {
                        if (((DateTime.UtcNow - this.LastAggressiveJoinKickTime).TotalSeconds) <= 3)
                        {
                            // vip joined with 'aggressive join kick', count vip kicks
                            DebugWrite("[OnJoin] [VIP] [AggressiveJoin] Valid VIP player " + this.strGreen(soldierName) + " joined with 'Aggressive Join Kick' (counter: " + this.AggressiveVips[soldierName].ToString() + "/" + this.SettingAggressiveJoinKickAbuseMax.ToString() + ").", 5);
                            this.LastAggressiveJoinKickTime = DateTime.UtcNow.AddMinutes(-5);
                            this.AggressiveVips[soldierName]++;
                        }
                        if (this.AggressiveVips[soldierName] >= this.SettingAggressiveJoinKickAbuseMax)
                        {
                            // block vip to use 'aggressive join kick' till next round
                            DebugWrite("[OnJoin] [VIP] [AggressiveJoin] Valid VIP player " + this.strGreen(soldierName) + " triggered his max. threshold (" + this.AggressiveVips[soldierName].ToString() + "/" + this.SettingAggressiveJoinKickAbuseMax.ToString() + ") to use 'Aggressive Join Kick' till next round.", 4);

                            if ((this.AggressiveVips[soldierName] >= 2) && (this.SettingAdkatsLog == enumBoolYesNo.Yes) && (this.SettingAdkatsLogVipAggressiveJoinAbuse == enumBoolYesNo.Yes))
                            {
                                if (this.NameGuidList.ContainsKey(soldierName))
                                {
                                    this.AdkatsPlayerLog(soldierName, "Aggressive Join Abuse " + this.AggressiveVips[soldierName].ToString() + "/" + this.SettingAggressiveJoinKickAbuseMax.ToString() + " per round (blocked to use Aggressive Join Kick till next round)");
                                }
                                else
                                {
                                    // wait, retry. need ea guid for adkats
                                    Thread ThreadWorker1011 = new Thread(new ThreadStart(delegate ()
                                    {
                                        Int32 tmp_retry_counter = 0;
                                        while (tmp_retry_counter < 10)
                                        {
                                            Thread.Sleep(5000);
                                            tmp_retry_counter++;
                                            if (this.NameGuidList.ContainsKey(soldierName))
                                            {
                                                tmp_retry_counter = 99;
                                                this.AdkatsPlayerLog(soldierName, "Aggressive Join Abuse " + this.AggressiveVips[soldierName].ToString() + "/" + this.SettingAggressiveJoinKickAbuseMax.ToString() + " per round (blocked to use Aggressive Join Kick till next round)");
                                            }
                                        }
                                    }));
                                    ThreadWorker1011.IsBackground = true;
                                    ThreadWorker1011.Name = "threadworker1011";
                                    ThreadWorker1011.Start();
                                }
                            }

                            this.RoundTempVips.Add(soldierName);
                            this.ProconVipAdd(soldierName);
                            this.ProconVipSave();
                            this.ProconVipList();
                        }
                    }
                }
            }

            // no multiple join spam
            if (this.onJoinSpammer.ContainsKey(soldierName))
            {
                if (((DateTime.UtcNow - this.onJoinSpammer[soldierName]).TotalSeconds) < 30)
                {
                    this.onJoinSpammer[soldierName] = DateTime.UtcNow;
                    return;
                }
            }
            else
            {
                this.onJoinSpammer.Add(soldierName, DateTime.UtcNow);
            }

            // check case sensitive difference between real ingame playername and sql playername
            if (this.SqlVipsActiveNamesOnly.Contains(soldierName, StringComparer.CurrentCultureIgnoreCase))
            {
                if (!this.SqlVipsActive.ContainsKey(soldierName))
                {
                    // case sensitive problem detected
                    if (this.changevipname(soldierName, soldierName))
                    {
                        DebugWrite("[OnJoin] [Auto-Correction] Change VIP Slot playername to " + this.strGreen(soldierName) + " for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database (case sensitive difference detected between real playername and SQL playername). Gameserver will be updated on next Sync.", 3);
                        this.ProconChat("Auto-Correction > Change VIP Slot playername to " + this.strGreen(soldierName) + " (case sensitive difference detected between real playername and SQL playername).");
                    }
                }
            }

            /////////////////////////
            // Notify - Join Message
            /////////////////////////
            if (this.SettingVipJoin == enumBoolYesNo.Yes)
            {
                if (tmp_playerTimestamp != -1)
                {
                    DebugWrite("[OnJoin] [VIP] " + this.strGreen(soldierName) + " with !VIP Slot joined the server", 3);
                    tmp_remaing = this.strGetRemTime(tmp_playerTimestamp);
                    tmp_msg = this.SettingVipJoinMsg.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                    this.SayMsg(tmp_msg);
                }
            }
            if (this.SettingNonVipJoin == enumBoolYesNo.Yes)
            {
                if (tmp_playerTimestamp == -1)
                {
                    tmp_msg = this.SettingNonVipJoinMsg.Replace("%player%", soldierName).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                    this.SayMsg(tmp_msg);
                }
            }

            if (this.vipmsg.Contains(soldierName)) { this.vipmsg.Remove(soldierName); }
            if ((this.EAGuidTracking == enumBoolYesNo.Yes) && (!this.GetPlayerGuid.Contains(soldierName))) { this.GetPlayerGuid.Add(soldierName); }
        }

        public override void OnPlayerSpawned(String soldierName, Inventory spawnedInventory)
        {
            String tmp_remaing = String.Empty;
            String tmp_msg = String.Empty;
            Boolean tmp_firstspawn = false;

            // clean player join spamming
            if (this.onJoinSpammer.ContainsKey(soldierName)) { this.onJoinSpammer.Remove(soldierName); }

            /////////////////////////
            // Notify - Spawn Message
            /////////////////////////
            if (this.SettingVipSpawn == enumBoolYesNo.Yes)
            {
                Int32 tmp_playerTimestamp = this.GetVipTimestamp(soldierName);
                if (tmp_playerTimestamp != -1)
                {
                    if (!this.vipmsg.Contains(soldierName))
                    {
                        this.vipmsg.Add(soldierName);
                        tmp_firstspawn = true;
                        if ((this.EAGuidTracking == enumBoolYesNo.Yes) && (!this.GetPlayerGuid.Contains(soldierName))) { this.GetPlayerGuid.Add(soldierName); }
                        DebugWrite("[OnSpawn] [VIP] " + this.strGreen(soldierName) + " with !VIP Slot spawned (first time this round)", 5);
                        tmp_remaing = this.strGetRemTime(tmp_playerTimestamp);
                        // First Spawn Msg
                        tmp_msg = this.SettingVipSpawnMsg.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        if (this.SettingVipSpawnMsgSay == enumBoolYesNo.Yes)
                        {
                            this.SayMsg(tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        else
                        {
                            this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        // Second Spawn Msg
                        tmp_msg = this.SettingVipSpawnMsg2.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        if (this.SettingVipSpawnMsg2Say == enumBoolYesNo.Yes)
                        {
                            this.SayMsg(tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        else
                        {
                            this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        // Third Spawn Msg
                        tmp_msg = this.SettingVipSpawnMsg3.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        if (this.SettingVipSpawnMsg3Say == enumBoolYesNo.Yes)
                        {
                            this.SayMsg(tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        else
                        {
                            this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        // Spawn Yell
                        tmp_msg = this.SettingVipSpawnYell.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        if (this.SettingVipSpawnYellAll == enumBoolYesNo.Yes)
                        {
                            this.YellMsg(tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                        else
                        {
                            this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                        }
                    }
                }
            }

            /////////////////////////
            // Notify - VIP Slot Expired Message
            /////////////////////////
            if (this.SettingVipExp == enumBoolYesNo.Yes)
            {
                if (this.vipsExpired.Contains(soldierName))
                {
                    if (!this.vipmsg.Contains(soldierName))
                    {
                        this.vipmsg.Add(soldierName);
                        DebugWrite("[OnSpawn] [VIP] Send VIP Expired Message to " + this.strGreen(soldierName), 3);
                        // First Spawn Msg
                        tmp_msg = this.SettingVipExpMsg.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipExpDelay);
                        // Second Spawn Msg
                        tmp_msg = this.SettingVipExpMsg2.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipExpDelay);
                        // Spawn Yell
                        tmp_msg = this.SettingVipExpYell.Replace("%player%", this.strGreen(soldierName)).Replace("%time%", tmp_remaing).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                        this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipExpDelay);
                        // SQL Set Status inactive
                        if (this.SetSqlpStatus(soldierName, "inactive"))
                        {
                            DebugWrite("[OnSpawn] [VIP Expired] " + this.strGreen(soldierName) + " set vip status to 'inactive' in SQL database", 4);
                        }
                    }
                }
            }

            /////////////////////////
            // Notify - NON-VIP got kicked for VIP
            /////////////////////////
            if (this.SettingAggressiveJoinMsg.Length > 2)
            {
                if (this.kickedForVip.Contains(soldierName))
                {
                    DebugWrite("[OnSpawn] " + this.strBlack(soldierName) + " rejoined as kicked NON-VIP", 4);
                    tmp_msg = this.SettingAggressiveJoinMsg.Replace("%player%", soldierName).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                    // Send Msg
                    if (this.SettingAggressiveJoinMsgType == "Private Yell")
                    {
                        this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                    }
                    else if (this.SettingAggressiveJoinMsgType == "Private Say")
                    {
                        this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                    }
                    else if (this.SettingAggressiveJoinMsgType == "Private Yell and Private Say")
                    {
                        this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                        this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay);
                    }
                    else if (this.SettingAggressiveJoinMsgType == "Say to all Players")
                    {
                        this.SayMsg(tmp_msg, this.SettingVipSpawnMsgDelay);
                    }
                    this.kickedForVip.Remove(soldierName);
                }
            }

            /////////////////////////
            // Notify - VIP lost his 'Aggressive Join Kick' privilege till next round
            /////////////////////////
            if (this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes)
            {
                if ((tmp_firstspawn) || (!this.vipmsg.Contains(soldierName)))
                {
                    if (!this.vipmsg.Contains(soldierName)) { this.vipmsg.Add(soldierName); }
                    if (this.SettingAggressiveJoinKickAbuseMsg.Length > 2)
                    {
                        if ((this.SqlVipsActive.ContainsKey(soldierName)) && (this.RoundTempVips.Contains(soldierName)))
                        {
                            DebugWrite("[OnSpawn] [VIP] [AggressiveJoinAbuseBlocked]" + this.strGreen(soldierName) + " send message about 'Aggressive Join Abuse'.", 5);
                            // Send Msg
                            tmp_msg = this.SettingAggressiveJoinKickAbuseMsg.Replace("%player%", soldierName).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString());
                            if (this.SettingAggressiveJoinKickAbuseMsgType == "Private Yell")
                            {
                                this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay + 1);
                            }
                            else if (this.SettingAggressiveJoinKickAbuseMsgType == "Private Say")
                            {
                                this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay + 1);
                            }
                            else if (this.SettingAggressiveJoinKickAbuseMsgType == "Private Yell and Private Say")
                            {
                                this.PlayerYellMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay + 1);
                                this.PlayerSayMsg(soldierName, tmp_msg, this.SettingVipSpawnMsgDelay + 1);
                            }
                        }
                    }
                }
            }
        }

        public override void OnRoundOver(Int32 winningTeamId)
        {
            this.RoundEndCleaner();
        }

        public override void OnLevelLoaded(String mapFileName, String Gamemode, Int32 roundsPlayed, Int32 roundsTotal)
        { // BF3 BF4 BFH
            this.RoundEndCleaner();
        }

        public override void OnLoadingLevel(String strMapFileName, Int32 roundsPlayed, Int32 roundsTotal)
        { // BFBC2
            this.RoundEndCleaner();
        }

        public override void OnServerInfo(CServerInfo serverInfo)
        {
            this.CurrentGameMode = serverInfo.GameMode;
            if (this.SettingAggressiveJoin == enumBoolYesNo.Yes)
            {
                if ((this.CurrentGameMode == "ConquestLarge0") || (this.CurrentGameMode == "ConquestSmall0") || (this.CurrentGameMode == "TeamDeathMatch0") || (this.CurrentGameMode == "Chainlink0"))
                {
                    Double tmp_scoreLoser = 999999;
                    foreach (TeamScore score in serverInfo.TeamScores)
                    {
                        if (score.TeamID != null && score.Score != null)
                        {
                            Double tmp_remscore = score.WinningScore - score.Score;
                            if (this.CurrentGameMode != "TeamDeathMatch0") { tmp_remscore = score.Score - score.WinningScore; }
                            if (tmp_remscore <= tmp_scoreLoser) { tmp_scoreLoser = tmp_remscore; }
                        }
                    }
                    this.ticketLoserTeam = tmp_scoreLoser;
                }
            }
        }

        public override void OnPlayerDisconnected(String playerName, String reason)
        {
            if ((this.AggressiveJoin) && (reason == "PLAYER_KICKED"))
            {
                DebugWrite(this.strBlack(playerName) + " got KICKED to make room for VIP", 3);
                if (this.SettingAggressiveJoinKick == String.Empty) { this.ProconChat(playerName + " got KICKED to make room for VIP"); }
                if ((this.SettingAdkatsLog == enumBoolYesNo.Yes) && (this.SettingAdkatsLogNonVipKick == enumBoolYesNo.Yes)) { this.AdkatsPlayerLog(playerName, "got kicked to make room for VIP"); }
                this.SayMsg(this.SettingAggressiveJoinKick.Replace("%player%", playerName).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()), this.SettingVipSpawnMsgDelay);
                if (!this.kickedForVip.Contains(playerName)) { this.kickedForVip.Add(playerName); }
                this.LastAggressiveJoinKickTime = DateTime.UtcNow;
            }

            if (this.RoundTempVips.Contains(playerName))
            {
                // remove vip
                if (this.SqlVipsActive.ContainsKey(playerName))
                {
                    DebugWrite("[OnDisconnected] [AggressiveJoinAbuse] Valid VIP player " + this.strGreen(playerName) + " blocked to use 'Aggressive Join Kick' till next round.", 3);
                    DebugWrite("[OnDisconnected] Remove VIP Slot " + this.strGreen(playerName) + " from Gameserver.", 4);
                }
                else
                {
                    DebugWrite("[OnDisconnected] [SemiVIP] Remove Semi VIP Slot " + this.strBlack(playerName) + " from Gameserver.", 3);
                }
                this.ProconVipRemove(playerName);
                this.ProconVipSave();
                this.ProconVipList();
            }

            if (this.playerTeamID.ContainsKey(playerName)) { this.playerTeamID.Remove(playerName); }
            if (this.playerSquadID.ContainsKey(playerName)) { this.playerSquadID.Remove(playerName); }
            if (this.GetPlayerGuid.Contains(playerName)) { this.GetPlayerGuid.Remove(playerName); }
        }

        public override void OnTeamChat(String speaker, String message, Int32 teamId) { this.vsChatEvent(speaker, message); }

        public override void OnSquadChat(String speaker, String message, Int32 teamId, Int32 squadId) { this.vsChatEvent(speaker, message); }

        public override void OnGlobalChat(String strSpeaker, String strMessage) { this.vsChatEvent(strSpeaker, strMessage); }
    }
}
