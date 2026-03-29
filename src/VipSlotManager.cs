/*  VipSlotManager.cs -  Procon Plugin [BF3, BF4, BFH, BC2]

    Version: 1.0.0.6

    Code Credit:
    PapaCharlie9 (forum.myrcon.com)  -  Basic Plugin Template Part (BasicPlugin.cs)
    [GWC]XpKiller (forum.myrcon.com) -  MySQL Main Functions (CChatGUIDStatsLogger.inc)
    [VdSk]LmaA-aD  -  Sponsoring BF3 & BFH Gameserver for testing Plugin

    Development by maxdralle@gmx.com

    This plugin file is part of PRoCon Frostbite.

    This plugin is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This plugin is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Dapper;

using MySqlConnector;

using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;

using CapturableEvent = PRoCon.Core.Events.CapturableEvents;
// Aliases
using EventType = PRoCon.Core.Events.EventType;

namespace PRoConEvents
{
    public partial class VipSlotManager : PRoConPluginAPI, IPRoConPluginInterface
    {
        /* Inherited:
            this.PunkbusterPlayerInfoList = new Dictionary<String, CPunkbusterInfo>();
            this.FrostbitePlayerInfoList = new Dictionary<String, CPlayerInfo>();
        */

        public enum MessageType { Warning, Error, Exception, Normal };

        private Boolean fIsEnabled;
        private Boolean gotVipsGS;
        private Boolean firstCheck;
        private Boolean AggressiveJoin;
        private Boolean isForceSync;
        private Boolean SqlTableExist;
        private Boolean AdkatsRunning;
        private Int32 fDebugLevel;
        private Int32 SyncCounter;
        private Int32 DBCleanerCounter = (new Random().Next(960, 990));
        private Int32 vipsCurrentlyOnline;
        private Int32 start_delay = (new Random().Next(40, 100));
        private Double ticketLoserTeam;
        private String CurrentGameMode;
        private String ServerIP;
        private String ServerPort;
        private String NewLiner;
        private String GetGameType;
        private DateTime LayerStartingTime = DateTime.UtcNow;
        private DateTime CurrentRoundEndTime = DateTime.UtcNow;
        private DateTime LastSync = DateTime.UtcNow;
        private DateTime LastAggressiveJoinKickTime = DateTime.UtcNow.AddMinutes(-5);

        private List<String> vipsGS = new List<String>();
        private List<String> vipmsg = new List<String>();
        private List<String> vipsExpired = new List<String>();
        private List<String> SqlVipsActiveNamesOnly = new List<String>();
        private List<String> kickedForVip = new List<String>();
        private List<String> GetPlayerGuid = new List<String>();
        private List<String> RoundTempVips = new List<String>();
        private Dictionary<String, Int32> SqlVipsActive = new Dictionary<String, Int32>();
        private Dictionary<String, Int32> playerTeamID = new Dictionary<String, Int32>();
        private Dictionary<String, Int32> playerSquadID = new Dictionary<String, Int32>();
        private Dictionary<String, String> SquadLederList = new Dictionary<String, String>();
        private Dictionary<String, String> Guid2Check = new Dictionary<String, String>();
        private Dictionary<String, DateTime> onJoinSpammer = new Dictionary<String, DateTime>();
        private Dictionary<String, String> NameGuidList = new Dictionary<String, String>();
        private Dictionary<String, Int32> AggressiveVips = new Dictionary<String, Int32>();

        private MySqlConnector.MySqlTransaction MySqlTrans;

        private String SettingStrSqlHostname;
        private String SettingStrSqlPort;
        private String SettingStrSqlDatabase;
        private String SettingStrSqlUsername;
        private String SettingStrSqlPassword;
        private String SettingGameType;
        private String SettingStrSqlServerGroup;
        private Int32 SettingSyncInterval;
        private String SettingSyncGs2Sql;
        private enumBoolYesNo SettingProconRulzIni;
        private enumBoolYesNo SettingAdminCmd;
        private enumBoolYesNo SettingPluginCmd;
        private enumBoolYesNo SettingVipCmd;
        private enumBoolYesNo SettingLeadCmd;
        private enumBoolYesNo SettingIgnoreVipLeader;
        private enumBoolYesNo SettingLeadByCRose;
        private enumBoolYesNo SettingKillmeCmd;
        private enumBoolYesNo SettingSwitchmeCmd;
        private enumBoolYesNo SettingChatReq;
        private String SettingInfoCommands;
        private String SettingInfoCmdRegMatch;
        private String SettingInfoVip1;
        private String SettingInfoVip2;
        private String SettingInfoVip3;
        private enumBoolYesNo SettingInfoSay;
        private enumBoolYesNo SettingVipJoin;
        private String SettingVipJoinMsg;
        private enumBoolYesNo SettingNonVipJoin;
        private String SettingNonVipJoinMsg;
        private enumBoolYesNo SettingVipSpawn;
        private String SettingVipSpawnMsg;
        private String SettingVipSpawnMsg2;
        private String SettingVipSpawnMsg3;
        private String SettingVipSpawnYell;
        private enumBoolYesNo SettingVipSpawnMsgSay;
        private enumBoolYesNo SettingVipSpawnMsg2Say;
        private enumBoolYesNo SettingVipSpawnMsg3Say;
        private enumBoolYesNo SettingVipSpawnYellAll;
        private Int32 SettingVipSpawnMsgDelay;
        private enumBoolYesNo SettingVipExp;
        private String SettingVipExpMsg;
        private String SettingVipExpMsg2;
        private String SettingVipExpYell;
        private Int32 SettingVipExpDelay;
        private enumBoolYesNo SettingForceSync;
        private enumBoolYesNo SettingMiniManager;
        private Int32 SettingYellDuring;
        private Int32 SettingDBCleaner;
        private Int32 SettingAggressiveJoinKickAbuseMax;
        private enumBoolYesNo EAGuidTracking;
        private enumBoolYesNo SettingAggressiveJoin;
        private enumBoolYesNo SettingAdkatsLog;
        private enumBoolYesNo SettingAdkatsLogNonVipKick;
        private enumBoolYesNo SettingAdkatsLogVipChanged;
        private enumBoolYesNo SettingAdkatsLogVipAggressiveJoinAbuse;
        private enumBoolYesNo SettingAggressiveJoinKickAbuse;

        private String SettingAggressiveJoinKick;
        private String SettingAggressiveJoinMsg;
        private String SettingAggressiveJoinMsgType;
        private String SettingAggressiveJoinKickAbuseMsg;
        private String SettingAggressiveJoinKickAbuseMsgType;

        public VipSlotManager()
        {
            this.fIsEnabled = false;
            this.gotVipsGS = false;
            this.firstCheck = false;
            this.AggressiveJoin = false;
            this.fDebugLevel = 3;
            this.SyncCounter = 999;
            this.ServerIP = String.Empty;
            this.ServerPort = String.Empty;
            this.isForceSync = false;
            this.SqlTableExist = false;
            this.AdkatsRunning = false;
            this.ticketLoserTeam = 0;
            this.CurrentGameMode = String.Empty;
            this.NewLiner = "";
            this.vipsCurrentlyOnline = 0;

            this.SettingStrSqlHostname = String.Empty;
            this.SettingStrSqlPort = "3306";
            this.SettingStrSqlDatabase = String.Empty;
            this.SettingStrSqlUsername = String.Empty;
            this.SettingStrSqlPassword = String.Empty;
            this.SettingGameType = "AUTOMATIC";
            this.SettingStrSqlServerGroup = "1";
            this.SettingSyncInterval = 5;
            this.SettingSyncGs2Sql = "yes  (30 days first Plugin installation only)";
            this.SettingProconRulzIni = enumBoolYesNo.No;
            this.SettingAdminCmd = enumBoolYesNo.Yes;
            this.SettingPluginCmd = enumBoolYesNo.Yes;
            this.SettingVipCmd = enumBoolYesNo.No;
            this.SettingLeadCmd = enumBoolYesNo.No;
            this.SettingIgnoreVipLeader = enumBoolYesNo.Yes;
            this.SettingLeadByCRose = enumBoolYesNo.No;
            this.SettingKillmeCmd = enumBoolYesNo.No;
            this.SettingSwitchmeCmd = enumBoolYesNo.No;
            this.SettingChatReq = enumBoolYesNo.Yes;
            this.SettingInfoCommands = "!vip,!slot,!reserved,!buy";
            this.SettingInfoCmdRegMatch = "^!vip|^!slot|^!reserved|^!buy";
            this.SettingInfoVip1 = "Buy a !VIP SLOT for 4 Euro / Month";
            this.SettingInfoVip2 = "!VIP SLOT includes: Reserved Slot, Auto Balancer Whitelist, High Ping Whitelist";
            this.SettingInfoVip3 = "!VIP %player% valid for: %time%";
            this.SettingInfoSay = enumBoolYesNo.Yes;
            this.SettingVipJoin = enumBoolYesNo.No;
            this.SettingVipJoinMsg = "%player% with !VIP SLOT joined the server";
            this.SettingNonVipJoin = enumBoolYesNo.No;
            this.SettingNonVipJoinMsg = "%player% joined the server";
            this.SettingVipSpawn = enumBoolYesNo.Yes;
            this.SettingVipSpawnMsg = "%player% welcome! Enjoy your !VIP SLOT";
            this.SettingVipSpawnMsg2 = "!VIP SLOT valid for: %time%";
            this.SettingVipSpawnMsg3 = String.Empty;
            this.SettingVipSpawnYell = String.Empty;
            this.SettingVipSpawnMsgSay = enumBoolYesNo.Yes;
            this.SettingVipSpawnMsg2Say = enumBoolYesNo.No;
            this.SettingVipSpawnMsg3Say = enumBoolYesNo.No;
            this.SettingVipSpawnYellAll = enumBoolYesNo.No;
            this.SettingVipSpawnMsgDelay = 7;
            this.SettingVipExp = enumBoolYesNo.Yes;
            this.SettingVipExpMsg = "%player% your !VIP SLOT has expired";
            this.SettingVipExpMsg2 = "You can buy a !VIP SLOT on our website";
            this.SettingVipExpYell = "%player% your !VIP SLOT has expired";
            this.SettingVipExpDelay = 7;
            this.SettingForceSync = enumBoolYesNo.No;
            this.SettingMiniManager = enumBoolYesNo.No;
            this.SettingYellDuring = 15;
            this.SettingDBCleaner = 60;
            this.EAGuidTracking = enumBoolYesNo.No;
            this.SettingAggressiveJoin = enumBoolYesNo.No;
            this.SettingAdkatsLog = enumBoolYesNo.No;
            this.SettingAdkatsLogNonVipKick = enumBoolYesNo.No;
            this.SettingAdkatsLogVipChanged = enumBoolYesNo.No;
            this.SettingAdkatsLogVipAggressiveJoinAbuse = enumBoolYesNo.No;
            this.SettingAggressiveJoinKick = "%player% got disconnected to make room for !VIP on full server";
            this.SettingAggressiveJoinMsg = "%player% welcome back! " + System.Environment.NewLine + "You got KICKED randomly because a !VIP joined on full server :/";
            this.SettingAggressiveJoinMsgType = "Private Yell and Private Say";
            this.SettingAggressiveJoinKickAbuse = enumBoolYesNo.Yes;
            this.SettingAggressiveJoinKickAbuseMax = 3;
            this.SettingAggressiveJoinKickAbuseMsg = "%player% you can bypass the server queue on next round (too many rejoins per round).";
            this.SettingAggressiveJoinKickAbuseMsgType = "Private Say";
        }

        //////////////////////
        // BasicPlugin.cs part by PapaCharlie9@gmail.com
        //////////////////////

        public String FormatMessage(String msg, MessageType type)
        {
            String prefix = "[^b" + GetPluginName() + "^n] ";

            if (type.Equals(MessageType.Warning))
                prefix += "^1^bWARNING^0^n: ";
            else if (type.Equals(MessageType.Error))
                prefix += "^1^bERROR^n^0: ";
            else if (type.Equals(MessageType.Exception))
                prefix += "^1^bEXCEPTION^0^n: ";

            return prefix + msg;
        }

        public void LogWrite(String msg)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }

        public void ConsoleWrite(String msg, MessageType type)
        {
            LogWrite(FormatMessage(msg, type));
        }

        public void ConsoleWrite(String msg)
        {
            ConsoleWrite(msg, MessageType.Normal);
        }

        public void ConsoleWarn(String msg)
        {
            ConsoleWrite(msg, MessageType.Warning);
        }

        public void ConsoleError(String msg)
        {
            ConsoleWrite("^8" + msg + "^0", MessageType.Error);
        }

        public void ConsoleException(String msg)
        {
            ConsoleWrite(msg, MessageType.Exception);
        }

        public void DebugWrite(String msg, Int32 level)
        {
            if (this.fDebugLevel >= level) ConsoleWrite(msg, MessageType.Normal);
        }

        public void OnPluginLoadingEnv(List<String> lstPluginEnv)
        {
            this.GetGameType = lstPluginEnv[1].ToUpper();
            if (this.SettingGameType == "AUTOMATIC")
            {
                if (this.GetGameType == "BF3") { this.NewLiner = "\n"; this.SettingGameType = "BF3"; }
                if (this.GetGameType == "BF4") { this.NewLiner = "\n"; this.SettingGameType = "BF4"; }
                if (this.GetGameType == "BFHL") { this.NewLiner = ""; this.SettingGameType = "BFH"; }
                if (this.GetGameType == "BFBC2") { this.NewLiner = ""; this.SettingGameType = "BC2"; }
            }
        }

        public String GetPluginName()
        {
            return "VIP Slot Manager";
        }

        public String GetPluginVersion()
        {
            return "1.0.0.6";
        }

        public String GetPluginAuthor()
        {
            return "maxdralle (maintained by Prophet731)";
        }

        public String GetPluginWebsite()
        {
            return "github.com/procon-plugin/vip-slot-manager";
        }

        public String GetPluginDescription()
        {
            return @"
Credits:<br>
PapaCharlie9 (forum.myrcon.com) - Basic Plugin Template Part (BasicPlugin.cs)<br>
[GWC]XpKiller (forum.myrcon.com) - MySQL Functions (CChatGUIDStatsLogger.inc)<br>
[VdSk]LmaA-aD - Sponsoring BF3 & BFH Gameserver for testing Plugin
<p></p><p></p>
<h2>VIP Slot Manager [BF3,  BF4,  BFH,  BC2]</h2>
<p>If you find this plugin useful, please feel free to donante.</p>
<h2>Description</h2>
<p>This FREE Plugin gives you full control over reserved VIP Slots, with many customizations and features.</p>
";
        }

        public void OnPluginLoaded(String strHostName, String strPort, String strPRoConVersion)
        {
            this.RegisterEvents(this.GetType().Name, "OnServerInfo", "OnPlayerJoin", "OnPlayerDisconnected", "OnPlayerSpawned", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnRoundOver", "OnLevelLoaded", "OnLoadingLevel", "OnReservedSlotsList", "OnReservedSlotsListAggressiveJoin", "OnListPlayers", "OnSquadLeader");
            this.ServerIP = strHostName;
            this.ServerPort = strPort;

            if (this.SettingGameType == "AUTOMATIC")
            {
                if (this.GetGameType == "BF3") { this.SettingGameType = "BF3"; }
                if (this.GetGameType == "BF4") { this.SettingGameType = "BF4"; }
                if (this.GetGameType == "BFHL") { this.SettingGameType = "BFH"; }
                if (this.GetGameType == "BFBC2") { this.SettingGameType = "BC2"; }
            }

            if (this.GetGameType == "BF3") { this.NewLiner = "\n"; }
            if (this.GetGameType == "BF4") { this.NewLiner = "\n"; }
            if (this.GetGameType == "BFHL") { this.NewLiner = ""; }
            if (this.GetGameType == "BFBC2") { this.NewLiner = ""; }
        }

        public void OnPluginEnable()
        {
            this.fIsEnabled = true;
            this.gotVipsGS = false;
            this.firstCheck = false;
            this.SqlTableExist = false;
            this.SyncCounter = 999;
            this.DBCleanerCounter = (new Random().Next(960, 990));
            ConsoleWrite("^b^2Enabled!^0^n");

            if (this.SettingGameType == "AUTOMATIC")
            {
                ConsoleError("[Checkup] ERROR: Please go to the Plugin settings and select a valid 'GameType'. Shutdown Plugin");
                return;
            }
            if (this.SettingGameType != this.GetGameType.Replace("BFHL", "BFH").Replace("BFBC", "BC")) { ConsoleWrite("Info about your current Plugin settings: YOU selected " + this.strBlack(this.SettingGameType) + " as Gametype but Plugin detects " + this.strBlack(this.GetGameType)); }
            ConsoleWrite("Main Settings loaded. ^nServer Group: ^b" + this.SettingGameType + " - " + this.SettingStrSqlServerGroup + "^n");
            if ((this.SettingSyncGs2Sql != "no  (remove from Gameserver)") && (this.SettingSyncGs2Sql != "yes  (30 days first Plugin installation only)")) { ConsoleWrite("Info about your current Plugin settings: If two or more Gameservers use the same 'Server Group' ID then the Plugin setting 'Import NEW VIPs from GS to SQL'^n must be set to 'no (remove)^n'. This setting is also required to use the 'Aggressive Join Kick Protection'."); }

            this.ProconVipList();
            this.GenRegMatch();
            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "3", "9", "20", "procon.protected.plugins.call", "VipSlotManager", "PluginStarter");
            if (this.GetGameType == "BFBC2") { this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "5", "100", "-1", "procon.protected.send", "reservedSlots.list"); } //bfbc2
        }

        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.tasks.remove", "VipSlotManager");
            this.AggressiveJoinAbuseCleaner();
            this.fIsEnabled = false;
            this.SqlVipsActive.Clear();
            this.SqlVipsActiveNamesOnly.Clear();
            this.onJoinSpammer.Clear();
            this.vipsExpired.Clear();
            this.kickedForVip.Clear();
            this.vipmsg.Clear();
            this.playerTeamID.Clear();
            this.playerSquadID.Clear();
            this.SquadLederList.Clear();
            this.Guid2Check.Clear();
            this.GetPlayerGuid.Clear();
            this.vipsGS.Clear();
            this.NameGuidList.Clear();
            this.SyncCounter = 999;
            this.SqlTableExist = false;
            this.gotVipsGS = false;
            this.firstCheck = false;
            ConsoleWrite("^b^8Disabled!^0^n");
        }

        //////////////////////
        // Private & Public Helper Functions
        //////////////////////

        private void DisplayVips()
        {
            Int32 vipcount = 0;
            this.ProconChat("[VIP LIST] [" + this.SettingGameType + " - " + this.SettingStrSqlServerGroup + "] Display VIP list with players remaining time (data from last Sync):");
            foreach (KeyValuePair<String, Int32> tmp_sqlvips in this.SqlVipsActive)
            {
                vipcount++;
                this.ProconChat("[VIP LIST] [" + this.SettingGameType + " - " + this.SettingStrSqlServerGroup + "] " + vipcount.ToString() + ". " + this.strGreen(tmp_sqlvips.Key) + " valid for: " + this.strGetRemTime(tmp_sqlvips.Value));
            }
            ConsoleWrite("^bVIP Slot Manager^n > [VIP LIST] Display VIP list with players remaining time in CHAT tab");
            this.ProconChat("[VIP LIST] Command: ^b/vsm-addvip <full playername> <days>^n   ( e.g. /vsm-addvip SniperBen +30 )");
            this.ProconChat("[VIP LIST] Command: ^b/vsm-removevip <full playername>^n   ( e.g. /vsm-removevip SniperBen )");
            this.ProconChat("[VIP LIST] Command: ^b/vsm-changevip <old playername> <new playername>^n   ( e.g. /vsm-changevip SniperBen SniperBenni )");
            this.ProconChat("[VIP LIST] You can enter these commands in the Procon PC Tool > Chat (say, all)");

            this.ProconChat("[VIP LIST] VIPs online: " + this.vipsCurrentlyOnline.ToString() + "/" + this.SqlVipsActive.Count.ToString());
            if (this.SettingVipExp == enumBoolYesNo.Yes) { this.ProconChat("[VIP LIST] VIPs expired: " + this.vipsExpired.Count.ToString() + " (Players will get a 'VIP Slot Expired' Message on next spawn/join event)"); }
            if ((this.SettingAggressiveJoinKickAbuseMax > 1) && (this.RoundTempVips.Count > 0)) { ConsoleWrite("[VIP LIST] Aggressive Join Abuse Protection > On current round blocked for " + this.RoundTempVips.Count.ToString() + " VIPs: " + String.Join(", ", this.RoundTempVips.ToArray())); }
        }

        private Int32 GetVipTimestamp(String playername)
        {
            if (playername.Length >= 3)
            {
                if (this.SqlVipsActive.ContainsKey(playername))
                {
                    return this.SqlVipsActive[playername];
                }
            }
            return -1;
        }

        private void AdkatsPlayerLog(String playername, String msg)
        {
            if ((playername.Length >= 3) && (msg.Length >= 3) && (this.AdkatsRunning))
            {
                if (this.NameGuidList.ContainsKey(playername))
                {
                    this.ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", "VipSlotManager", JSON.JsonEncode(new Hashtable { { "caller_identity", "VipSlotManager" }, { "response_requested", false }, { "command_type", "player_log" }, { "source_name", "VIP Slot Manager" }, { "target_name", playername }, { "target_guid", this.NameGuidList[playername] }, { "record_message", msg } }));
                }
            }
        }

        public void PluginStarter()
        {
            if ((this.gotVipsGS) && (!this.firstCheck) && (this.fIsEnabled))
            {
                // wait a little bit after layer restart
                DebugWrite("[Task] [PluginStarter] Layer restart detected. Warmup...", 5);
                if (((DateTime.UtcNow - this.LayerStartingTime).TotalSeconds) > this.start_delay)
                {
                    Thread ThreadWorker2 = new Thread(new ThreadStart(delegate ()
                    {
                        this.CheckSettingsSql();
                        this.AdkatsRunning = GetRegisteredCommands().Any(command => command.RegisteredClassname == "AdKats" && command.RegisteredMethodName == "PluginEnabled");
                    }));
                    ThreadWorker2.IsBackground = true;
                    ThreadWorker2.Name = "threadworker2";
                    ThreadWorker2.Start();
                }
            }
        }

        public void CheckSettingsSql()
        {
            // basic checks after plugin enabled
            // check 1. sql logins

            DebugWrite("[Task] [Check] Checking SQL Settings", 5);
            if ((!this.fIsEnabled) || (!this.SqlLoginsOk())) { return; }
            if ((this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes) && (this.SettingSyncGs2Sql != "no  (remove from Gameserver)") && (this.SettingSyncGs2Sql != "no  (ignore)") && (this.SettingSyncGs2Sql != "yes  (30 days first Plugin installation only)"))
            {
                ConsoleWrite("ERROR: 'Aggressive Join Abuse Protection' disabled. To use this function the Plugin setting 'Import NEW VIPS from Gameserver to SQL' must be set to 'NO (remove)'.");
                this.SettingAggressiveJoinKickAbuse = enumBoolYesNo.No;
                this.ExecuteCommand("procon.protected.plugins.setVariable", "VipSlotManager", "Enable Aggressive Join Abuse Protection", "No");
            }

            // check 2. sql database connection
            DebugWrite("[Task] [Check] Try to connect to SQL Server", 5);
            try
            {
                using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                {
                    Con.Open();
                    DebugWrite("[Task] [Check] Test Connection to SQL was successfully", 5);
                    List<MatchCommand> registered = this.GetRegisteredCommands();
                    Boolean tmp_checkAdkatsSettings = false;
                    foreach (MatchCommand command in registered)
                    {
                        if ((command.RegisteredClassname == "AdKats") && (command.RegisteredMethodName == "PluginEnabled")) { tmp_checkAdkatsSettings = true; }
                    }
                    if ((tmp_checkAdkatsSettings) && (this.ServerIP != String.Empty) && (this.ServerPort != String.Empty))
                    {
                        DebugWrite("[Task] [Check] Adkats Plugin detected. Adkats is currently enabled", 5);
                        try
                        {
                            //check adkats plugin conflict
                            //check if table exist or not in SQL database
                            Boolean AdkatsTableExist = false;
                            String SQL = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='adkats_settings' AND table_schema=@Database";
                            DebugWrite("[Task] [Check] Try to get Adkats settings. SQL COMMAND: " + SQL, 5);
                            var tableRows = Con.Query(SQL, new { Database = this.SettingStrSqlDatabase });
                            foreach (var row in tableRows)
                            {
                                String tmp_tablename = (String)row.TABLE_NAME;
                                if (tmp_tablename == "adkats_settings")
                                {
                                    AdkatsTableExist = true;
                                }
                            }
                            if (AdkatsTableExist)
                            {
                                //check 3. adkats plugin, "a16 Orchestration Settings, Feed Reserved Slots" = False
                                SQL = "SELECT `setting_value` FROM `adkats_settings` WHERE setting_name = 'Feed Server Reserved Slots' AND server_id IN (SELECT ServerID as ServerID FROM `" + this.SettingStrSqlDatabase + "`.`tbl_server` WHERE `IP_Address` = @IpAddress)";
                                DebugWrite("[Task] [Check] [AdkatsSettings] Try to get Adkats setting 'Feed Server Reserved Slots'. SQL COMMAND: " + SQL, 5);
                                var settingRows = Con.Query(SQL, new { IpAddress = this.ServerIP + ":" + this.ServerPort });
                                foreach (var row in settingRows)
                                {
                                    String tmp_adkatsSetting = (String)row.setting_value;
                                    if (tmp_adkatsSetting == "True")
                                    {
                                        //problem found! adkats still manage the reserved slot list
                                        ConsoleError("[Task] [Check] [AdkatsSettings] ERROR: Plugin conflict with current settings from Adkats!");
                                        ConsoleError("[Task] [Check] [AdkatsSettings] ERROR: Adkats Plugin still manage the reserved VIP slot. IMPORTANT: You must disable this function in Adkats! Open the settings from Adkats Plugin. Then go to ^0^8^bAdkats  >  A16. Orchestration Settings  >  Feed Server Reserved Slots  > False^n^0");
                                        //shutdown plugin
                                        this.ExecuteCommand("procon.protected.plugins.enable", "VipSlotManager", "False");
                                        return;
                                    }
                                    else if (tmp_adkatsSetting == "False")
                                    {
                                        DebugWrite("[Task] [Check] [AdkatsSettings] Adkats Plugin do not manage the reserved VIP slot", 5);
                                    }
                                }
                            }
                        }
                        catch (Exception c)
                        {
                            ConsoleError("[Task] [Check] Can not read the current settings from Adkats Plugin. SQL Error: " + c);
                        }
                    }

                    if (Con.State == ConnectionState.Open)
                    {
                        DebugWrite("[Task] [Check] Close SQL Connection (Con)", 5);
                        Con.Close();
                    }
                }

                //startup test finished
                this.firstCheck = true;
                DebugWrite("[Task] Checkup completed.", 3);
                this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "5", "60", "-1", "procon.protected.plugins.call", "VipSlotManager", "updnow");
                this.TableBuilder();
            }
            catch (Exception c)
            {
                ConsoleError("[Task] [Check] FATAL ERROR: CAN NOT CONNECT TO SQL SERVER! Please check your Plugin settings 'SQL Server Details' (Host IP, Port, Database, Username, PW) and try again. Maybe the provider of your Procon Layer block the connection (server firewall settings).");
                ConsoleError("[Task] [Check] Error (Con): " + c);
                ConsoleError("[Task] [Check] Error");
                ConsoleError("[Task] [Check] Shutdown Plugin...");
                this.ExecuteCommand("procon.protected.plugins.enable", "VipSlotManager", "False");
                return;
            }
        }

        public void updnow()
        {
            if (!this.fIsEnabled) { return; }
            this.SyncCounter++;
            if (this.SyncCounter >= this.SettingSyncInterval)
            {
                this.SyncCounter = 0;
                DebugWrite("[Task] Sync VIP players from SQL database with VIP players from Gameserver", 5);
                this.SyncVipList();
            }
            else
            {
                this.DBCleanerCounter++;
                if (this.DBCleanerCounter >= 1000)
                {
                    this.DBCleanerCounter = 0;
                    DebugWrite("[Task] Start Auto Database Cleaner", 5);
                    this.DatabaseCleaner();
                }
            }

            if (this.SettingAggressiveJoin == enumBoolYesNo.Yes)
            {
                // Enable / Disable Aggressive Join
                if (this.AggressiveJoin)
                {
                    if ((this.CurrentGameMode == "ConquestLarge0") || (this.CurrentGameMode == "ConquestSmall0") || (this.CurrentGameMode == "Chainlink0"))
                    {
                        if (this.ticketLoserTeam <= 120)
                        {
                            DebugWrite("[Task] Aggressive Join for VIPs disabled (less than 120 tickets remaining).", 4);
                            this.ProconChat("Aggressive Join for VIPs disabled (less than 120 tickets remaining).");
                            this.ExecuteCommand("procon.protected.send", "reservedSlotsList.aggressiveJoin", "false");
                        }
                    }
                    else if (this.CurrentGameMode == "TeamDeathMatch0")
                    {
                        if (this.ticketLoserTeam <= 30)
                        {
                            DebugWrite("[Task] Aggressive Join for VIPs disabled (less than 30 kills remaining).", 4);
                            this.ProconChat("Aggressive Join for VIPs disabled (less than 30 kills remaining).");
                            this.ExecuteCommand("procon.protected.send", "reservedSlotsList.aggressiveJoin", "false");
                        }
                    }
                }
                else if (((DateTime.UtcNow - this.CurrentRoundEndTime).TotalSeconds) > 50)
                {
                    if ((this.CurrentGameMode == "ConquestLarge0") || (this.CurrentGameMode == "ConquestSmall0") || (this.CurrentGameMode == "Chainlink0"))
                    {
                        if (this.ticketLoserTeam > 120)
                        {
                            DebugWrite("[Task] Aggressive Join for VIPs enabled", 4);
                            this.ProconChat("Aggressive Join for VIPs enabled");
                            this.ExecuteCommand("procon.protected.send", "reservedSlotsList.aggressiveJoin", "true");
                        }
                    }
                    else if (this.CurrentGameMode == "TeamDeathMatch0")
                    {
                        if (this.ticketLoserTeam > 30)
                        {
                            DebugWrite("[Task] Aggressive Join for VIPs enabled", 4);
                            this.ProconChat("Aggressive Join for VIPs enabled");
                            this.ExecuteCommand("procon.protected.send", "reservedSlotsList.aggressiveJoin", "true");
                        }
                    }
                    else
                    {
                        DebugWrite("[Task] Aggressive Join for VIPs enabled", 4);
                        this.ProconChat("Aggressive Join for VIPs enabled");
                        this.ExecuteCommand("procon.protected.send", "reservedSlotsList.aggressiveJoin", "true");
                    }
                }
            }
        }

        private void RoundEndCleaner()
        {
            if (((DateTime.UtcNow - this.CurrentRoundEndTime).TotalSeconds) > 120)
            {
                this.CurrentRoundEndTime = DateTime.UtcNow;
                if (this.SettingProconRulzIni == enumBoolYesNo.Yes)
                {
                    DebugWrite("[OnRoundOver] Add Task to write proconrulz.ini in 15 secouds.", 5);
                    this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "15", "1", "1", "procon.protected.plugins.call", "VipSlotManager", "FileWriteProconRulz");
                }
                DebugWrite("[OnRoundOver] VIPs online: " + this.vipsCurrentlyOnline.ToString() + "/" + this.SqlVipsActive.Count.ToString(), 4);
                this.ProconChat("VIPs online: " + this.vipsCurrentlyOnline.ToString() + "/" + this.SqlVipsActive.Count.ToString());
                if ((this.SettingAggressiveJoinKickAbuseMax > 1) && (this.RoundTempVips.Count > 0))
                {
                    DebugWrite("[OnRoundOver] [AggressiveJoinAbuseProtection] 'Aggressive Join Kick' was temporary blocked for " + this.RoundTempVips.Count.ToString() + " VIPs: " + String.Join(", ", this.RoundTempVips.ToArray()), 3);
                    this.ProconChat("'Aggressive Join Kick' was temporary blocked for " + this.RoundTempVips.Count.ToString() + " VIPs: " + String.Join(", ", this.RoundTempVips.ToArray()));
                }

                this.AggressiveJoinAbuseCleaner();
                this.vipmsg.Clear();
                this.playerTeamID.Clear();
                this.playerSquadID.Clear();
                this.GetPlayerGuid.Clear();
                this.Guid2Check.Clear();
                this.onJoinSpammer.Clear();
                this.SquadLederList.Clear();
                this.NameGuidList.Clear();
                this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "80", "1", "1", "procon.protected.send", "admin.listPlayers", "all");
            }
        }

        private void AggressiveJoinAbuseCleaner()
        {
            Boolean tmp_upd_gs = false;
            if (this.RoundTempVips.Count > 0)
            {
                // switch blocked vips to full vips
                DebugWrite("[AggressiveJoinAbuseCleaner] Reactivate 'Aggressive Join Kick' privilege. for all vaild VIPs on Gameserver.", 4);
                foreach (String vipplayer in this.RoundTempVips)
                {
                    if (this.GetVipTimestamp(vipplayer) != -1)
                    {
                        // add vip slot
                        DebugWrite("[AggressiveJoinAbuseCleaner] Add VIP Slot: " + this.strGreen(vipplayer) + " to Gameserver.", 5);
                        tmp_upd_gs = true;
                        this.ProconVipAdd(vipplayer);
                    }
                    else
                    {
                        // remove semi vip slot
                        DebugWrite("[AggressiveJoinAbuseCleaner] Remove Semi VIP Slot: " + this.strBlack(vipplayer) + " from Gameserver.", 4);
                        tmp_upd_gs = true;
                        this.ProconVipRemove(vipplayer);
                    }
                }
                if (tmp_upd_gs)
                {
                    this.ProconVipSave();
                    this.ProconVipList();
                }
            }
            this.RoundTempVips.Clear();
            this.AggressiveVips.Clear();
        }

        private void ProconVipRemove(String playername)
        {
            if (this.GetGameType == "BFBC2")
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlots.removePlayer", playername);
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.remove", playername);
            }
        }

        private void ProconVipAdd(String playername)
        {
            if (this.GetGameType == "BFBC2")
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlots.addPlayer", playername);
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.add", playername);
            }
        }

        private void ProconVipSave()
        {
            if (this.GetGameType == "BFBC2")
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlots.save");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.save");
            }
        }

        private void ProconVipList()
        {
            if (this.GetGameType == "BFBC2")
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlots.list");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
            }
        }

        private void GenRegMatch()
        {
            Int32 tmp_x = 0;
            String tmp_regmatch = "^/?!vip|^/?!slot|^/?!reserved|^/?!buy";
            foreach (String chatcommand in this.SettingInfoCommands.Split(','))
            {
                if (chatcommand.Length >= 3)
                {
                    tmp_x++;
                    if (tmp_x == 1)
                    {
                        tmp_regmatch = "^/?" + chatcommand.ToLower();
                    }
                    else
                    {
                        tmp_regmatch = tmp_regmatch + "|^/?" + chatcommand.ToLower();
                    }
                }
            }
            this.SettingInfoCmdRegMatch = tmp_regmatch;
        }

        public void FileWriteProconRulz()
        {
            if ((!this.fIsEnabled) || (!this.firstCheck)) { return; }
            Thread ThreadWorker1 = new Thread(new ThreadStart(delegate ()
            {
                String tmp_currentvips = String.Empty;
                String tmp_tempData = String.Empty;
                Boolean tmp_parser = true;
                Int32 tmp_line_counter = 0;
                String path = "Configs\\" + this.ServerIP + "_" + this.ServerPort + "_proconrulz.ini";
                if ((this.ServerIP == String.Empty) || (this.ServerPort == String.Empty))
                {
                    ConsoleError("[FileWriteProconRulz] ^bERROR:^n Gameserver IP / Port is missing. Can not create filename");
                    return;
                }
                DebugWrite("[FileWriteProconRulz] Write valid VIPs in proconrulz.ini file. Path: " + path, 5);
                // create list with valid vip player and proconrulz timestamp
                tmp_currentvips = "[vipslotmanager]" + Environment.NewLine;
                if (this.SqlVipsActive.Count > 0)
                {
                    foreach (KeyValuePair<String, Int32> tmp_sqlvips in this.SqlVipsActive)
                    {
                        tmp_line_counter++;
                        // proconrulz timestamp since 01.01.2012 in SECONDS
                        tmp_currentvips += tmp_sqlvips.Key + "=" + (tmp_sqlvips.Value + Convert.ToInt32(((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds) - (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds) - 1325376000).ToString();
                        if (tmp_line_counter < this.SqlVipsActive.Count)
                        {
                            tmp_currentvips += Environment.NewLine;
                        }
                    }
                }
                else
                {
                    tmp_currentvips = String.Empty;
                }

                tmp_line_counter = 0;
                try
                {
                    if (File.Exists(path))
                    {
                        String[] readText = File.ReadAllLines(path);
                        if (readText.Length != 0)
                        {
                            foreach (String s in readText)
                            {
                                // parse proconrulz.ini file
                                tmp_line_counter++;
                                if (s == "[vipslotmanager]") { tmp_parser = false; }
                                if ((s.StartsWith("[")) && (s != "[vipslotmanager]")) { tmp_parser = true; }
                                if (tmp_parser)
                                {
                                    tmp_tempData += s;
                                    if (tmp_line_counter < readText.Length)
                                    {
                                        tmp_tempData += Environment.NewLine;
                                    }
                                }
                            }
                        }
                    }
                    if (tmp_tempData.Length > 1)
                    {
                        File.WriteAllText(path, tmp_currentvips + Environment.NewLine + tmp_tempData);
                    }
                    else
                    {
                        File.WriteAllText(path, tmp_currentvips);
                    }
                }
                catch (Exception e)
                {
                    ConsoleError("[FileWriteProconRulz] Can NOT write proconrulz.ini file. Requires Read+Write file permission. Path: " + path + "    ERROR: " + e);
                }
            }));
            ThreadWorker1.IsBackground = true;
            ThreadWorker1.Name = "threadworker1";
            ThreadWorker1.Start();
        }

        public Boolean isAdmin(String playername)
        {
            try
            {
                CPrivileges AdminPrivis = this.GetAccountPrivileges(playername);
                if (AdminPrivis.CanEditReservedSlotsList)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        private String SqlLogin() { return "Server=" + this.SettingStrSqlHostname + ";" + "Port=" + this.SettingStrSqlPort + ";" + "Database=" + this.SettingStrSqlDatabase + ";" + "Uid=" + this.SettingStrSqlUsername + ";" + "Pwd=" + this.SettingStrSqlPassword + ";" + "Connection Timeout=5;"; }

        private String strGreen(String StrInp) { return "^b^2" + StrInp + "^0^n"; }

        private String strNoGreen(String StrInp) { return StrInp.Replace("^b^2", "").Replace("^0^n", ""); }

        private String strBlack(String StrInp) { return "^b" + StrInp + "^n"; }

        private Int32 intUtcTimestamp() { return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds); }

        private String strGetRemTime(Int32 PlayerTimeStamp)
        {
            Int32 tmp_days = ((PlayerTimeStamp - intUtcTimestamp()) / 86400);
            Int32 tmp_hours = ((PlayerTimeStamp - intUtcTimestamp()) / 3600);
            String tmp_msg = String.Empty;
            if (tmp_days > 2000)
            {
                tmp_msg = "permanent";
            }
            else if (tmp_days > 1)
            {
                tmp_msg = tmp_days.ToString() + " days " + (tmp_hours - (tmp_days * 24)).ToString() + " hours";
            }
            else if (tmp_days == 1)
            {
                tmp_msg = tmp_days.ToString() + " day " + (tmp_hours - (tmp_days * 24)).ToString() + " hours";
            }
            else if (tmp_hours > 1)
            {
                tmp_msg = tmp_hours.ToString() + " hours";
            }
            else if (tmp_hours > 0)
            {
                tmp_msg = tmp_hours.ToString() + " hour";
            }
            else if (tmp_hours == 0)
            {
                tmp_msg = "> 1 hour";
            }
            return tmp_msg;
        }

        private String strYellDur()
        {
            if (this.GetGameType == "BFBC2")
            {
                return (this.SettingYellDuring * 1000).ToString();
            }
            else
            {
                return this.SettingYellDuring.ToString();
            }
        }

        public void AddRoundSemiVip(String soldierName)
        {
            if ((!this.fIsEnabled) || (!this.firstCheck) || (soldierName.Length < 4)) { return; }
            if ((!this.RoundTempVips.Contains(soldierName)) && (this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes))
            {
                DebugWrite("[AddRoundSemiVip] Player " + this.strBlack(soldierName) + " added as Semi VIP (valid for current round / rejoin).", 4);
                this.RoundTempVips.Add(soldierName);
                this.ProconVipAdd(soldierName);
                this.ProconVipSave();
                this.ProconVipList();
            }
        }

        //////////////////////
        // Send In-Game Messages (say,yell,...)
        //////////////////////

        public void PlayerSayMsg(String target, String message)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            this.ExecuteCommand("procon.protected.send", "admin.say", this.strNoGreen(message.Replace(System.Environment.NewLine, " ")), "player", target);
            this.ExecuteCommand("procon.protected.chat.write", "(PlayerSay " + target + ") " + message.Replace(System.Environment.NewLine, " "));
        }

        public void PlayerSayMsg(String target, String message, Int32 msgdelay)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            if (msgdelay == 0)
            {
                PlayerSayMsg(target, message);
                return;
            }
            if (message.Length < 3) { return; }
            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", msgdelay.ToString(), "1", "1", "procon.protected.send", "admin.say", this.strNoGreen(message.Replace(System.Environment.NewLine, " ")), "player", target);
            this.ExecuteCommand("procon.protected.chat.write", "(PlayerSay " + target + ") " + message.Replace(System.Environment.NewLine, " "));
        }

        public void SayMsg(String message)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            this.ExecuteCommand("procon.protected.send", "admin.say", this.strNoGreen(message.Replace(System.Environment.NewLine, " ")), "all");
            this.ExecuteCommand("procon.protected.chat.write", message.Replace(System.Environment.NewLine, " "));
        }

        public void SayMsg(String message, Int32 msgdelay)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            if (msgdelay == 0)
            {
                SayMsg(message);
                return;
            }
            if (message.Length < 3) { return; }
            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", msgdelay.ToString(), "1", "1", "procon.protected.send", "admin.say", this.strNoGreen(message.Replace(System.Environment.NewLine, " ")), "all");
            this.ExecuteCommand("procon.protected.chat.write", message.Replace(System.Environment.NewLine, " "));
        }

        public void PlayerYellMsg(String target, String message)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            this.ExecuteCommand("procon.protected.send", "admin.yell", "[VIP SLOT] " + this.NewLiner + this.strNoGreen(message.Replace(System.Environment.NewLine, this.NewLiner)), this.strYellDur(), "player", target);
            this.ExecuteCommand("procon.protected.chat.write", "(PlayerYell " + target + ") " + message.Replace(System.Environment.NewLine, "  -  "));
        }

        public void PlayerYellMsg(String target, String message, Int32 msgdelay)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            if (msgdelay == 0)
            {
                PlayerYellMsg(target, message);
                return;
            }
            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", msgdelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", "[VIP SLOT] " + this.NewLiner + this.strNoGreen(message.Replace(System.Environment.NewLine, this.NewLiner)), this.strYellDur(), "player", target);
            this.ExecuteCommand("procon.protected.chat.write", "(PlayerYell " + target + ") " + message.Replace(System.Environment.NewLine, "  -  "));
        }

        public void YellMsg(String message)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            this.ExecuteCommand("procon.protected.send", "admin.yell", "[VIP SLOT] " + this.NewLiner + this.strNoGreen(message.Replace(System.Environment.NewLine, this.NewLiner)), this.strYellDur(), "all");
            this.ExecuteCommand("procon.protected.chat.write", "(Yell) " + message.Replace(System.Environment.NewLine, "  -  "));
        }

        public void YellMsg(String message, Int32 msgdelay)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            if (msgdelay == 0)
            {
                YellMsg(message);
                return;
            }
            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", msgdelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", "[VIP SLOT] " + this.NewLiner + this.strNoGreen(message.Replace(System.Environment.NewLine, this.NewLiner)), this.strYellDur(), "all");
            this.ExecuteCommand("procon.protected.chat.write", "(Yell) " + message.Replace(System.Environment.NewLine, "  -  "));
        }

        public void ProconChat(String message)
        {
            if ((!this.fIsEnabled) || (message.Length < 3)) { return; }
            this.ExecuteCommand("procon.protected.chat.write", "^bVIP Slot Manager^n > " + message.Replace(System.Environment.NewLine, " "));
        }
    }
} // end namespace PRoConEvents
