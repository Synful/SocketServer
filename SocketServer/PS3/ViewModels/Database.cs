using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SocketServer.Utils;

namespace SocketServer.PS3.ViewModels {
    public class Database {
        public enum Auth_Codes { 
            UnknownError,
            TimeExpired,
            UserIsBanned,
            InvalidChecksum,
            InvalidVersion,
            InvalidLicense,
            InvalidMac,
            InvalidPsid,
            EmptyInputs,
            AuthSuccess
        }

        public MySqlConnection con;

        private static Database _instance;
        public static Database instance {
            get {
                if(_instance == null) {
                    _instance = new Database();
                }
                return _instance;
            }
        }

        public Database() {
            con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
            con.Open();
        }

        public bool ValidKey(ClientInfo i) {
            try {
                if(con.State != ConnectionState.Open)
                    con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{i.lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                bool ret = rdr.HasRows;

                rdr.Close();

                return ret;
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                con.Close();
                con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
                con.Open();
                return false;
            }
        }

        public void LogAuth(ClientInfo i, long date) {
            try {
                if(con.State != ConnectionState.Open)
                    con.Open();

                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO logins(username, license, ip, mac, psid, checksum, version, date, type, info) VALUES(@name, @lic, @ip, @mac, @psid, @cs, @ver, @date, @type, @code)";
                cmd.Parameters.AddWithValue("@name", $"{i.name}");
                cmd.Parameters.AddWithValue("@lic", $"{i.lic}");
                cmd.Parameters.AddWithValue("@ip", $"{i.ip}");
                cmd.Parameters.AddWithValue("@mac", $"{i.mac}");
                cmd.Parameters.AddWithValue("@psid", $"{i.psid}");
                cmd.Parameters.AddWithValue("@cs", $"{i.checksum}");
                cmd.Parameters.AddWithValue("@ver", $"{i.version}");
                cmd.Parameters.AddWithValue("@date", $"{date}");
                cmd.Parameters.AddWithValue("@type", "PS3");
                cmd.Parameters.AddWithValue("@code", $"{i.code}");
                cmd.ExecuteNonQuery();
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                con.Close();
                con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
                con.Open();
            }
        }

        public bool isFreemode() {
            return Settings.instance.freemode;
        }

        public void UpdateMac(ClientInfo i) {
            try {
                if(con.State != ConnectionState.Open)
                    con.Open();

                MySqlCommand cmd = new MySqlCommand($"UPDATE users SET mac = '{i.mac}' WHERE license = '{i.lic}'", con);
                cmd.ExecuteNonQuery();

            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                con.Close();
                con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
                con.Open();
            }
        }
        public void UpdatePsid(ClientInfo i) {
            try {
                if(con.State != ConnectionState.Open)
                    con.Open();

                MySqlCommand cmd = new MySqlCommand($"UPDATE users SET psid = '{i.psid}', setlock = 0 WHERE license = '{i.lic}'", con);
                cmd.ExecuteNonQuery();

            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                con.Close();
                con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
                con.Open();
            }
        }

        public Auth_Codes AuthClient(ClientInfo i) {
            try {
                string lic = i.lic, mac = i.mac, psid = i.psid, cs = i.checksum, version = i.version;

                if(lic == "" || mac == "" || psid == "" || cs == "") {
                    return Auth_Codes.EmptyInputs;
                }

                if(!ValidKey(i)) {
                    return Auth_Codes.InvalidLicense;
                }

                if(version != "3.3") {
                    return Auth_Codes.InvalidVersion;
                }

                if(cs != Settings.instance.checksum) {
                    return Auth_Codes.InvalidChecksum;
                }

                if(mac == "000000000000") {
                    return Auth_Codes.InvalidMac;
                }

                if(psid == "00000000000000000000000000000000") {
                    return Auth_Codes.InvalidPsid;
                }

                if(con.State != ConnectionState.Open)
                    con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                i.name = rdr["username"].ToString();
                i.banned = bool.Parse(rdr["banned"].ToString());
                i.enddate = Convert.ToDouble(rdr["enddate"].ToString());
                i.setlock = bool.Parse(rdr["setlock"].ToString());
                i.dbmac = rdr["mac"].ToString();
                i.dbpsid = rdr["psid"].ToString();

                rdr.Close();

                if(i.banned) {
                    return Auth_Codes.UserIsBanned;
                }

                if(convDate(i.enddate) < DateTime.Now && !isFreemode()) {
                    return Auth_Codes.TimeExpired;
                }
                bool updatedMac = false;
                if(i.setlock) {
                    if(i.dbmac == "" || i.dbmac == null || i.dbmac == "NULL") {
                        UpdateMac(i);
                        updatedMac = true;
                    }

                    UpdatePsid(i);
                    goto mac;
                } else {
                    if(psid != i.dbpsid) {
                        return Auth_Codes.InvalidPsid;
                    }
                }
            mac:
                if(mac != i.dbmac && !updatedMac) {
                    return Auth_Codes.InvalidMac;
                }

                return Auth_Codes.AuthSuccess;
            } catch (Exception ex) {
                Logger.inst.Error(ex.ToString());
                con.Close();
                con = new MySqlConnection("server=147.135.120.177;database=genisys;uid=synful;pwd=g,kDS4ig=)+S;");
                con.Open();
                return Auth_Codes.UnknownError;
            }
        }

        private DateTime convDate(double unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
