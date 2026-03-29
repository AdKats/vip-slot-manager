using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using Dapper;

using MySqlConnector;

namespace PRoConEvents
{
    public partial class VipSlotManager
    {
        //////////////////////
        // SQL Functions
        // CChatGUIDStatsLogger part by [GWC]XpKiller
        //////////////////////

        private void TableBuilder()
        {
            Boolean TableExist = false;
            Boolean ColumnGuidExist = false;
            Boolean TableCreated = false;
            Boolean TableUpdated = false;

            if (!this.SqlTableExist)
            {
                if (this.SqlLoginsOk())
                {
                    DebugWrite("[SQL-TableBuilder] Connecting to SQL and check database", 4);
                    Thread SQLWorker4 = new Thread(new ThreadStart(delegate ()
                    {
                        try
                        {
                            using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                            {
                                Con.Open();
                                try
                                {
                                    // check if table exist in SQL database
                                    String SQL = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='vsm_vips' AND table_schema='" + this.SettingStrSqlDatabase + "'";
                                    DebugWrite("[SQL-TableBuilder] [CheckExist] Connected to SQL. Check if table exist or not in SQL database. SQL COMMAND (MyCommand): " + SQL, 5);
                                    using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                                    {
                                        DataTable resultTable = this.SQLquery(MyCommand);
                                        if (resultTable.Rows != null)
                                        {
                                            DebugWrite("[SQL-TableBuilder] [CheckExist] Receive informations from SQL", 5);
                                            foreach (DataRow row in resultTable.Rows)
                                            {
                                                // reading sql
                                                if (row["COLUMN_NAME"].ToString() == "servergroup")
                                                {
                                                    // yes, table 'vsm_vips' exist in SQL DB!!
                                                    DebugWrite("[SQL-TableBuilder] [CheckExist] Table 'vsm_vips' exist in SQL database", 5);
                                                    TableExist = true;
                                                }
                                                else if (row["COLUMN_NAME"].ToString() == "guid")
                                                {
                                                    DebugWrite("[SQL-TableBuilder] [CheckExist] Column 'guid' exist in table 'vsm_vips' in SQL database", 5);
                                                    ColumnGuidExist = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ConsoleError("[SQL-TableBuilder] [CheckExist] Table 'vsm_vips' NOT exist on your SQL Server");
                                        }
                                    }
                                }
                                catch (Exception c)
                                {
                                    ConsoleError("[SQL-TableBuilder] [CheckExist] SQL Error (MyCommand): " + c);
                                    TableExist = false;
                                }

                                // create NEW table in SQL if not exist (first plugin start after installation)
                                if ((!TableExist) || (!ColumnGuidExist))
                                {
                                    ////////////////////////////////
                                    // start table bulider
                                    ////////////////////////////////
                                    try
                                    {
                                        String SqlTableBuild = String.Empty;
                                        if (!TableExist)
                                        {
                                            SqlTableBuild = "CREATE TABLE IF NOT EXISTS `vsm_tBrowserSessions` (id INT NOT NULL auto_increment,sessionID VARCHAR(250) NOT NULL,time INT NOT NULL,lockedUntil INT NOT NULL DEFAULT 0,error VARCHAR(300),userID INT,tSessionID INT,PRIMARY KEY (id))";
                                            ConsoleWrite("[SQL-TableBuilder] [CreateTable] Plugin create NEW table 'vsm_tBrowserSessions' SQL database");
                                            DebugWrite("^b[SQL-TableBuilder] [CreateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                            }
                                            SqlTableBuild = "CREATE TABLE IF NOT EXISTS `vsm_tUser` (id int NOT NULL auto_increment,sessionID varchar(250),email varchar(100),password varchar(40),passwordDummy varchar(20),salt VARCHAR(5),rights INT(0),PRIMARY KEY (id))";
                                            ConsoleWrite("[SQL-TableBuilder] [CreateTable] Plugin create NEW table 'vsm_tUser' in SQL database");
                                            DebugWrite("^b[SQL-TableBuilder] [CreateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                            }
                                            SqlTableBuild = "CREATE TABLE IF NOT EXISTS `vsm_tFilter` (id int NOT NULL auto_increment,userID INT,server varchar(10),gruppe varchar(10),PRIMARY KEY (id))";
                                            ConsoleWrite("[SQL-TableBuilder] [CreateTable] Plugin create NEW table 'vsm_tFilter' in SQL database");
                                            DebugWrite("^b[SQL-TableBuilder] [CreateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                            }
                                            SqlTableBuild = "INSERT INTO vsm_tUser (email, password, salt, rights) SELECT * FROM (SELECT 'admin', '8a2c156a7d5c76b1f9e4c75353627a3a', '28g7d', 0) AS tmp WHERE NOT EXISTS ( SELECT email FROM vsm_tUser ) LIMIT 1";
                                            ConsoleWrite("[SQL-TableBuilder] [CreateWebAdmin] ^bLOGIN FOR FOR WEBSITE: user: admin , pw: admin^n");
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                            }
                                            SqlTableBuild = "CREATE TABLE IF NOT EXISTS `vsm_vips` (`ID` INT NOT NULL AUTO_INCREMENT ,`gametype` varchar(3) NOT NULL,`servergroup` varchar(2) NOT NULL,`playername` varchar(35) NULL DEFAULT NULL ,`timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,`status` varchar(8) NOT NULL,`admin` varchar(35) NULL DEFAULT NULL ,`comment` text NULL DEFAULT NULL, `guid` varchar(35) NULL DEFAULT NULL ,PRIMARY KEY (`ID`),UNIQUE KEY `servergroup` (`servergroup`,`playername`,`gametype`))ENGINE = InnoDB";
                                            ConsoleWrite("[SQL-TableBuilder] [CreateTable] Plugin create NEW table 'vsm_vips' in SQL database");
                                            DebugWrite("^b[SQL-TableBuilder] [CreateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                                MyCom.Connection.Close();
                                                TableCreated = true;
                                            }
                                        }
                                        else if (!ColumnGuidExist)
                                        {
                                            // update sql, add column 'guid' to table 'vsm_vips'
                                            SqlTableBuild = "ALTER TABLE `vsm_vips` ADD `guid` varchar(35) NULL DEFAULT NULL";
                                            ConsoleWrite("[SQL-TableBuilder] [UpdateTable] Add new column into table 'vsm_vips' in SQL database");
                                            DebugWrite("^b[SQL-TableBuilder] [UpdateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                            }
                                            SqlTableBuild = "ALTER TABLE `vsm_vips` CHANGE `timestamp` `timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
                                            using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con))
                                            {
                                                MyCom.ExecuteNonQuery();
                                                TableUpdated = true;
                                                MyCom.Connection.Close();
                                            }
                                        }
                                    }
                                    catch (MySqlException oe)
                                    {
                                        ConsoleError("[SQL-TableBuilder] [CreateTable] Error in Tablebuilder:");
                                        this.DisplayMySqlErrorCollection(oe);
                                        TableCreated = false;
                                    }
                                    catch (Exception c)
                                    {
                                        ConsoleError("[SQL-TableBuilder] [CreateTable] SQL Error (MyCom): " + c);
                                        TableCreated = false;
                                    }
                                    finally
                                    {
                                        DebugWrite("[[SQL-TableBuilder] Close SQL Connection (Con)", 5);
                                        Con.Close();
                                        if (TableCreated)
                                        {
                                            this.SqlTableExist = true;
                                            ConsoleWrite("[SQL-TableBuilder] ^b^2NEW table created successfully^0^n");
                                        }
                                        else if (TableUpdated)
                                        {
                                            this.SqlTableExist = true;
                                            ConsoleWrite("[SQL-TableBuilder] [UpdateTable] ^b^2NEW column in table 'vsm_vips' created successfully^0^n");
                                        }
                                    }
                                }
                                else
                                {
                                    if (Con.State == ConnectionState.Open)
                                    {
                                        DebugWrite("[[SQL-TableBuilder] Close SQL Connection (Con)", 5);
                                        Con.Close();
                                    }
                                    this.SqlTableExist = true;
                                }
                            }
                        }
                        catch (Exception c)
                        {
                            ConsoleError("[SQL-TableBuilder] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                        }
                    }));
                    SQLWorker4.IsBackground = true;
                    SQLWorker4.Name = "sqlworker4";
                    SQLWorker4.Start();
                }
            }
        }

        public void SyncVipList()
        {
            if (!this.fIsEnabled) { return; }
            Boolean SyncGameserver = false;
            Boolean SqlConOK = false;
            Boolean tmp_gotSql = false;
            Dictionary<String, Int32> tmp_sql_vips_active = new Dictionary<String, Int32>();
            Dictionary<String, String> tmp_sqlguid = new Dictionary<String, String>();
            Dictionary<String, String> tmp_GuidsActive = new Dictionary<String, String>();
            List<String> tmp_sql_vips2add = new List<String>();
            List<String> tmp_sql_vips2del = new List<String>();
            List<String> tmp_sql_vips2inactive = new List<String>();
            List<String> tmp_sql_vips_INVALID = new List<String>();
            List<String> tmp_gs_vips = this.vipsGS;
            Int32 tmp_intpTimestamp = 0;
            Int32 tmp_intServerTimestamp = intUtcTimestamp(); // utc time
            Int32 tmp_adders = 0;
            String SQL = String.Empty;
            String tmp_newStatus = String.Empty;
            String tmp_pName = String.Empty;
            String tmp_pStatus = String.Empty;
            String tmp_pGuid = String.Empty;
            String tmp_queryGuid = String.Empty;

            this.AdkatsRunning = GetRegisteredCommands().Any(command => command.RegisteredClassname == "AdKats" && command.RegisteredMethodName == "PluginEnabled");

            if (((DateTime.UtcNow - this.LastSync).TotalSeconds) < 10) { return; }
            DebugWrite("[SyncVipList] ^bSync started^n", 5);
            if ((!this.gotVipsGS) || (!this.firstCheck))
            {
                DebugWrite("[SyncVipList] ^bSync canceled!^n Still loading VIPs from Gameserver", 3);
                return;
            }
            this.LastSync = DateTime.UtcNow;

            Thread SQLWorker = new Thread(new ThreadStart(delegate ()
            {
                /////////////////////
                // Connect to SQL
                /////////////////////
                DebugWrite("[SyncVipList] [SqlConnection] Try to sync VIP players from SQL database and Gameserver", 5);
                this.TableBuilder();
                if (!this.SqlTableExist)
                {
                    ConsoleError("[SyncVipList] Sync canceled! (SQL Error)");
                    return;
                }

                if (this.SqlLoginsOk())
                {
                    try
                    {
                        using (MySqlConnection Connection = new MySqlConnection(this.SqlLogin()))
                        {
                            Connection.Open();
                            try
                            {
                                if (Connection.State == ConnectionState.Open)
                                {
                                    try
                                    {
                                        SqlConOK = true;
                                        if (this.EAGuidTracking == enumBoolYesNo.Yes) { tmp_queryGuid = "`guid`, "; }
                                        SQL = "SELECT `playername`, `status`, " + tmp_queryGuid + "TIMESTAMPDIFF(SECOND,'1970-01-01',timestamp) AS timestamp FROM `vsm_vips` WHERE gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "' AND status in ('active', 'adding', 'expired', 'deleting', 'removing') LIMIT 800";
                                        using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                                        {
                                            DataTable resultTable = this.SQLquery(MyCommand);
                                            if (resultTable.Rows != null)
                                            {
                                                DebugWrite("[SyncVipList] [SqlConnection] OK! Connected to SQL database. Read playerlist with [gametype]: " + this.SettingGameType + " and [servergroup]: " + this.SettingStrSqlServerGroup + " and [status]: active, adding, expired, deleting, removing). SQL COMMAND (MyCommand): " + SQL, 5);
                                                if (resultTable.Rows.Count >= 750) { DebugWrite("^b^8WARNING^0^n^8 This Gameserver (Server Group) have more than 700 VIPs (status = 'active / expired')! ^b^8LIMIT:^n^8 Maximal 800 VIPs for each Server Group!  IMPORTANT: YOU have to change the setting 'Auto Database Cleaner' or remove some VIPs with current status 'active' or 'expired' for this Server Group manually (go to the website and change the status from 'expired' to 'inactive' for some players).^0", 2); }
                                                this.vipsExpired.Clear();
                                                tmp_gotSql = true;
                                                foreach (DataRow row in resultTable.Rows)
                                                {
                                                    // reading sql, create tmp lists
                                                    tmp_pName = row["playername"].ToString();
                                                    tmp_pStatus = row["status"].ToString();
                                                    tmp_intpTimestamp = Convert.ToInt32(row["timestamp"]);
                                                    if (tmp_pStatus == "active")
                                                    {
                                                        tmp_sql_vips_active.Add(tmp_pName, tmp_intpTimestamp);
                                                    }
                                                    else if (tmp_pStatus == "deleting")
                                                    {
                                                        tmp_sql_vips2del.Add(tmp_pName);
                                                    }
                                                    else if (tmp_pStatus == "removing")
                                                    {
                                                        tmp_sql_vips2inactive.Add(tmp_pName);
                                                    }
                                                    else if (tmp_pStatus == "adding")
                                                    {
                                                        tmp_sql_vips_active.Add(tmp_pName, tmp_intpTimestamp);
                                                        tmp_sql_vips2add.Add(tmp_pName);
                                                    }
                                                    else if (tmp_pStatus == "expired")
                                                    {
                                                        this.vipsExpired.Add(tmp_pName);
                                                    }
                                                    if (this.EAGuidTracking == enumBoolYesNo.Yes)
                                                    {
                                                        if (row["guid"].ToString().StartsWith("EA_")) { tmp_sqlguid.Add(tmp_pName, row["guid"].ToString()); }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DebugWrite("[SyncVipList] [SqlConnection] ERROR: Can not read SQL informations", 3);
                                                this.SqlTableExist = false;
                                            }
                                            MyCommand.Connection.Close();
                                            DebugWrite("[SyncVipList] [SqlConnection] Close SQL Connection (MyCommand)", 5);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        ConsoleError("[SyncVipList] SQL Connection Error: " + e);
                                        tmp_gotSql = false;
                                    }

                                    /////////////////////
                                    // Check vip player from TMP-SQL list and Gamerserver list
                                    /////////////////////
                                    if (tmp_gotSql)
                                    {
                                        DebugWrite("[SyncVipList] [SqlListActive] Parse VIP player list from SQL. Check VIP players remaining time. (Current UTC Timestmp in seconds from Gameserver: " + tmp_intServerTimestamp.ToString() + ")", 5);
                                        this.SqlVipsActive.Clear();
                                        this.SqlVipsActiveNamesOnly.Clear();
                                        foreach (KeyValuePair<String, Int32> tmp_sqlvips in tmp_sql_vips_active)
                                        {
                                            DebugWrite("[SyncVipList] [SqlListActive] Checking VIP player " + this.strGreen(tmp_sqlvips.Key) + " from SQL database", 5);
                                            if (tmp_sqlvips.Value > tmp_intServerTimestamp)
                                            {
                                                // player timestamp ok
                                                this.SqlVipsActive.Add(tmp_sqlvips.Key, tmp_sqlvips.Value);
                                                this.SqlVipsActiveNamesOnly.Add(tmp_sqlvips.Key);
                                                if (tmp_sqlguid.ContainsKey(tmp_sqlvips.Key)) { tmp_GuidsActive.Add(tmp_sqlguid[tmp_sqlvips.Key], tmp_sqlvips.Key); }
                                                if (tmp_sql_vips2add.Contains(tmp_sqlvips.Key))
                                                {
                                                    DebugWrite("[SyncVipList] [SqlListActive] Receive NEW VIP player " + this.strGreen(tmp_sqlvips.Key) + " with valid VIP Slot time from SQL database", 3);
                                                    tmp_adders++;
                                                    if (this.vipsExpired.Contains(tmp_sqlvips.Key))
                                                    {
                                                        this.vipsExpired.Remove(tmp_sqlvips.Key);
                                                        this.vipmsg.Remove(tmp_sqlvips.Key);
                                                    }
                                                    // set vip player status to "active" in SQL
                                                    SQL = "UPDATE `vsm_vips` SET status='active' WHERE playername='" + tmp_sqlvips.Key + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                    {
                                                        DebugWrite("[SyncVipList] [SqlListActive] Set VIP player status to 'active' in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                        MyCom.ExecuteNonQuery();
                                                    }
                                                }
                                                if (!tmp_gs_vips.Contains(tmp_sqlvips.Key))
                                                {
                                                    if (this.RoundTempVips.Contains(tmp_sqlvips.Key))
                                                    {
                                                        DebugWrite("[SyncVipList] [SqlListActive] [AggressiveJoinAbuse] Valid VIP player " + this.strGreen(tmp_sqlvips.Key) + " blocked to use 'Aggressive Join Kick' till next round.", 5);
                                                    }
                                                    else
                                                    {
                                                        if (tmp_gs_vips.Contains(tmp_sqlvips.Key, StringComparer.CurrentCultureIgnoreCase))
                                                        {
                                                            // remove from gs, case sensitive problem detected
                                                            tmp_gs_vips.Remove(tmp_sqlvips.Key);
                                                            this.ProconVipRemove(tmp_sqlvips.Key);
                                                            this.ProconVipSave();
                                                            this.ProconVipList();
                                                            DebugWrite("[SyncVipList] [SqlListActive] Change VIP Slot playername to " + this.strGreen(tmp_sqlvips.Key) + " on Gameserver (case sensitive).", 3);
                                                        }
                                                        // add player to gameserver
                                                        DebugWrite("[SyncVipList] [SqlListActive] ^bAdd^n VIP Slot: " + this.strGreen(tmp_sqlvips.Key) + " from SQL database to Gameserver", 2);
                                                        this.ProconVipAdd(tmp_sqlvips.Key);
                                                        tmp_gs_vips.Add(tmp_sqlvips.Key);
                                                        SyncGameserver = true;
                                                    }
                                                }
                                                else
                                                {
                                                    DebugWrite("[SyncVipList] [SqlListActive] Valid VIP player " + this.strGreen(tmp_sqlvips.Key) + " from SQL database already on Gameserver", 5);
                                                }
                                            }
                                            else
                                            {
                                                // vip expired, remove from gameserver
                                                DebugWrite("[SyncVipList] [SqlListExpired] ^bExpired^n VIP Slot: " + this.strGreen(tmp_sqlvips.Key), 3);
                                                this.vipsExpired.Add(tmp_sqlvips.Key);
                                                this.vipmsg.Remove(tmp_sqlvips.Key);
                                                tmp_sql_vips_INVALID.Add(tmp_sqlvips.Key);
                                                // set SQL status to "expired" or "inactive"
                                                tmp_newStatus = "inactive";
                                                if (this.SettingVipExp == enumBoolYesNo.Yes)
                                                {
                                                    tmp_newStatus = "expired";
                                                    this.vipsExpired.Remove(tmp_sqlvips.Key);
                                                }
                                                SQL = "UPDATE `vsm_vips` SET status='" + tmp_newStatus + "', guid=NULL WHERE playername='" + tmp_sqlvips.Key + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                {
                                                    DebugWrite("[SyncVipList] [SqlListExpired] Set VIP player status to '" + tmp_newStatus + "' in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                    MyCom.ExecuteNonQuery();
                                                }
                                                if (tmp_gs_vips.Contains(tmp_sqlvips.Key))
                                                {
                                                    DebugWrite("[SyncVipList] [SqlListExpired] ^bRemove^n expired VIP Slot: " + this.strGreen(tmp_sqlvips.Key) + " from Gameserver", 2);
                                                    this.ProconVipRemove(tmp_sqlvips.Key);
                                                    tmp_gs_vips.Remove(tmp_sqlvips.Key);
                                                    SyncGameserver = true;
                                                }
                                                if (this.RoundTempVips.Contains(tmp_sqlvips.Key)) { this.RoundTempVips.Remove(tmp_sqlvips.Key); }
                                            }
                                        }

                                        // check ea guids from vips
                                        if (this.EAGuidTracking == enumBoolYesNo.Yes)
                                        {
                                            foreach (KeyValuePair<String, String> tmp_guid in this.Guid2Check)
                                            {
                                                if (tmp_GuidsActive.ContainsKey(tmp_guid.Key))
                                                {
                                                    if (tmp_GuidsActive[tmp_guid.Key] == tmp_guid.Value)
                                                    {
                                                        DebugWrite("[SyncVipList] [EAGuidTracking] VIP " + this.strGreen(tmp_guid.Value) + " already linked to EA GUID: " + tmp_guid.Key, 5);
                                                    }
                                                    else
                                                    {
                                                        // vip changed his playername
                                                        DebugWrite("[SyncVipList] [EAGuidTracking] VIP " + tmp_GuidsActive[tmp_guid.Key] + " (" + tmp_guid.Key + ") changed his playername to " + this.strGreen(tmp_guid.Value) + ". Updating new playername in SQL database.", 2);
                                                        if ((this.SettingAdkatsLog == enumBoolYesNo.Yes) && (this.SettingAdkatsLogVipChanged == enumBoolYesNo.Yes)) { this.AdkatsPlayerLog(tmp_guid.Value, "VIP Slot updated to new playername " + tmp_guid.Value); }
                                                        SQL = "DELETE FROM `vsm_vips` WHERE `vsm_vips`.playername='" + tmp_guid.Value + "' AND gametype = '" + this.SettingGameType + "' AND status != 'active'";
                                                        using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                        {
                                                            DebugWrite("[SyncVipList] [EAGuidTracking] Remove duplicate entries in SQL. SQL COMMAND (MyCom): " + SQL, 5);
                                                            MyCom.ExecuteNonQuery();
                                                        }
                                                        SQL = "UPDATE IGNORE `vsm_vips` SET playername='" + tmp_guid.Value + "' WHERE playername='" + tmp_GuidsActive[tmp_guid.Key] + "' AND gametype = '" + this.SettingGameType + "'";
                                                        using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                        {
                                                            DebugWrite("[SyncVipList] [EAGuidTracking] Change VIP Slot playername from " + tmp_GuidsActive[tmp_guid.Key] + " to " + this.strGreen(tmp_guid.Value) + " for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                            MyCom.ExecuteNonQuery();
                                                            this.PlayerSayMsg(tmp_guid.Value, tmp_guid.Value + " your VIP Slot will be changed from playername " + tmp_GuidsActive[tmp_guid.Key] + " to " + this.strGreen(tmp_guid.Value) + " in few minutes.");
                                                            this.PlayerYellMsg(tmp_guid.Value, tmp_guid.Value + " your VIP Slot will be changed to playername " + this.strGreen(tmp_guid.Value));
                                                            this.ProconVipRemove(tmp_GuidsActive[tmp_guid.Key]);
                                                            SyncGameserver = true;
                                                            if (tmp_gs_vips.Contains(tmp_GuidsActive[tmp_guid.Key])) { tmp_gs_vips.Remove(tmp_GuidsActive[tmp_guid.Key]); }
                                                            if (!tmp_sql_vips_INVALID.Contains(tmp_GuidsActive[tmp_guid.Key])) { tmp_sql_vips_INVALID.Add(tmp_GuidsActive[tmp_guid.Key]); }
                                                        }
                                                    }
                                                }
                                                else if (this.SqlVipsActive.ContainsKey(tmp_guid.Value))
                                                {
                                                    // link guid to vip playername
                                                    DebugWrite("[SyncVipList] [EAGuidTracking] VIP " + this.strGreen(tmp_guid.Value) + " is now linked to EA GUID: " + tmp_guid.Key, 4);
                                                    SQL = "UPDATE `vsm_vips` SET guid='" + tmp_guid.Key + "' WHERE playername='" + tmp_guid.Value + "' AND gametype = '" + this.SettingGameType + "'";
                                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                    {
                                                        DebugWrite("[SyncVipList] [EAGuidTracking] VIP " + this.strGreen(tmp_guid.Value) + " linked to EA GUID " + tmp_guid.Key + " for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                        MyCom.ExecuteNonQuery();
                                                    }
                                                }
                                                else
                                                {
                                                    DebugWrite("[SyncVipList] [EAGuidTracking] EA GUID: " + tmp_guid.Key + " from " + tmp_guid.Value + " is not linked.", 5);
                                                }
                                            }
                                            this.Guid2Check.Clear();
                                        }

                                        // SQL set expired slots to inactive when "notify vip slot expired" is disabled in plugin settings
                                        if (this.SettingVipExp == enumBoolYesNo.No)
                                        {
                                            if (this.vipsExpired.Count > 0)
                                            {
                                                SQL = "UPDATE `vsm_vips` SET status='inactive', guid=NULL WHERE status = 'expired' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                {
                                                    DebugWrite("[SyncVipList] [SqlListExpired] Set SQL database status to 'inactive' from all expired VIP players for this Server Group. SQL COMMAND (MyCom): " + SQL, 5);
                                                    MyCom.ExecuteNonQuery();
                                                    MyCom.Connection.Close();
                                                }
                                                this.vipsExpired.Clear();
                                            }
                                        }

                                        // list to remove from gameserver, del from sql
                                        foreach (String vipplayer in tmp_sql_vips2del)
                                        {
                                            // DEL from SQL
                                            SQL = "DELETE FROM `vsm_vips` WHERE `vsm_vips`.playername='" + vipplayer + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "' AND status = 'deleting'";
                                            using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                            {
                                                DebugWrite("[SyncVipList] [SqlListDeleting] ^bDELETE^n player in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                MyCom.ExecuteNonQuery();
                                                MyCom.Connection.Close();
                                            }
                                            if (tmp_gs_vips.Contains(vipplayer))
                                            {
                                                this.ProconVipRemove(vipplayer);
                                                DebugWrite("[SyncVipList] [SqlListDeleting] ^bDELETE^n player " + this.strGreen(vipplayer) + " from Gameserver", 5);
                                                tmp_gs_vips.Remove(vipplayer);
                                                SyncGameserver = true;
                                            }
                                            if (this.RoundTempVips.Contains(vipplayer)) { this.RoundTempVips.Remove(vipplayer); }
                                        }

                                        // list to remove from gameserver, set sql status to 'inactive'
                                        if (tmp_sql_vips2inactive.Count > 0)
                                        {
                                            // update SQL player status
                                            SQL = "UPDATE `vsm_vips` SET status='inactive', guid=NULL WHERE status = 'removing' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                            using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                            {
                                                DebugWrite("[SyncVipList] [SqlListDeleting] Set players status from 'removing' to 'inactive' in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                MyCom.ExecuteNonQuery();
                                                MyCom.Connection.Close();
                                            }
                                            foreach (String vipplayer in tmp_sql_vips2inactive)
                                            {
                                                if (tmp_gs_vips.Contains(vipplayer))
                                                {
                                                    this.ProconVipRemove(vipplayer);
                                                    DebugWrite("[SyncVipList] [SqlListRemoving] ^bREMOVE^n player " + this.strGreen(vipplayer) + " from Gameserver", 4);
                                                    tmp_gs_vips.Remove(vipplayer);
                                                    SyncGameserver = true;
                                                }
                                                if (this.RoundTempVips.Contains(vipplayer)) { this.RoundTempVips.Remove(vipplayer); }
                                            }
                                        }

                                        // check vips from gameserver
                                        String tmp_imp_status = "inactive";
                                        String tmp_imp_days = "30";
                                        String SqlPartTimetamp = "NULL";
                                        String SqlPartTimetamp2 = "NULL";
                                        if (this.SettingSyncGs2Sql.Contains("inactive"))
                                        {
                                            tmp_imp_status = "inactive";
                                            SqlPartTimetamp = "NULL";
                                            SqlPartTimetamp2 = "NULL";
                                        }
                                        else if (this.SettingSyncGs2Sql.Contains("permanent"))
                                        {
                                            tmp_imp_status = "active";
                                            SqlPartTimetamp = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '7' YEAR)";
                                            SqlPartTimetamp2 = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '7' YEAR)";
                                        }
                                        else if (this.SettingSyncGs2Sql.Contains("for "))
                                        {
                                            tmp_imp_days = this.SettingSyncGs2Sql.Replace("yes  (for ", "").Replace(" days)", "").Replace(" ", "");
                                            tmp_imp_status = "active";
                                            SqlPartTimetamp = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + tmp_imp_days + "' DAY)";
                                            SqlPartTimetamp2 = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + tmp_imp_days + "' DAY)";
                                        }
                                        else if (this.SettingSyncGs2Sql.Contains("Plugin installation"))
                                        {
                                            tmp_imp_days = "30";
                                            tmp_imp_status = "active";
                                            SqlPartTimetamp = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + tmp_imp_days + "' DAY)";
                                            SqlPartTimetamp2 = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + tmp_imp_days + "' DAY)";
                                        }
                                        foreach (String vipplayer in tmp_gs_vips)
                                        {
                                            if ((!tmp_sql_vips_active.ContainsKey(vipplayer)) && (!tmp_sql_vips_INVALID.Contains(vipplayer)) && (!this.RoundTempVips.Contains(vipplayer)))
                                            {
                                                if (this.SettingSyncGs2Sql.Contains("remove"))
                                                {
                                                    DebugWrite("[SyncVipList] [GameserverList] ^bRemove^n player " + this.strGreen(vipplayer) + " from Gameserver (Plugin setting: 'Import NEW VIPS from Gameserver to SQL: NO')", 4);
                                                    this.ProconVipRemove(vipplayer);
                                                    SyncGameserver = true;
                                                }
                                                else if (this.SettingSyncGs2Sql.Contains("ignore"))
                                                {
                                                    DebugWrite("[SyncVipList] [GameserverList] " + this.strGreen(vipplayer) + " found on Gameserver but NOT in SQL database", 5);
                                                }
                                                else
                                                {
                                                    DebugWrite("[SyncVipList] [GameserverList] ^bAdd^n NEW VIP: " + this.strGreen(vipplayer) + " from Gameserver to SQL database ^n" + this.SettingSyncGs2Sql.Replace("yes  ", "").Replace("no  ", "").Replace(" first Plugin installation only", "") + "^n", 2);
                                                    // add new player from gameserver to SQL
                                                    tmp_adders++;
                                                    SQL = "INSERT INTO `vsm_vips` (`gametype`, `servergroup`, `playername`, `timestamp`, `status`, `admin`) VALUES ('" + this.SettingGameType + "', '" + this.SettingStrSqlServerGroup + "', '" + vipplayer + "', " + SqlPartTimetamp + ", '" + tmp_imp_status + "', 'Plugin') ON DUPLICATE KEY UPDATE status='" + tmp_imp_status + "', timestamp=" + SqlPartTimetamp2 + ", admin='Plugin'";
                                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Connection))
                                                    {
                                                        DebugWrite("[SyncVipList] [GameserverList] ^bAdd^n NEW VIP to SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                                        MyCom.ExecuteNonQuery();
                                                    }
                                                    if (tmp_imp_status == "inactive")
                                                    {
                                                        // remove from gameserver
                                                        DebugWrite("[SyncVipList] [GameserverList] ^bRemove^n player " + this.strGreen(vipplayer) + " from Gameserver (Status: inactive)", 4);
                                                        tmp_adders--;
                                                        this.ProconVipRemove(vipplayer);
                                                        SyncGameserver = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    DebugWrite("[SyncVipList] Can NOT connect to SQL", 5);
                                    SqlConOK = false;
                                    this.SqlTableExist = false;
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleError("[SyncVipList] SQL Connection Error: " + e);
                                SqlConOK = false;
                                this.SqlTableExist = false;
                            }
                            finally
                            {
                                Connection.Close();
                                DebugWrite("[SyncVipList] Close SQL Connection (Connection)", 5);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleError("[SyncVipList] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + e);
                        SqlConOK = false;
                        this.SqlTableExist = false;
                    }
                }
                else
                {
                    SqlConOK = false;
                }

                if (SyncGameserver)
                {
                    DebugWrite("[SyncVipList] Refresh VIPs on Gameserver", 4);
                    this.ProconVipSave();
                    this.ProconVipList();
                }

                if (!SqlConOK)
                {
                    DebugWrite("[SyncVipList] Sync stopped (SQL Error)", 2);
                    this.SqlTableExist = false;
                    return;
                }
                else
                {
                    if (this.SettingSyncGs2Sql.Contains("Plugin installation"))
                    {
                        this.SettingSyncGs2Sql = "no  (remove from Gameserver)";
                        this.ExecuteCommand("procon.protected.plugins.setVariable", "VipSlotManager", "Import NEW VIPS from Gameserver to SQL", "no  (remove from Gameserver)");
                        ConsoleWrite("[SyncVipList] ^bInstallation finished:^n Plugin setting 'Import new VIP from GS to SQL' set to 'no (remove)");
                    }
                }

                DebugWrite("[SyncVipList] ^bSync finished^n", 5);
                DebugWrite("[SyncVipList] Valid VIPs from SQL database: " + (this.SqlVipsActive.Count + tmp_adders).ToString() + "  ---  VIP Slot Expired: " + this.vipsExpired.Count.ToString(), 5);
                if (this.SqlVipsActive.Count >= 500) { DebugWrite("^b^8WARNING^0^n^8 This Gameserver (Server Group) have more than 500 valid VIPs! BF Gameservers can NOT handle more than 500 VIPs.", 2); }

                if (this.isForceSync)
                {
                    this.isForceSync = false;
                    if (this.SettingProconRulzIni == enumBoolYesNo.Yes) { this.FileWriteProconRulz(); }
                    ConsoleWrite("[ForceSync] ^bForce Sync finished:^n Valid VIP players from SQL database: " + (this.SqlVipsActive.Count + tmp_adders).ToString() + "  ---  VIP Slot Expired: " + this.vipsExpired.Count.ToString());
                    if ((this.SettingAggressiveJoinKickAbuseMax > 1) && (this.RoundTempVips.Count > 0)) { ConsoleWrite("[ForceSync] [AggressiveJoinAbuseProtection] Aggressive Join Kick on current round blocked for " + this.RoundTempVips.Count.ToString() + " valid VIPs: " + String.Join(", ", this.RoundTempVips.ToArray())); }
                }
            }));
            SQLWorker.IsBackground = true;
            SQLWorker.Name = "sqlworker";
            SQLWorker.Start();
        }

        private Boolean addvip(String playername, String days, String admin)
        {
            Boolean SqlConOK = false;
            if ((playername.Length >= 3) && (days.Length >= 1) && (admin.Length >= 3))
            {
                this.TableBuilder();
                if (this.SqlTableExist)
                {
                    try
                    {
                        using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                        {
                            Con.Open();
                            try
                            {
                                if (Con.State == ConnectionState.Open)
                                {
                                    String SqlPartTimetamp = String.Empty;
                                    if (days.Contains("+"))
                                    {
                                        SqlPartTimetamp = "DATE_ADD(IF(TIMESTAMPDIFF(DAY,timestamp,UTC_TIMESTAMP()) >= 0, UTC_TIMESTAMP(), timestamp),INTERVAL '" + days.Replace("+", "") + "' DAY)";
                                    }
                                    else
                                    {
                                        SqlPartTimetamp = "DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + days.Replace("+", "") + "' DAY)";
                                    }

                                    String SQL = "INSERT INTO `vsm_vips` (`gametype`, `servergroup`, `playername`, `timestamp`, `status`, `admin`) VALUES ('" + this.SettingGameType + "', '" + this.SettingStrSqlServerGroup + "', '" + playername + "', DATE_ADD(UTC_TIMESTAMP(),INTERVAL '" + days.Replace("+", "") + "' DAY), 'active', '" + admin + "') ON DUPLICATE KEY UPDATE status='active', timestamp=" + SqlPartTimetamp + ", admin='" + admin + "'";
                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                    {
                                        MyCom.ExecuteNonQuery();
                                        SqlConOK = true;
                                        MyCom.Connection.Close();
                                        this.SyncCounter = 999;
                                    }
                                }
                                else
                                {
                                    DebugWrite("[SQL-addvip] Can NOT connect to SQL", 5);
                                }
                            }
                            catch (Exception c)
                            {
                                ConsoleError("[SQL-addvip] SQL Error (Con): " + c);
                                SqlConOK = false;
                                this.SqlTableExist = false;
                            }
                            finally
                            {
                                Con.Close();
                                DebugWrite("[SQL-addvip] Close SQL Connection (Con)", 5);
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        ConsoleError("[SQL-addvip] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                        SqlConOK = false;
                        this.SqlTableExist = false;
                    }
                }
            }
            if (!SqlConOK)
            {
                DebugWrite("[SQL-addvip] SQL Connection Error. Can not write in SQL", 2);
                this.SqlTableExist = false;
                return false;
            }
            else
            {
                if (this.vipmsg.Contains(playername)) { this.vipmsg.Remove(playername); }
                if (this.vipsExpired.Contains(playername)) { this.vipsExpired.Remove(playername); }
                return true;
            }
        }

        private Boolean removevip(String playername)
        {
            Boolean SqlConOK = false;
            if (playername.Length >= 3)
            {
                this.TableBuilder();
                if (this.SqlTableExist)
                {
                    try
                    {
                        using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                        {
                            Con.Open();
                            try
                            {
                                if (Con.State == ConnectionState.Open)
                                {
                                    String SQL = "UPDATE `vsm_vips` SET status='inactive', guid=NULL WHERE playername='" + playername + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                    DebugWrite("[SQL-removevip] Set player status to 'inactive' in SQL database. SQL COMMAND (MyCom): " + SQL, 4);
                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                    {
                                        MyCom.ExecuteNonQuery();
                                        SqlConOK = true;
                                        MyCom.Connection.Close();
                                        this.SyncCounter = 999;
                                    }
                                }
                                else
                                {
                                    DebugWrite("[SQL-removevip] Can NOT connect to SQL", 5);
                                    this.SqlTableExist = false;
                                }
                            }
                            catch (Exception c)
                            {
                                ConsoleError("[SQL-removevip] SQL Error (Con): " + c);
                                SqlConOK = false;
                                this.SqlTableExist = false;
                            }
                            finally
                            {
                                Con.Close();
                                DebugWrite("[SQL-removevip] Close SQL Connection (Con)", 5);
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        ConsoleError("[removevip] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                        SqlConOK = false;
                        this.SqlTableExist = false;
                    }
                }
            }
            if (!SqlConOK)
            {
                DebugWrite("[SQL-removevip] SQL Connection Error. Can not write in SQL", 2);
                this.SqlTableExist = false;
                return false;
            }
            else
            {
                // remove player from gameserver list
                DebugWrite("[SQL-removevip] ^bRemove^n" + this.strGreen(playername) + " from Gameserver", 4);
                if (this.vipmsg.Contains(playername)) { this.vipmsg.Remove(playername); }
                if (this.vipsExpired.Contains(playername)) { this.vipsExpired.Remove(playername); }
                this.ProconVipRemove(playername);
                this.ProconVipSave();
                this.ProconVipList();
                return true;
            }
        }

        private Boolean changevipname(String oldplayername, String playername)
        {
            Boolean SqlConOK = false;
            if ((playername.Length >= 3) && (oldplayername.Length >= 3))
            {
                this.TableBuilder();
                if (this.SqlTableExist)
                {
                    try
                    {
                        using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                        {
                            Con.Open();
                            try
                            {
                                if (Con.State == ConnectionState.Open)
                                {
                                    String SQL = "DELETE FROM `vsm_vips` WHERE `vsm_vips`.playername='" + playername + "' AND gametype = '" + this.SettingGameType + "' AND status != 'active'";
                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                    {
                                        DebugWrite("[SQL-changevip] Remove duplicate entries in SQL. SQL COMMAND (MyCom): " + SQL, 5);
                                        MyCom.ExecuteNonQuery();
                                    }
                                    DebugWrite("[SQL-changevip] Change VIP Slot playername from " + this.strGreen(oldplayername) + " to " + this.strGreen(playername) + " for all " + this.strBlack(this.SettingGameType) + " Server Groups in SQL database.", 4);
                                    SQL = "UPDATE IGNORE `vsm_vips` SET playername='" + playername + "', guid=NULL WHERE playername='" + oldplayername + "' AND gametype = '" + this.SettingGameType + "'";
                                    using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                    {
                                        MyCom.ExecuteNonQuery();
                                        SqlConOK = true;
                                        MyCom.Connection.Close();
                                        this.SyncCounter = 999;
                                    }
                                }
                                else
                                {
                                    DebugWrite("[SQL-changevip] Can NOT connect to SQL", 5);
                                    this.SqlTableExist = false;
                                }
                            }
                            catch (Exception c)
                            {
                                ConsoleError("[SQL-changevip] SQL Error (Con): " + c);
                                SqlConOK = false;
                                this.SqlTableExist = false;
                            }
                            finally
                            {
                                Con.Close();
                                DebugWrite("[SQL-changevip] Close SQL Connection (Con)", 5);
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        ConsoleError("[changevip] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                        SqlConOK = false;
                        this.SqlTableExist = false;
                    }
                }
            }
            if (!SqlConOK)
            {
                DebugWrite("[SQL-changevip] SQL Connection Error. Can not write in SQL", 2);
                this.SqlTableExist = false;
                return false;
            }
            else
            {
                DebugWrite("[SQL-changevip] ^bRemove^n VIP Slot: " + oldplayername + " from Gameserver", 5);
                this.ProconVipRemove(oldplayername);
                this.ProconVipSave();
                this.ProconVipList();
                return true;
            }
        }

        private Int32 checkvip(String playername)
        {
            Int32 erg = -1;
            if (playername.Length >= 3)
            {
                this.TableBuilder();
                if (this.SqlTableExist)
                {
                    try
                    {
                        using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                        {
                            Con.Open();
                            try
                            {
                                String SQL = "SELECT TIMESTAMPDIFF(SECOND,'1970-01-01',timestamp) AS timestamp FROM `vsm_vips` WHERE playername = '" + playername + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                DebugWrite("[CheckVIP] Connected to SQL. SQL COMMAND (MyCommand): " + SQL, 5);
                                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                                {
                                    DataTable resultTable = this.SQLquery(MyCommand);
                                    if (resultTable.Rows != null)
                                    {
                                        DebugWrite("[CheckVIP] Receive informations from SQL", 5);
                                        foreach (DataRow row in resultTable.Rows)
                                        {
                                            // reading sql
                                            erg = Convert.ToInt32(row["timestamp"].ToString());
                                        }
                                    }
                                    else
                                    {
                                        ConsoleError("[CheckVIP] ERROR: Can NOT receive informations from SQL.");
                                        if (Con.State == ConnectionState.Open)
                                        {
                                            DebugWrite("[CheckVIP] Close SQL Connection (Con)", 5);
                                            Con.Close();
                                        }
                                    }
                                }
                            }
                            catch (Exception c)
                            {
                                ConsoleError("[CheckVIP] Error, can not read from SQL database (MyCommand): " + c);
                            }
                            if (Con.State == ConnectionState.Open)
                            {
                                DebugWrite("[CheckVIP] Close SQL Connection (Con)", 5);
                                Con.Close();
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        ConsoleError("[CheckVIP] Error (Con): " + c);
                    }
                }
            }
            return erg;
        }

        private Boolean SetSqlpStatus(String playername, String newstatus)
        {
            Boolean SqlConOK = false;
            this.TableBuilder();
            if (this.SqlTableExist)
            {
                try
                {
                    using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                    {
                        Con.Open();
                        try
                        {
                            if (Con.State == ConnectionState.Open)
                            {
                                String SQL = "UPDATE `vsm_vips` SET status='" + newstatus + "' WHERE playername='" + playername + "' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                {
                                    MyCom.ExecuteNonQuery();
                                    SqlConOK = true;
                                    MyCom.Connection.Close();
                                }
                            }
                            else
                            {
                                DebugWrite("[SetSqlStatus] Can NOT connect to SQL", 5);
                            }
                        }
                        catch (Exception c)
                        {
                            ConsoleError("[SetSqlStatus] SQL Error (Con): " + c);
                            SqlConOK = false;
                            this.SqlTableExist = false;
                        }
                        finally
                        {
                            Con.Close();
                            DebugWrite("[SetSqlStatus] Close SQL Connection (Con)", 5);
                        }
                    }
                }
                catch (Exception c)
                {
                    ConsoleError("[SetSqlStatus] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                    SqlConOK = false;
                    this.SqlTableExist = false;
                }
            }
            if (!SqlConOK)
            {
                DebugWrite("[SetSqlStatus] SQL Connection Error. Can not write in SQL", 2);
                this.SqlTableExist = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        public void DatabaseCleaner()
        {
            if ((!this.fIsEnabled) || (!this.firstCheck)) { return; }
            Boolean SqlConOK = false;
            this.TableBuilder();
            if (this.SqlTableExist)
            {
                try
                {
                    using (MySqlConnection Con = new MySqlConnection(this.SqlLogin()))
                    {
                        Con.Open();
                        try
                        {
                            if (Con.State == ConnectionState.Open)
                            {
                                String SQL = "DELETE FROM `vsm_vips` WHERE `vsm_vips`.status = 'inactive' AND (TIMESTAMPDIFF(DAY, timestamp, UTC_TIMESTAMP()) > 365) AND comment IS NULL AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                DebugWrite("[SqlAutoDatabaseCleaner] Clean up database", 4);
                                DebugWrite("[SqlAutoDatabaseCleaner] set 'expired' VIPs without join event to 'inactive' in SQL database. SQL COMMAND (MyCom): " + SQL, 5);
                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                {
                                    MyCom.ExecuteNonQuery();
                                }
                                SQL = "UPDATE `vsm_vips` SET status='inactive', guid=NULL WHERE (TIMESTAMPDIFF(DAY, timestamp, UTC_TIMESTAMP()) >= " + this.SettingDBCleaner.ToString() + ") AND status = 'expired' AND gametype = '" + this.SettingGameType + "' AND servergroup = '" + this.SettingStrSqlServerGroup + "'";
                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                {
                                    MyCom.ExecuteNonQuery();
                                }
                                SQL = "UPDATE `vsm_vips` SET timestamp=UTC_TIMESTAMP() WHERE STR_TO_DATE(`timestamp`, '%Y') = '0000-00-00' OR `timestamp` IS NULL";
                                using (MySqlCommand MyCom = new MySqlCommand(SQL, Con))
                                {
                                    MyCom.ExecuteNonQuery();
                                    SqlConOK = true;
                                    this.SyncCounter = 999;
                                    MyCom.Connection.Close();
                                }
                            }
                            else
                            {
                                DebugWrite("[SQL-Auto-Database-Cleaner] Can NOT connect to SQL", 5);
                                this.SqlTableExist = false;
                            }
                        }
                        catch (Exception c)
                        {
                            ConsoleError("[SqlAutoDatabaseCleaner] SQL Error (Con): " + c);
                            SqlConOK = false;
                            this.SqlTableExist = false;
                        }
                        finally
                        {
                            Con.Close();
                            DebugWrite("[SqlAutoDatabaseCleaner] Close SQL Connection (Con)", 5);
                        }
                    }
                }
                catch (Exception c)
                {
                    ConsoleError("[SQL-Auto-Database-Cleaner] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
                    SqlConOK = false;
                    this.SqlTableExist = false;
                }
            }

            if (!SqlConOK)
            {
                DebugWrite("[SqlAutoDatabaseCleaner] SQL Connection Error. Can not write in SQL", 2);
                this.SqlTableExist = false;
            }
        }

        private DataTable SQLquery(MySqlCommand selectQuery)
        {
            DataTable MyDataTable = new DataTable();
            try
            {
                if (selectQuery == null)
                {
                    ConsoleWrite("SQLquery: selectQuery is null");
                    return MyDataTable;
                }
                else if (selectQuery.CommandText.Equals(String.Empty) == true)
                {
                    DebugWrite("[SQLquery] CommandText is empty", 4);
                    return MyDataTable;
                }

                try
                {
                    using (MySqlConnection Connection = new MySqlConnection(this.SqlLogin()))
                    {
                        selectQuery.Connection = Connection;
                        using (MySqlDataAdapter MyAdapter = new MySqlDataAdapter(selectQuery))
                        {
                            if (MyAdapter != null)
                            {
                                MyAdapter.Fill(MyDataTable);
                            }
                            else
                            {
                                DebugWrite("[SQLquery] MyAdapter is null", 4);
                            }
                        }
                        Connection.Close();
                    }
                }
                catch (MySqlException me)
                {
                    ConsoleError("[SQLquery] Error in SQL.");
                    this.DisplayMySqlErrorCollection(me);
                    this.SqlTableExist = false;
                }
                catch (Exception c)
                {
                    ConsoleError("[SQLquery] Error in SQL Query: " + c);
                    this.SqlTableExist = false;
                }
            }
            catch (Exception c)
            {
                ConsoleError("[SQLquery] SQLQuery OuterException: " + c);
                this.SqlTableExist = false;
            }
            return MyDataTable;
        }

        public void DisplayMySqlErrorCollection(MySqlException myException)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Message: " + myException.Message + "^0");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Native: " + myException.ErrorCode.ToString() + "^0");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Source: " + myException.Source.ToString() + "^0");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^1StackTrace: " + myException.StackTrace.ToString() + "^0");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^1InnerException: " + myException.InnerException.ToString() + "^0");
        }

        private Boolean SqlLoginsOk()
        {
            if ((this.SettingStrSqlHostname != String.Empty) && (this.SettingStrSqlPort != String.Empty) && (this.SettingStrSqlDatabase != String.Empty) && (this.SettingStrSqlUsername != String.Empty) && (this.SettingStrSqlPassword != String.Empty))
            {
                return true;
            }
            else
            {
                ConsoleWrite("[SqlLoginDetails]^8^b SQL Server Details not completed (Host IP, Port, Database, Username, PW). Please check your Plugin settings.^0^n");
                DebugWrite("[SqlLoginDetails] SQL Details: Host=`" + this.SettingStrSqlHostname + "` ; Port=`" + this.SettingStrSqlPort + "` ; Database=`" + this.SettingStrSqlDatabase + "` ; Username=`" + this.SettingStrSqlUsername + "` ; Password=`" + this.SettingStrSqlPassword + "`", 2);
                if (this.fIsEnabled)
                {
                    this.ExecuteCommand("procon.protected.plugins.enable", "VipSlotManager", "False");
                }
                return false;
            }
        }
    }
}
