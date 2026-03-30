using System;
using System.Text.RegularExpressions;
using System.Threading;

using PRoCon.Core;

namespace PRoConEvents
{
    public partial class VipSlotManager
    {
        //////////////////////
        // In-Game Chat Event
        //////////////////////

        private void vsChatEvent(String tmpPlayername, String Msg)
        {
            if ((!this.fIsEnabled) || (!this.firstCheck) || (Msg.Length < 4)) { return; }
            Match regexResult = null;
            String tmp_pname = String.Empty;
            String tmp_pname_old = String.Empty;
            Boolean CRoseLead = false;

            //////////////////////
            // COMMANDS FROM OTHER PLUGIN
            //////////////////////
            if (tmpPlayername == "Server")
            {
                if (this.SettingPluginCmd == enumBoolYesNo.Yes)
                {
                    if (Regex.Match(Msg, @"^/vsm-addvip", RegexOptions.IgnoreCase).Success)
                    {
                        // remote command from other plugin via hidden admin say  ( e.g. /vsm-addvip <playername> <days> )
                        Match regexMatch = Regex.Match(Msg, @"/vsm-addvip\s+([^\s]+)\s+([^\s][0-9]*)$", RegexOptions.IgnoreCase);
                        if (regexMatch.Success)
                        {
                            regexResult = regexMatch;
                            if ((regexResult.Groups[1].Value.Length > 2) && (regexResult.Groups[2].Value.Length >= 1))
                            {
                                Int32 tmp_days = 0;
                                String tmp_strdays = "1";
                                Int32.TryParse(regexResult.Groups[2].Value, out tmp_days);
                                if ((tmp_days > 0) && (tmp_days < 5555))
                                {
                                    if (regexResult.Groups[2].Value.Contains("+"))
                                    {
                                        tmp_strdays = "+" + tmp_days.ToString();
                                    }
                                    else
                                    {
                                        tmp_strdays = tmp_days.ToString();
                                    }
                                    tmp_pname = regexResult.Groups[1].Value;
                                    Thread SQLWorker1 = new Thread(new ThreadStart(delegate ()
                                    {
                                        if (this.addvip(tmp_pname, tmp_strdays, "PluginCmd"))
                                        {
                                            DebugWrite("[OnChat] [PluginCmd] ^bAdd^n VIP Slot: " + this.strGreen(tmp_pname) + " for " + tmp_strdays + " days  (by command from other plugin)", 2);
                                            if (this.fDebugLevel >= 2) { this.ProconChat("PluginCmd > Add VIP Slot: " + this.strGreen(tmp_pname) + " for " + tmp_strdays + " days"); }
                                        }
                                    }));
                                    SQLWorker1.IsBackground = true;
                                    SQLWorker1.Name = "sqlworker1";
                                    SQLWorker1.Start();
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [PluginCmd] ^bERROR^n in /vsm-addvip syntax. Command is NOT valid: " + Msg, 2);
                            }
                        }
                        else
                        {
                            DebugWrite("[OnChat] [PluginCmd] ^bERROR^n in /vsm-addvip syntax. Command is NOT valid: " + Msg, 2);
                        }
                    }
                    else if (Regex.Match(Msg, @"^/vsm-removevip", RegexOptions.IgnoreCase).Success)
                    {
                        // remote command from other plugin via hidden admin say  ( e.g. /vsm-removevip <playername> )
                        Match regexMatch = Regex.Match(Msg, @"/vsm-removevip\s+([^\s]+)$", RegexOptions.IgnoreCase);
                        if (regexMatch.Success)
                        {
                            regexResult = regexMatch;
                            if (regexResult.Groups[1].Value.Length > 2)
                            {
                                DebugWrite("[OnChat] [PluginCmd] ^bRemove^n VIP Slot: " + this.strGreen(regexResult.Groups[1].Value.Replace(" ", "")) + "  (by command from other plugin)", 4);
                                tmp_pname = regexResult.Groups[1].Value;
                                Thread SQLWorker2 = new Thread(new ThreadStart(delegate ()
                                {
                                    if (this.removevip(tmp_pname))
                                    {
                                        DebugWrite("[OnChat] [PluginCmd] ^bRemove^n VIP Slot: " + this.strGreen(tmp_pname) + "  (by command from other plugin)", 2);
                                        if (this.fDebugLevel >= 2) { this.ProconChat("PluginCmd > Remove VIP Slot: " + this.strGreen(tmp_pname)); }
                                    }
                                }));
                                SQLWorker2.IsBackground = true;
                                SQLWorker2.Name = "sqlworker2";
                                SQLWorker2.Start();
                            }
                        }
                    }
                    else if (Regex.Match(Msg, @"^/vsm-changevip", RegexOptions.IgnoreCase).Success)
                    {
                        // remote command from other plugin via hidden admin say  ( e.g. /vsm-changevip <old playername> <new playername> )
                        Match regexMatch = Regex.Match(Msg, @"/vsm-changevip\s+([^\s]+)\s+([^\s]*)$", RegexOptions.IgnoreCase);
                        if (regexMatch.Success)
                        {
                            regexResult = regexMatch;
                            DebugWrite("[OnChat] [PluginCmd] Command from other Plugin (/vsm-changevip)", 4);
                            if ((regexResult.Groups[1].Value.Length > 2) && (regexResult.Groups[2].Value.Length > 2))
                            {
                                tmp_pname_old = regexResult.Groups[1].Value;
                                tmp_pname = regexResult.Groups[2].Value;
                                Thread SQLWorker6 = new Thread(new ThreadStart(delegate ()
                                {
                                    if (this.changevipname(tmp_pname_old, tmp_pname))
                                    {
                                        DebugWrite("[OnChat] [PluginCmd] Change VIP Slot playername from " + this.strGreen(tmp_pname_old) + " to " + this.strGreen(tmp_pname) + " for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database  (by command from other plugin)", 2);
                                        if (this.fDebugLevel >= 2) { this.ProconChat("PluginCmd > Change VIP Slot playername from " + this.strGreen(tmp_pname_old) + " to " + this.strGreen(tmp_pname) + " for all " + this.strBlack(this.SettingGameType) + " Server Groups"); }
                                    }
                                }));
                                SQLWorker6.IsBackground = true;
                                SQLWorker6.Name = "sqlworker6";
                                SQLWorker6.Start();
                            }
                        }
                    }
                    else if (Regex.Match(Msg, @"^/vsm-addsemivip", RegexOptions.IgnoreCase).Success)
                    {
                        // remote command from other plugin via hidden admin say  ( e.g. /vsm-addsemivip <playername> )
                        Match regexMatch = Regex.Match(Msg, @"/vsm-addsemivip\s+([^\s]+)$", RegexOptions.IgnoreCase);
                        if (regexMatch.Success)
                        {
                            regexResult = regexMatch;
                            if (regexResult.Groups[1].Value.Length > 2)
                            {
                                if ((this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes) && (this.SettingSyncGs2Sql == "no  (remove from Gameserver)"))
                                {
                                    DebugWrite("[OnChat] [PluginCmd] ^Add^n Semi VIP: " + this.strBlack(regexResult.Groups[1].Value.Replace(" ", "")) + " (valid for current round / rejoin) - (command from other plugin)", 3);
                                    if (this.fDebugLevel >= 2) { this.ProconChat("PluginCmd > Add Semi VIP Slot: " + this.strBlack(regexResult.Groups[1].Value.Replace(" ", "")) + " (valid for current round / rejoin)"); }
                                    this.AddRoundSemiVip(regexResult.Groups[1].Value.Replace(" ", ""));
                                }
                                else
                                {
                                    DebugWrite("[OnChat] [PluginCmd] ^bERROR^n Semi VIPs are disabled by Plugin settings", 3);
                                }
                            }
                        }
                    }
                }
                return;
            }

            //////////////////////
            // Notify - Chat Request  (!vip, !slot, ...)
            //////////////////////
            if (this.SettingChatReq == enumBoolYesNo.Yes)
            {
                if (Regex.Match(Msg, this.SettingInfoCmdRegMatch, RegexOptions.IgnoreCase).Success)
                {
                    DebugWrite("[OnChat] !VIP Slot Infos requested by ^b" + tmpPlayername + "^n", 3);
                    if (this.SettingInfoSay == enumBoolYesNo.Yes)
                    {
                        this.SayMsg(this.SettingInfoVip1.Replace("%player%", tmpPlayername).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()));
                        this.SayMsg(this.SettingInfoVip2.Replace("%player%", tmpPlayername).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()));
                    }
                    else
                    {
                        this.PlayerSayMsg(tmpPlayername, this.SettingInfoVip1.Replace("%player%", tmpPlayername).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()));
                        this.PlayerSayMsg(tmpPlayername, this.SettingInfoVip2.Replace("%player%", tmpPlayername).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()));
                    }
                    if (this.SqlVipsActive.ContainsKey(tmpPlayername))
                    {
                        this.PlayerSayMsg(tmpPlayername, this.SettingInfoVip3.Replace("%player%", tmpPlayername).Replace("%time%", this.strGetRemTime(this.GetVipTimestamp(tmpPlayername))).Replace("%online%", this.vipsCurrentlyOnline.ToString()).Replace("%total%", this.SqlVipsActive.Count.ToString()));
                    }
                    else if ((this.EAGuidTracking == enumBoolYesNo.Yes) && (!this.GetPlayerGuid.Contains(tmpPlayername)))
                    {
                        this.GetPlayerGuid.Add(tmpPlayername);
                    }
                    return;
                }
            }

            //////////////////////
            // In-Game VIP Commands  (!lead, !killme, !switchme)
            //////////////////////
            if (this.SettingVipCmd == enumBoolYesNo.Yes)
            {
                if ((this.SettingLeadByCRose == enumBoolYesNo.Yes) && (Msg == "ID_CHAT_REQUEST_ORDER")) { CRoseLead = true; }
                if ((Regex.Match(Msg, @"^/?[!|@|/]lead$", RegexOptions.IgnoreCase).Success) || (CRoseLead))
                {
                    if (this.SettingLeadCmd == enumBoolYesNo.Yes)
                    {
                        if ((this.SqlVipsActive.ContainsKey(tmpPlayername)) || (this.RoundTempVips.Contains(tmpPlayername)))
                        {
                            if ((this.playerTeamID[tmpPlayername] > 0) && (this.playerSquadID[tmpPlayername] > 0))
                            {
                                if ((this.SettingIgnoreVipLeader == enumBoolYesNo.No) && (this.SquadLederList.ContainsKey(this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString())))
                                {
                                    if (this.SqlVipsActive.ContainsKey(this.SquadLederList[this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString()]))
                                    {
                                        // other vip is leader
                                        if (!CRoseLead)
                                        {
                                            DebugWrite("[OnChat] [IngameVipCmd] Igonore In-Game VIP Command !lead by ^b" + tmpPlayername + "^n. (Other VIP " + this.SquadLederList[this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString()] + " is currently leader)", 4);
                                            this.PlayerSayMsg(tmpPlayername, "Sorry, you can NOT take the lead from other !VIP");
                                        }
                                    }
                                    else
                                    {
                                        DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !lead by ^b" + tmpPlayername + "^n (TeamID: " + this.playerTeamID[tmpPlayername].ToString() + " SquadID: " + this.playerSquadID[tmpPlayername].ToString() + ")", 4);
                                        this.PlayerSayMsg(tmpPlayername, "You are the leader now");
                                        this.ExecuteCommand("procon.protected.send", "squad.leader", this.playerTeamID[tmpPlayername].ToString(), this.playerSquadID[tmpPlayername].ToString(), tmpPlayername);
                                    }
                                }
                                else
                                {
                                    DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !lead by ^b" + tmpPlayername + "^n (TeamID: " + this.playerTeamID[tmpPlayername].ToString() + " SquadID: " + this.playerSquadID[tmpPlayername].ToString() + ")", 4);
                                    this.PlayerSayMsg(tmpPlayername, "You are the leader now");
                                    this.ExecuteCommand("procon.protected.send", "squad.leader", this.playerTeamID[tmpPlayername].ToString(), this.playerSquadID[tmpPlayername].ToString(), tmpPlayername);
                                }
                            }
                            else
                            {
                                // enforcing lead
                                this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                                Int32 tmp_enforcingcounter = 0;
                                Thread ThreadWorker3 = new Thread(new ThreadStart(delegate ()
                                {
                                    while ((this.playerTeamID[tmpPlayername] > 0) && (this.playerSquadID[tmpPlayername] > 0))
                                    {
                                        Thread.Sleep(1500);
                                        tmp_enforcingcounter++;
                                        if (tmp_enforcingcounter == 2) { this.PlayerSayMsg(tmpPlayername, "Enforcing lead..."); }
                                        if (tmp_enforcingcounter > 5)
                                        {
                                            if (!CRoseLead)
                                            {
                                                DebugWrite("[OnChat] [IngameVipCmd] [LeadEnforcer] ERROR: Can not execute In-Game VIP Command !lead by ^b" + tmpPlayername + "^n (TeamID: " + this.playerTeamID[tmpPlayername].ToString() + " SquadID: " + this.playerSquadID[tmpPlayername].ToString() + ")", 4);
                                                this.PlayerSayMsg(tmpPlayername, tmpPlayername + " please use the command !lead in few seconds again.");
                                            }
                                            return;
                                        }
                                    }
                                    // ready for check again
                                    if ((this.playerTeamID[tmpPlayername] > 0) && (this.playerSquadID[tmpPlayername] > 0))
                                    {
                                        if ((this.SettingIgnoreVipLeader == enumBoolYesNo.No) && (this.SquadLederList.ContainsKey(this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString())))
                                        {
                                            if (this.SqlVipsActive.ContainsKey(this.SquadLederList[this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString()]))
                                            {
                                                // other vip is leader
                                                if (!CRoseLead)
                                                {
                                                    DebugWrite("[OnChat] [IngameVipCmd] Igonore In-Game VIP Command !lead by ^b" + tmpPlayername + "^n. (Other VIP " + this.SquadLederList[this.playerTeamID[tmpPlayername].ToString() + this.playerSquadID[tmpPlayername].ToString()] + " is currently leader)", 4);
                                                    this.PlayerSayMsg(tmpPlayername, "Sorry, you can NOT take the lead from other !VIP");
                                                }
                                            }
                                            else
                                            {
                                                DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !lead by ^b" + tmpPlayername + "^n (TeamID: " + this.playerTeamID[tmpPlayername].ToString() + " SquadID: " + this.playerSquadID[tmpPlayername].ToString() + ")", 4);
                                                this.PlayerSayMsg(tmpPlayername, "You are the leader now");
                                                this.ExecuteCommand("procon.protected.send", "squad.leader", this.playerTeamID[tmpPlayername].ToString(), this.playerSquadID[tmpPlayername].ToString(), tmpPlayername);
                                            }
                                        }
                                        else
                                        {
                                            DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !lead by ^b" + tmpPlayername + "^n (TeamID: " + this.playerTeamID[tmpPlayername].ToString() + " SquadID: " + this.playerSquadID[tmpPlayername].ToString() + ")", 4);
                                            this.PlayerSayMsg(tmpPlayername, "You are the leader now");
                                            this.ExecuteCommand("procon.protected.send", "squad.leader", this.playerTeamID[tmpPlayername].ToString(), this.playerSquadID[tmpPlayername].ToString(), tmpPlayername);
                                        }
                                    }
                                }));
                                ThreadWorker3.IsBackground = true;
                                ThreadWorker3.Name = "threadworker3";
                                ThreadWorker3.Start();
                            }
                        }
                        else if (!CRoseLead)
                        {
                            DebugWrite("[OnChat] [IngameVipCmd] ^b" + tmpPlayername + "^n have NO VIP privileges to use the command '!lead'", 4);
                            this.PlayerSayMsg(tmpPlayername, "Sorry, this command is for !VIP SLOT players only");
                        }
                    }
                }
                else if (Regex.Match(Msg, @"^/?[!|@|/]killme$", RegexOptions.IgnoreCase).Success)
                {
                    if (this.SettingKillmeCmd == enumBoolYesNo.Yes)
                    {
                        if ((this.SqlVipsActive.ContainsKey(tmpPlayername)) || (this.RoundTempVips.Contains(tmpPlayername)))
                        {
                            DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !killme by ^b" + tmpPlayername + "^n", 4);
                            this.ExecuteCommand("procon.protected.send", "admin.killPlayer", tmpPlayername);
                        }
                        else
                        {
                            DebugWrite("[OnChat] [IngameVipCmd] ^b" + tmpPlayername + "^n have NO VIP privileges to use the command '!killme'", 4);
                            this.PlayerSayMsg(tmpPlayername, "Sorry, this command is for !VIP SLOT players only");
                        }
                    }
                }
                else if (Regex.Match(Msg, @"^/?[!|@|/]switchme$", RegexOptions.IgnoreCase).Success)
                {
                    if (this.SettingSwitchmeCmd == enumBoolYesNo.Yes)
                    {
                        if ((this.SqlVipsActive.ContainsKey(tmpPlayername)) || (this.RoundTempVips.Contains(tmpPlayername)))
                        {
                            DebugWrite("[OnChat] [IngameVipCmd] In-Game VIP Command !switchme by ^b" + tmpPlayername + "^n", 4);
                            if (this.playerTeamID[tmpPlayername] > 0)
                            {
                                Int32 tmp_newteam = 1;
                                if (this.playerTeamID[tmpPlayername] == 1) { tmp_newteam = 2; }
                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", tmpPlayername, tmp_newteam.ToString(), "0", "true");
                            }
                        }
                        else
                        {
                            DebugWrite("[OnChat] [IngameVipCmd] ^b" + tmpPlayername + "^n have NO VIP privileges to use the command '!switchme'", 4);
                            this.PlayerSayMsg(tmpPlayername, "Sorry, this command is for !VIP SLOT players only");
                        }
                    }
                }
            }

            //////////////////////
            // COMMANDS FROM ADMINS
            //////////////////////
            if (this.SettingAdminCmd == enumBoolYesNo.Yes)
            {
                if (Regex.Match(Msg, @"^/?[!|/|@]addvip|^/?[!|/|@]removevip|^/?[!|/|@]checkvip|^/?[!|/|@]changevip|^/?[!|/|@]addsemivip", RegexOptions.IgnoreCase).Success)
                {
                    Msg = Msg.Replace("\"", "").Replace(";", "").Replace(",", "").Replace("(", "").Replace(")", "").Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace("'", "").Replace("\u2018", "").Replace("\u2019", "");
                    Thread SQLWorker3 = new Thread(new ThreadStart(delegate ()
                    {
                        if (Regex.Match(Msg, @"^/?[!|/|@]addvip", RegexOptions.IgnoreCase).Success)
                        {
                            if (this.isAdmin(tmpPlayername))
                            {
                                DebugWrite("[OnChat] [IngameAdmin] Admin In-Game Command (!addvip) from " + tmpPlayername, 4);
                                // command from In-Game chat  ( e.g. !addvip <playername> <days>)
                                Boolean AddOk = false;
                                Match regexMatch = Regex.Match(Msg, @"^/?[!|/|@]addvip\s+([^\s]+)\s+([^\s][0-9]*)$", RegexOptions.IgnoreCase);
                                if (regexMatch.Success)
                                {
                                    regexResult = regexMatch;
                                    if ((regexResult.Groups[1].Value.Length > 2) && (regexResult.Groups[2].Value.Length >= 1))
                                    {
                                        Int32 tmpx_days = 0;
                                        String tmpx_strdays = "1";
                                        Int32.TryParse(regexResult.Groups[2].Value, out tmpx_days);
                                        if ((tmpx_days > 0) && (tmpx_days < 5555))
                                        {
                                            if (regexResult.Groups[2].Value.Contains("+"))
                                            {
                                                tmpx_strdays = "+" + tmpx_days.ToString();
                                            }
                                            else
                                            {
                                                tmpx_strdays = tmpx_days.ToString();
                                            }
                                            if (this.addvip(regexResult.Groups[1].Value.Replace(" ", ""), tmpx_strdays, tmpPlayername))
                                            {
                                                DebugWrite("[OnChat] [IngameAdmin] ^bAdd^n VIP Slot: " + this.strGreen(regexResult.Groups[1].Value.Replace(" ", "")) + " for " + tmpx_strdays + " days  (ingame admin " + tmpPlayername + ")", 2);
                                                this.PlayerSayMsg(tmpPlayername, "!VIP SLOT added: " + regexResult.Groups[1].Value.Replace(" ", ""));
                                                AddOk = true;
                                            }
                                        }
                                    }
                                }
                                if (!AddOk)
                                {
                                    DebugWrite("[OnChat] [IngameAdmin] ^bERROR in !addvip syntax from " + tmpPlayername + ". Command is NOT valid: " + Msg + "^n", 4);
                                    this.PlayerSayMsg(tmpPlayername, "ERROR: Player NOT added as !VIP. Check your syntax");
                                    this.PlayerSayMsg(tmpPlayername, "TYPE: !addvip <full playername> <days>  (e.g. !addvip SniperBen +30)");
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [IngameAdmin] ^b" + tmpPlayername + "^n have NO admin privileges to use the command '!addvip'.  (Requires: Can Edit Reserved Slots List)", 3);
                                this.PlayerSayMsg(tmpPlayername, "ERROR: You do NOT have admin privileges");
                            }
                        }
                        else if (Regex.Match(Msg, @"^/?[!|/|@]removevip", RegexOptions.IgnoreCase).Success)
                        {
                            if (this.isAdmin(tmpPlayername))
                            {
                                // command from In-Game chat  ( e.g. !removevip <playername> )
                                Boolean AddOk = false;
                                Match regexMatch = Regex.Match(Msg, @"^/?[!|/|@]removevip\s+([^\s]+)$", RegexOptions.IgnoreCase);
                                if (regexMatch.Success)
                                {
                                    regexResult = regexMatch;
                                    if (regexResult.Groups[1].Value.Length > 2)
                                    {
                                        DebugWrite("[OnChat] [IngameAdmin] Admin In-Game Command (!removevip) from " + tmpPlayername, 4);
                                        if (this.removevip(regexResult.Groups[1].Value.Replace(" ", "")))
                                        {
                                            DebugWrite("[OnChat] [IngameAdmin] ^bRemove^n VIP Slot: " + this.strGreen(regexResult.Groups[1].Value.Replace(" ", "")) + " (ingame admin " + tmpPlayername + ")", 2);
                                            this.PlayerSayMsg(tmpPlayername, "!VIP SLOT removed: " + regexResult.Groups[1].Value.Replace(" ", ""));
                                            AddOk = true;
                                        }
                                    }
                                }
                                if (!AddOk)
                                {
                                    DebugWrite("[OnChat] [IngameAdmin] ^bERROR in !remove syntax from " + tmpPlayername + ". Command is NOT valid: " + Msg + "^n", 4);
                                    this.PlayerSayMsg(tmpPlayername, "ERROR: Player NOT removed! Check your syntax");
                                    this.PlayerSayMsg(tmpPlayername, "TYPE: !removevip <full playername>  (e.g. !removevip SniperBen)");
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [IngameAdmin] ^b" + tmpPlayername + "^n have NO admin privileges to use the command '!removevip'.  (Requires: Can Edit Reserved Slots List)", 3);
                                this.PlayerSayMsg(tmpPlayername, "ERROR: You do NOT have admin privileges");
                            }
                        }
                        else if (Regex.Match(Msg, @"^/?[!|/|@]checkvip", RegexOptions.IgnoreCase).Success)
                        {
                            if (this.isAdmin(tmpPlayername))
                            {
                                // command from In-Game chat  ( e.g. !checkevip <playername> )
                                Match regexMatch = Regex.Match(Msg, @"^/?[!|/|@]checkvip\s+([^\s]+)$", RegexOptions.IgnoreCase);
                                if (regexMatch.Success)
                                {
                                    DebugWrite("[OnChat] [IngameAdmin] Admin In-Game Command (!checkvip) from " + tmpPlayername, 4);
                                    regexResult = regexMatch;
                                    Int32 tmp_playerTimestamp = this.checkvip(regexResult.Groups[1].Value);
                                    if (tmp_playerTimestamp != -1)
                                    {
                                        this.PlayerSayMsg(tmpPlayername, regexResult.Groups[1].Value + " valid !VIP SLOT: " + this.strGetRemTime(tmp_playerTimestamp));
                                        if (this.RoundTempVips.Contains(regexResult.Groups[1].Value)) { this.PlayerSayMsg(tmpPlayername, regexResult.Groups[1].Value + " without 'Aggressive Join Kick' privilege till next round."); }
                                    }
                                    else
                                    {
                                        if (this.RoundTempVips.Contains(regexResult.Groups[1].Value))
                                        {
                                            this.PlayerSayMsg(tmpPlayername, regexResult.Groups[1].Value + " Semi !VIP SLOT: till round end / rejoin.");
                                        }
                                        else
                                        {
                                            this.PlayerSayMsg(tmpPlayername, regexResult.Groups[1].Value + " is NOT a !VIP");
                                            this.PlayerSayMsg(tmpPlayername, "Requires: <full playername> with case sensitive");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [IngameAdmin] ^b" + tmpPlayername + "^n have NO admin privileges to use the command '!removevip'.  (Requires: Can Edit Reserved Slots List)", 3);
                                this.PlayerSayMsg(tmpPlayername, "ERROR: You do NOT have admin privileges");
                            }
                        }
                        else if (Regex.Match(Msg, @"^/?[!|/|@]changevip", RegexOptions.IgnoreCase).Success)
                        {
                            if (this.isAdmin(tmpPlayername))
                            {
                                DebugWrite("[OnChat] [IngameAdmin] Admin In-Game Command (!changevip) from " + tmpPlayername, 4);
                                // command from In-Game chat  ( e.g. !changevip <old playername> <new playername> )
                                Boolean changeOk = false;
                                Match regexMatch = Regex.Match(Msg, @"^/?[!|/|@]changevip\s+([^\s]+)\s+([^\s]*)$", RegexOptions.IgnoreCase);
                                if (regexMatch.Success)
                                {
                                    regexResult = regexMatch;
                                    if ((regexResult.Groups[1].Value.Length > 2) && (regexResult.Groups[2].Value.Length > 2))
                                    {
                                        tmp_pname_old = regexResult.Groups[1].Value;
                                        tmp_pname = regexResult.Groups[2].Value;
                                        if (this.changevipname(tmp_pname_old, tmp_pname))
                                        {
                                            DebugWrite("[OnChat] [IngameAdmin] Change VIP Slot playername from " + this.strGreen(tmp_pname_old) + " to " + this.strGreen(tmp_pname) + " for all " + this.SettingGameType + " Server Groups in SQL database  (ingame admin " + tmpPlayername + ")", 2);
                                            this.PlayerSayMsg(tmpPlayername, "VIP changed from " + tmp_pname_old + " to " + tmp_pname + " for all " + this.SettingGameType + " Server Groups");
                                            changeOk = true;
                                        }
                                    }
                                }
                                if (!changeOk)
                                {
                                    DebugWrite("[OnChat] [IngameAdmin] ^bERROR in !changevip syntax from " + tmpPlayername + ". Command is NOT valid: " + Msg + "^n", 4);
                                    this.PlayerSayMsg(tmpPlayername, "ERROR: Playername NOT changed. Check your syntax");
                                    this.PlayerSayMsg(tmpPlayername, "TYPE: !changevip <old playername> <new playername>  (e.g. !changevip SniperBen SniperBenni)");
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [IngameAdmin] ^b" + tmpPlayername + "^n have NO admin privileges to use the command '!changevip'.  (Requires: Can Edit Reserved Slots List)", 3);
                                this.PlayerSayMsg(tmpPlayername, "ERROR: You do NOT have admin privileges");
                            }
                        }
                        else if (Regex.Match(Msg, @"^/vsm-addsemivip", RegexOptions.IgnoreCase).Success)
                        {
                            // remote command from other plugin via hidden admin say  ( e.g. /vsm-addsemivip <playername> )
                            Match regexMatch = Regex.Match(Msg, @"/vsm-addsemivip\s+([^\s]+)$", RegexOptions.IgnoreCase);
                            if (this.isAdmin(tmpPlayername))
                            {
                                if (regexMatch.Success)
                                {
                                    regexResult = regexMatch;
                                    if (regexResult.Groups[1].Value.Length > 2)
                                    {
                                        if ((this.SettingAggressiveJoinKickAbuse == enumBoolYesNo.Yes) && (this.SettingSyncGs2Sql == "no  (remove from Gameserver)"))
                                        {
                                            DebugWrite("[OnChat] [IngameAdmin] ^Add^n Semi VIP: " + this.strBlack(regexResult.Groups[1].Value.Replace(" ", "")) + ". Valid for current round / rejoin.  (ingame admin " + tmpPlayername + ")", 2);
                                            this.PlayerSayMsg(tmpPlayername, "Semi VIP: " + regexResult.Groups[1].Value.Replace(" ", "") + " added (valid for current round / rejoin).");
                                            this.AddRoundSemiVip(regexResult.Groups[1].Value.Replace(" ", ""));
                                        }
                                        else
                                        {
                                            this.PlayerSayMsg(tmpPlayername, "ERROR: Semi VIPs are disabled by Plugin settings.");
                                        }
                                    }
                                }
                                else
                                {
                                    this.PlayerSayMsg(tmpPlayername, "ERROR: Check your syntax. Type: !addsemivip <playername>");
                                }
                            }
                            else
                            {
                                DebugWrite("[OnChat] [IngameAdmin] ^b" + tmpPlayername + "^n have NO admin privileges to use the command '!addsemivip'.  (Requires: Can Edit Reserved Slots List)", 3);
                                this.PlayerSayMsg(tmpPlayername, "ERROR: You do NOT have admin privileges");
                            }
                        }
                    }));
                    SQLWorker3.IsBackground = true;
                    SQLWorker3.Name = "sqlworker3";
                    SQLWorker3.Start();
                }
            }
        }
    }
}
