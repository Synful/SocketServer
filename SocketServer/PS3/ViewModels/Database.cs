using System;
using System.Collections.Generic;
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

        private MySqlConnection con;
        
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
        }

        public bool ValidKey(string lic) {
            try {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                bool ret = rdr.HasRows;

                rdr.Close();
                con.Close();

                return ret;
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
                return false;
            }
        }

        public string GetUsername(string lic) {
            try {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                string ret = rdr["username"].ToString();

                rdr.Close();
                con.Close();

                return ret;
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
                return "";
            }
        }

        public void LogAuth(ClientInfo info, long date) {
            try {
                con.Open();

                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO logins(username, license, ip, mac, psid, checksum, version, date, type, info) VALUES(@name, @lic, @ip, @mac, @psid, @cs, @ver, @date, @type, @code)";
                cmd.Parameters.AddWithValue("@name", $"{info.name}");
                cmd.Parameters.AddWithValue("@lic", $"{info.lic}");
                cmd.Parameters.AddWithValue("@ip", $"{info.ip}");
                cmd.Parameters.AddWithValue("@mac", $"{info.mac}");
                cmd.Parameters.AddWithValue("@psid", $"{info.psid}");
                cmd.Parameters.AddWithValue("@cs", $"{info.checksum}");
                cmd.Parameters.AddWithValue("@ver", $"{info.version}");
                cmd.Parameters.AddWithValue("@date", $"{date}");
                cmd.Parameters.AddWithValue("@type", "PS3");
                cmd.Parameters.AddWithValue("@code", $"{info.code}");
                cmd.ExecuteNonQuery();

                con.Close();
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
            }
        }

        public bool isFreemode() {
            return Settings.instance.freemode;
        }

        public void UpdateMac(string lic, string mac) {
            try {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($"UPDATE users SET mac = '{mac}' WHERE license = '{lic}'", con);
                cmd.ExecuteNonQuery();

                con.Close();
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
            }
        }
        public void UpdatePsid(string lic, string psid) {
            try {
                con.Open();

                MySqlCommand cmd = new MySqlCommand($"UPDATE users SET psid = '{psid}' WHERE license = '{lic}'", con);
                cmd.ExecuteNonQuery();

                con.Close();
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
            }
        }

        public Auth_Codes AuthClient(string lic, string mac, string psid, string cs, string version) {
            try {
                if(lic == "" || mac == "" || psid == "" || cs == "") {
                    return Auth_Codes.EmptyInputs;
                }

                if(!ValidKey(lic)) {
                    return Auth_Codes.InvalidLicense;
                }

                if(version != "3.3") {
                    return Auth_Codes.InvalidVersion;
                }

                //if(cs != Settings.instance.checksum) {
                //    return Auth_Codes.InvalidChecksum;
                //}

                if(mac == "000000000000") {
                    return Auth_Codes.InvalidMac;
                }

                if(psid == "00000000000000000000000000000000") {
                    return Auth_Codes.InvalidPsid;
                }

                con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();

                if(rdr["banned"].ToString() == "True") {
                    return Auth_Codes.UserIsBanned;
                }

                if(convDate(Convert.ToDouble(rdr["enddate"].ToString())) < DateTime.Now && !isFreemode()) {
                    return Auth_Codes.TimeExpired;
                }

                if(rdr["setlock"].ToString() == "1") {
                    if(rdr["mac"].ToString() == "") {
                        UpdateMac(lic, mac);
                    }

                    UpdatePsid(lic, psid);
                    goto mac;
                } else {
                    if(psid != rdr["psid"].ToString()) {
                        return Auth_Codes.InvalidPsid;
                    }
                }
            mac:
                if(mac != rdr["mac"].ToString()) {
                    return Auth_Codes.InvalidMac;
                }

                con.Close();
                return Auth_Codes.AuthSuccess;
            } catch (Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
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
