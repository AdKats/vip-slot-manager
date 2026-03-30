using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using PRoCon.Core;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
    public partial class VipSlotManager
    {
        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("1. MySQL Details|Host", this.SettingStrSqlHostname.GetType(), this.SettingStrSqlHostname));
            lstReturn.Add(new CPluginVariable("1. MySQL Details|Port", this.SettingStrSqlPort.GetType(), this.SettingStrSqlPort));
            lstReturn.Add(new CPluginVariable("1. MySQL Details|Database", this.SettingStrSqlDatabase.GetType(), this.SettingStrSqlDatabase));
            lstReturn.Add(new CPluginVariable("1. MySQL Details|Username", this.SettingStrSqlUsername.GetType(), this.SettingStrSqlUsername));
            lstReturn.Add(new CPluginVariable("1. MySQL Details|Password", this.SettingStrSqlPassword.GetType(), this.SettingStrSqlPassword));
            lstReturn.Add(new CPluginVariable("2. Main Settings|Gameserver Type", "enum.SettingGameType(AUTOMATIC|BF3|BF4|BFH|BC2)", this.SettingGameType));
            lstReturn.Add(new CPluginVariable("2. Main Settings|Server Group (1-99)", this.SettingStrSqlServerGroup.GetType(), this.SettingStrSqlServerGroup));
            lstReturn.Add(new CPluginVariable("3. Sync Settings|Sync Interval between SQL and Gameserver. Minutes (2-60)", this.SettingSyncInterval.GetType(), this.SettingSyncInterval));
            lstReturn.Add(new CPluginVariable("3. Sync Settings|Import NEW VIPS from Gameserver to SQL", "enum.SettingSyncGs2Sql(yes  (30 days first Plugin installation only)|no  (ignore)|no  (remove from Gameserver)|yes  (as inactive)|yes  (for 7 days)|yes  (for 30 days)|yes  (for 90 days)|yes  (for 365 days)|yes  (permanent))", this.SettingSyncGs2Sql));
            lstReturn.Add(new CPluginVariable("4. Commands|Enable In-Game Admin Commands?", typeof(enumBoolYesNo), this.SettingAdminCmd));
            lstReturn.Add(new CPluginVariable("4. Commands|Enable Commands for other Plugins?", typeof(enumBoolYesNo), this.SettingPluginCmd));
            lstReturn.Add(new CPluginVariable("4. Commands|Enable In-Game VIP Commands?", typeof(enumBoolYesNo), this.SettingVipCmd));
            if (this.SettingVipCmd == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("4. Commands|  - Enable VIP Command !lead", typeof(enumBoolYesNo), this.SettingLeadCmd));
                if (this.SettingLeadCmd == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("4. Commands|     - VIP can take from other VIP", typeof(enumBoolYesNo), this.SettingIgnoreVipLeader));
                    lstReturn.Add(new CPluginVariable("4. Commands|     - Enforce Commo Rose Request from VIPs", typeof(enumBoolYesNo), this.SettingLeadByCRose));
                }
                lstReturn.Add(new CPluginVariable("4. Commands|  - Enable VIP Command !killme", typeof(enumBoolYesNo), this.SettingKillmeCmd));
                lstReturn.Add(new CPluginVariable("4. Commands|  - Enable VIP Command !switchme", typeof(enumBoolYesNo), this.SettingSwitchmeCmd));
            }
            lstReturn.Add(new CPluginVariable("3. Sync Settings|On Round End write VIPs in proconrulz.ini file?", typeof(enumBoolYesNo), this.SettingProconRulzIni));
            lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|Show VIP Slot Infos on Chat request?", typeof(enumBoolYesNo), this.SettingChatReq));
            if (this.SettingChatReq == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|Chat Commands", this.SettingInfoCommands.GetType(), this.SettingInfoCommands));
                lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|First Info Message", this.SettingInfoVip1.GetType(), this.SettingInfoVip1));
                lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|Second Info Message", this.SettingInfoVip2.GetType(), this.SettingInfoVip2));
                lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|Private Message to valid VIPs only", this.SettingInfoVip3.GetType(), this.SettingInfoVip3));
                lstReturn.Add(new CPluginVariable("5. Notify - Chat Request|Send to all Players?", typeof(enumBoolYesNo), this.SettingInfoSay));
            }
            lstReturn.Add(new CPluginVariable("6. Notify - Join Message|Show Message when VIP is joining?", typeof(enumBoolYesNo), this.SettingVipJoin));
            if (this.SettingVipJoin == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("6. Notify - Join Message|  - VIP Player Join Message", this.SettingVipJoinMsg.GetType(), this.SettingVipJoinMsg));
            }
            lstReturn.Add(new CPluginVariable("6. Notify - Join Message|Show Message when NON-VIP is joining?", typeof(enumBoolYesNo), this.SettingNonVipJoin));
            if (this.SettingNonVipJoin == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("6. Notify - Join Message|  - NON-VIP Join Message", this.SettingNonVipJoinMsg.GetType(), this.SettingNonVipJoinMsg));
            }
            lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Show Message when VIP is spawning (each round first spawn)?", typeof(enumBoolYesNo), this.SettingVipSpawn));
            if (this.SettingVipSpawn == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|First VIP Spawn Message", this.SettingVipSpawnMsg.GetType(), this.SettingVipSpawnMsg));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Second VIP Spawn Message", this.SettingVipSpawnMsg2.GetType(), this.SettingVipSpawnMsg2));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Third VIP Spawn Message", this.SettingVipSpawnMsg3.GetType(), this.SettingVipSpawnMsg3));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|VIP Spawn Yell", this.SettingVipSpawnYell.GetType(), this.SettingVipSpawnYell));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Send First Msg to all Players?", typeof(enumBoolYesNo), this.SettingVipSpawnMsgSay));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Send Second Msg to all Players?", typeof(enumBoolYesNo), this.SettingVipSpawnMsg2Say));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Send Third Msg to all Players?", typeof(enumBoolYesNo), this.SettingVipSpawnMsg3Say));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Send Yell to all Players?", typeof(enumBoolYesNo), this.SettingVipSpawnYellAll));
                lstReturn.Add(new CPluginVariable("7. Notify - Spawn Message|Send Message Delay in sec. (0-20)", this.SettingVipSpawnMsgDelay.GetType(), this.SettingVipSpawnMsgDelay));
            }
            lstReturn.Add(new CPluginVariable("8. Notify - VIP Slot Expired|Show Private Message when VIP Slot expired?", typeof(enumBoolYesNo), this.SettingVipExp));
            if (this.SettingVipExp == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("8. Notify - VIP Slot Expired|First Slot Expired Message", this.SettingVipExpMsg.GetType(), this.SettingVipExpMsg));
                lstReturn.Add(new CPluginVariable("8. Notify - VIP Slot Expired|Second Slot Expired Message", this.SettingVipExpMsg2.GetType(), this.SettingVipExpMsg2));
                lstReturn.Add(new CPluginVariable("8. Notify - VIP Slot Expired|Slot Expired Yell", this.SettingVipExpYell.GetType(), this.SettingVipExpYell));
                lstReturn.Add(new CPluginVariable("8. Notify - VIP Slot Expired|Delay for send Message in sec. (0-20)", this.SettingVipExpDelay.GetType(), this.SettingVipExpDelay));
            }
            lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|Temporary disable the 'Aggressive Join' close on round end (Conquest, TDM and Chainlink only)", typeof(enumBoolYesNo), this.SettingAggressiveJoin));
            lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|Public Say Message when NON-VIP got kicked for VIP", this.SettingAggressiveJoinKick.GetType(), this.SettingAggressiveJoinKick));
            lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|Private Message after NON-VIP got kicked and rejoins", this.SettingAggressiveJoinMsg.GetType(), this.SettingAggressiveJoinMsg));
            lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|  - Send Private Message as:", "enum.SettingAggressiveJoinMsgType(Private Yell and Private Say|Private Yell|Private Say|Say to all Players)", this.SettingAggressiveJoinMsgType));
            lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|Enable Aggressive Join Abuse Protection", typeof(enumBoolYesNo), this.SettingAggressiveJoinKickAbuse));
            if (this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|  - Each VIP can rejoin with an Aggressive Join Kick on full server maximal: (# per round)", this.SettingAggressiveJoinKickAbuseMax.GetType(), this.SettingAggressiveJoinKickAbuseMax));
                lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|  - Private Message to target VIP if he triggered his max. threshold per round", this.SettingAggressiveJoinKickAbuseMsg.GetType(), this.SettingAggressiveJoinKickAbuseMsg));
                lstReturn.Add(new CPluginVariable("8. Server - Aggressive Join for VIPs|  - Send Private Msg as:", "enum.SettingAggressiveJoinKickAbuseMsgType(Private Yell and Private Say|Private Yell|Private Say)", this.SettingAggressiveJoinKickAbuseMsgType));
            }
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Debug Level (1-5)", this.fDebugLevel.GetType(), this.fDebugLevel));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Force Sync SQL and Gameserver NOW", typeof(enumBoolYesNo), this.SettingForceSync));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Mini Manager - Print VIP list with time", typeof(enumBoolYesNo), this.SettingMiniManager));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|During for Yell and PYell in sec. (5-60)", this.SettingYellDuring.GetType(), this.SettingYellDuring));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Auto DB Cleaner (set expired VIPs without join event to inactive after # days)", this.SettingDBCleaner.GetType(), this.SettingDBCleaner));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Enable EA Guid Tracking and update playername changes", typeof(enumBoolYesNo), this.EAGuidTracking));
            lstReturn.Add(new CPluginVariable("9. Advanced Settings|Enable Advanced Log to Adkats", typeof(enumBoolYesNo), this.SettingAdkatsLog));
            if (this.SettingAdkatsLog == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("9. Advanced Settings|  - NON-VIP got kicked for VIP", typeof(enumBoolYesNo), this.SettingAdkatsLogNonVipKick));
                lstReturn.Add(new CPluginVariable("9. Advanced Settings|  - VIP changed his playername", typeof(enumBoolYesNo), this.SettingAdkatsLogVipChanged));
                lstReturn.Add(new CPluginVariable("9. Advanced Settings|  - VIP triggered Aggressive Join Abuse Protection (min. threshold from settings: 2)", typeof(enumBoolYesNo), this.SettingAdkatsLogVipAggressiveJoinAbuse));
            }
            return lstReturn;
        }

        public void SetPluginVariable(String strVariable, String strValue)
        {
            Boolean layerReady = (((DateTime.UtcNow - this.LayerStartingTime).TotalSeconds) > 30);
            if (Regex.Match(strVariable, @"Debug Level").Success)
            {
                Int32 tmp = 3;
                Int32.TryParse(strValue, out tmp);
                if (tmp >= 0 && tmp <= 5)
                {
                    this.fDebugLevel = tmp;
                }
                else
                {
                    ConsoleError("Invalid value for Debug Level: '" + strValue + "'. It must be a number between 1 and 5. (e.g.: 3)");
                }
            }
            else if (Regex.Match(strVariable, @"Host").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                if (strValue.Length <= 100) { this.SettingStrSqlHostname = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Port").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                Int32 tmpport = 3306;
                Int32.TryParse(strValue, out tmpport);
                if (tmpport > 0 && tmpport < 65536)
                {
                    this.SettingStrSqlPort = tmpport.ToString();
                }
                else
                {
                    ConsoleError("Invalid value for MySQL Port: '" + strValue + "'. Port must be a number between 1 and 65535. (e.g.: 3306)");
                }
            }
            else if (Regex.Match(strVariable, @"Database").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                if (strValue.Length <= 100) { this.SettingStrSqlDatabase = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Username").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                if (strValue.Length <= 100) { this.SettingStrSqlUsername = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Password").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                if (strValue.Length <= 100) { this.SettingStrSqlPassword = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Gameserver Type").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
                this.SettingGameType = strValue;
                if (strValue == "AUTOMATIC")
                {
                    if (this.GetGameType == "BF3") { this.NewLiner = "\n"; this.SettingGameType = "BF3"; }
                    if (this.GetGameType == "BF4") { this.NewLiner = "\n"; this.SettingGameType = "BF4"; }
                    if (this.GetGameType == "BFHL") { this.NewLiner = ""; this.SettingGameType = "BFH"; }
                    if (this.GetGameType == "BFBC2") { this.NewLiner = ""; this.SettingGameType = "BC2"; }
                }
                if (strValue == "BF3") { this.NewLiner = "\n"; }
                if (strValue == "BF4") { this.NewLiner = "\n"; }
                if (strValue == "BFH") { this.NewLiner = ""; }
                if (strValue == "BC2") { this.NewLiner = ""; }
            }
            else if (Regex.Match(strVariable, @"Server Group").Success)
            {
                if ((this.fIsEnabled) && (layerReady) && (this.firstCheck)) { ConsoleError("Setting 'Server Group' locked! Please disable the Plugin and try again..."); return; }
                Int32 tmpserverid = 1;
                Int32.TryParse(strValue, out tmpserverid);
                if (tmpserverid > 0 && tmpserverid < 100)
                {
                    this.SettingStrSqlServerGroup = tmpserverid.ToString();
                }
                else
                {
                    ConsoleError("Invalid value for MySQL Server Group. It must be a number between 1 and 99. (e.g.: 1)");
                }
            }
            else if (Regex.Match(strVariable, @"Delay for send Message").Success)
            {
                Int32 tmpMsgDel2 = 0;
                Int32.TryParse(strValue, out tmpMsgDel2);
                if (tmpMsgDel2 >= 0 && tmpMsgDel2 < 21)
                {
                    this.SettingVipExpDelay = tmpMsgDel2;
                }
                else
                {
                    ConsoleError("Invalid value for Send Message Delay. It must be a number between 0 and 20. (e.g.: 0 for no delay)");
                }
            }
            else if (Regex.Match(strVariable, @"Sync Interval between SQL and Gameserver").Success)
            {
                Int32 tmpIntr = 0;
                Int32.TryParse(strValue, out tmpIntr);
                if (tmpIntr >= 2 && tmpIntr < 61)
                {
                    this.SettingSyncInterval = tmpIntr;
                }
                else
                {
                    ConsoleError("Invalid value for Sync Interval. It must be a number between 2 and 60");
                }
            }
            else if (Regex.Match(strVariable, @"Import NEW VIPS from Gameserver to SQL").Success)
            {
                this.SettingSyncGs2Sql = strValue;
            }
            else if (Regex.Match(strVariable, @"On Round End write VIPs in proconrulz.ini file?").Success)
            {
                this.SettingProconRulzIni = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Game Admin Commands?").Success)
            {
                this.SettingAdminCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Enable Commands for other Plugins").Success)
            {
                this.SettingPluginCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Game VIP Commands?").Success)
            {
                this.SettingVipCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"lead").Success)
            {
                this.SettingLeadCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"VIP can take from other VIP").Success)
            {
                this.SettingIgnoreVipLeader = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Enforce Commo Rose Request from VIPs").Success)
            {
                this.SettingLeadByCRose = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"killme").Success)
            {
                this.SettingKillmeCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"switchme").Success)
            {
                this.SettingSwitchmeCmd = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Show VIP Slot Infos on Chat request?").Success)
            {
                this.SettingChatReq = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Chat Commands").Success)
            {
                if ((strValue.Length < 3) || (strValue.Length > 100))
                {
                    this.SettingInfoCommands = "!vip,!slot,!reserved,!buy";
                    GenRegMatch();
                }
                else
                {
                    this.SettingInfoCommands = strValue.Replace(System.Environment.NewLine, ",").Replace(" ", "").Replace(";", ",").Replace(",,", ",");
                    GenRegMatch();
                }
            }
            else if (Regex.Match(strVariable, @"First Info Message").Success)
            {
                if ((strValue.Length >= 3) && (strValue.Length <= 100))
                {
                    this.SettingInfoVip1 = strValue.Replace(System.Environment.NewLine, "");
                }
                else
                {
                    ConsoleError("Invalid value for First Info Message (min. 3 and max. 100 chararcters)");
                }
            }
            else if (Regex.Match(strVariable, @"Second Info Message").Success)
            {
                this.SettingInfoVip2 = strValue.Replace(System.Environment.NewLine, "");
            }
            else if (Regex.Match(strVariable, @"Private Message to valid VIPs only").Success)
            {
                this.SettingInfoVip3 = strValue;
            }
            else if (Regex.Match(strVariable, @"Send to all Players?").Success)
            {
                this.SettingInfoSay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Show Message when VIP is joining?").Success)
            {
                this.SettingVipJoin = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"  - VIP Player Join Message").Success)
            {
                if ((strValue.Length < 3) || (strValue.Length > 100))
                {
                    this.SettingVipJoinMsg = "%player% with !VIP SLOT joined the server";
                }
                else
                {
                    this.SettingVipJoinMsg = strValue.Replace(System.Environment.NewLine, "");
                }
            }
            else if (Regex.Match(strVariable, @"Show Message when NON-VIP is joining?").Success)
            {
                this.SettingNonVipJoin = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"  - NON-VIP Join Message").Success)
            {
                if ((strValue.Length < 3) || (strValue.Length > 100))
                {
                    this.SettingNonVipJoinMsg = "%player% joined the server";
                }
                else
                {
                    this.SettingNonVipJoinMsg = strValue.Replace(System.Environment.NewLine, "");
                }
            }
            else if (Regex.Match(strVariable, @"Show Message when VIP is spawning").Success)
            {
                this.SettingVipSpawn = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"First VIP Spawn Message").Success)
            {
                if ((strValue.Length < 3) || (strValue.Length > 100))
                {
                    this.SettingVipSpawnMsg = String.Empty;
                }
                else
                {
                    this.SettingVipSpawnMsg = strValue;
                }
            }
            else if (Regex.Match(strVariable, @"Second VIP Spawn Message").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipSpawnMsg2 = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Third VIP Spawn Message").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipSpawnMsg3 = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"VIP Spawn Yell").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipSpawnYell = strValue; }
            }
            else if (Regex.Match(strVariable, @"Send First Msg to all Players?").Success)
            {
                this.SettingVipSpawnMsgSay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Send Second Msg to all Players?").Success)
            {
                this.SettingVipSpawnMsg2Say = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Send Third Msg to all Players?").Success)
            {
                this.SettingVipSpawnMsg3Say = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Send Yell to all Players?").Success)
            {
                this.SettingVipSpawnYellAll = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Send Message Delay").Success)
            {
                Int32 tmpMsgDel = 0;
                Int32.TryParse(strValue, out tmpMsgDel);
                if (tmpMsgDel >= 0 && tmpMsgDel < 21)
                {
                    this.SettingVipSpawnMsgDelay = tmpMsgDel;
                }
                else
                {
                    ConsoleError("Invalid value for Send Message Delay. It must be a number between 0 and 20. (e.g.: 0 for no delay)");
                }
            }
            else if (Regex.Match(strVariable, @"Show Private Message when VIP Slot expired?").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipExp = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue); }
            }
            else if (Regex.Match(strVariable, @"First Slot Expired Message").Success)
            {
                if (strValue.Length > 100)
                {
                    this.SettingVipExpMsg = "%player% your !VIP SLOT has expired";
                }
                else
                {
                    this.SettingVipExpMsg = strValue.Replace(System.Environment.NewLine, "");
                }
            }
            else if (Regex.Match(strVariable, @"Second Slot Expired Message").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipExpMsg2 = strValue.Replace(System.Environment.NewLine, ""); }
            }
            else if (Regex.Match(strVariable, @"Slot Expired Yell").Success)
            {
                if (strValue.Length <= 100) { this.SettingVipExpYell = strValue; }
            }
            else if (Regex.Match(strVariable, @"Temporary disable the").Success)
            {
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    ConsoleWrite("'Aggressive Join' server setting enabled. Close on round end it will be disabled to keep as many players as possible on the server. On next round it will be enabled automatically. This feature works for the following game modes: ConquestLarge, ConquestSmall, TDM and Chainlink.");
                }
                this.SettingAggressiveJoin = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"Public Say Message when NON-VIP got kicked for VIP").Success)
            {
                this.SettingAggressiveJoinKick = strValue;
            }
            else if (Regex.Match(strVariable, @"Private Message after NON-VIP got kicked and rejoins").Success)
            {
                if (strValue.Length <= 100) { this.SettingAggressiveJoinMsg = strValue; }
            }
            else if (Regex.Match(strVariable, @"Send Private Message as:").Success)
            {
                this.SettingAggressiveJoinMsgType = strValue;
            }
            else if (Regex.Match(strVariable, @"Enable Aggressive Join Abuse Protection").Success)
            {
                this.SettingAggressiveJoinKickAbuse = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    if ((layerReady) && (this.SettingSyncGs2Sql != "no  (remove from Gameserver)") && (this.SettingSyncGs2Sql != "no  (ignore)") && (this.SettingSyncGs2Sql != "yes  (30 days first Plugin installation only)"))
                    {
                        ConsoleWrite("ERROR: The setting 'Import NEW VIPS from Gameserver to SQL' must be set to 'NO (remove) to use 'Aggressive Join Abuse Protection'");
                        this.SettingAggressiveJoinKickAbuse = enumBoolYesNo.No;
                    }
                }
                else
                {
                    if (layerReady) { this.AggressiveJoinAbuseCleaner(); }
                }
            }
            else if (Regex.Match(strVariable, @"Each VIP can rejoin with an Aggressive Join Kick on full server").Success)
            {
                Int32 tmpMaxRejoin = 0;
                Int32.TryParse(strValue, out tmpMaxRejoin);
                if (tmpMaxRejoin >= 0 && tmpMaxRejoin < 10)
                {
                    this.SettingAggressiveJoinKickAbuseMax = tmpMaxRejoin;
                }
                else
                {
                    this.SettingAggressiveJoinKickAbuseMax = 3;
                    ConsoleError("Invalid value. It must be a number between 0 and 10. (e.g.: 3 - for max. 3 rejoins on full server)");
                }
            }
            else if (Regex.Match(strVariable, @"Private Message to target VIP if he triggered his max").Success)
            {
                if (strValue.Length <= 100) { this.SettingAggressiveJoinKickAbuseMsg = strValue; }
            }
            else if (Regex.Match(strVariable, @"Send Private Msg as:").Success)
            {
                this.SettingAggressiveJoinKickAbuseMsgType = strValue;
            }
            else if (Regex.Match(strVariable, @"Force Sync SQL and Gameserver NOW").Success)
            {
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    if (this.fIsEnabled)
                    {
                        this.ProconVipList();
                        if (this.firstCheck)
                        {
                            ConsoleWrite("[ForceSync] ^bForce Sync started...^n");
                            DebugWrite("[ForceSync] Receive VIP players from Gameserver and Sync with SQL", 3);
                            this.isForceSync = true;
                            this.ExecuteCommand("procon.protected.tasks.add", "VipSlotManager", "2", "1", "1", "procon.protected.plugins.call", "VipSlotManager", "SyncVipList");
                        }
                        else
                        {
                            ConsoleError("Force Sync canceled! Still loading VIPs from Gameserver");
                        }
                    }
                    else
                    {
                        ConsoleError("Force Sync canceled! Please enable the Plugin and try again...");
                    }
                }
                this.SettingForceSync = enumBoolYesNo.No;
            }
            else if (Regex.Match(strVariable, @"Print VIP list with time").Success)
            {
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    if (this.fIsEnabled)
                    {
                        if (this.firstCheck)
                        {
                            this.DisplayVips();
                        }
                        else
                        {
                            ConsoleError("Mini Manager canceled! Still loading VIPs from Gameserver");
                        }
                    }
                    else
                    {
                        ConsoleError("Mini Manager canceled! Please enable the Plugin and try again...");
                    }
                }
                this.SettingMiniManager = enumBoolYesNo.No;
            }
            else if (Regex.Match(strVariable, @"During for Yell and PYell in sec").Success)
            {
                Int32 tmpyelltime = 1;
                Int32.TryParse(strValue, out tmpyelltime);
                if (tmpyelltime >= 5 && tmpyelltime <= 60)
                {
                    this.SettingYellDuring = tmpyelltime;
                }
                else
                {
                    ConsoleError("Invalid value for Yell During. Time must be a number between 5 and 60. (e.g.: 15)");
                }
            }
            else if (Regex.Match(strVariable, @"Auto DB Cleaner").Success)
            {
                Int32 tmpDBCleaner = 1;
                Int32.TryParse(strValue, out tmpDBCleaner);
                if (tmpDBCleaner >= 1 && tmpDBCleaner <= 999)
                {
                    this.SettingDBCleaner = tmpDBCleaner;
                    this.DBCleanerCounter = (new Random().Next(960, 990));
                }
                else
                {
                    ConsoleError("Invalid value. It must be a number between 1 and 999. (e.g.: 90)");
                }
            }
            else if (Regex.Match(strVariable, @"Enable EA Guid Tracking and update playername changes").Success)
            {
                this.EAGuidTracking = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (layerReady)
                {
                    if (this.EAGuidTracking == enumBoolYesNo.Yes)
                    {
                        ConsoleWrite("EA GUID Tracking enabled.");
                        ConsoleWrite("EA GUID Tracking > ^bIMPORTANT INFO:^n If the Plugins runs on two or more " + this.SettingGameType + " Gameservers, then the Plugin setting 'Import NEW VIPs from GS to SQL'^n must be set to 'no (remove)^n' on ALL " + this.SettingGameType + " Gameservers to enable the 'EA GUID Tracking'.");
                        ConsoleWrite("EA GUID Tracking > INFO: If a valid VIP joins the server, then his playername will be linked to his EA GUID. If he joins again with a new/changed playername then his VIP Slot will be updated to the new playername for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database.");
                    }
                    else
                    {
                        this.Guid2Check.Clear();
                        this.GetPlayerGuid.Clear();
                    }
                    if ((this.EAGuidTracking == enumBoolYesNo.Yes) && ((this.SettingGameType != this.GetGameType.Replace("BFHL", "BFH").Replace("BFBC", "BC")) && (this.GetGameType != String.Empty) && (this.SettingGameType != "AUTOMATIC"))) { ConsoleWrite("WARNING: YOU selected " + this.strBlack(this.SettingGameType) + " as Gametype but Plugin detects " + this.strBlack(this.GetGameType) + ". EA GUID Tracking can NOT work correctly because the same player can have a diffrent EA GUID on a diffrent BF Version."); }
                }
            }
            else if (Regex.Match(strVariable, @"Enable Advanced Log to Adkats").Success)
            {
                this.SettingAdkatsLog = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"NON-VIP got kicked for VIP").Success)
            {
                this.SettingAdkatsLogNonVipKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"VIP changed his playername").Success)
            {
                this.SettingAdkatsLogVipChanged = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (Regex.Match(strVariable, @"VIP triggered Aggressive Join Abuse Protection").Success)
            {
                this.SettingAdkatsLogVipAggressiveJoinAbuse = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
        }
    }
}
