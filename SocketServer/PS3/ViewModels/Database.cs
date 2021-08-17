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
                cmd.CommandText = "INSERT INTO logins(username, license, ip, mac, psid, checksum, version, date, type, success, info) VALUES(@name, @lic, @ip, @mac, @psid, @cs, @ver, @date, @type, @success, @code)";
                cmd.Parameters.AddWithValue("@name", $"{info.name}");
                cmd.Parameters.AddWithValue("@lic", $"{info.lic}");
                cmd.Parameters.AddWithValue("@ip", $"{info.ip}");
                cmd.Parameters.AddWithValue("@mac", $"{info.mac}");
                cmd.Parameters.AddWithValue("@psid", $"{info.psid}");
                cmd.Parameters.AddWithValue("@cs", $"{info.checksum}");
                cmd.Parameters.AddWithValue("@ver", $"{info.version}");
                cmd.Parameters.AddWithValue("@date", $"{date}");
                cmd.Parameters.AddWithValue("@type", "PS3");
                cmd.Parameters.AddWithValue("@success", $"{(info.code == Auth_Codes.AuthSuccess ? "1" : "0")}");
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
            Auth_Codes ret = Auth_Codes.AuthSuccess;
            try {
                if(lic == "" || mac == "" || psid == "" || cs == "") {
                    ret = Auth_Codes.EmptyInputs;
                    goto end;
                }

                if(!ValidKey(lic)) {
                    ret = Auth_Codes.InvalidLicense;
                    goto end;
                }

                if(version != "3.3") {
                    ret = Auth_Codes.InvalidVersion;
                    goto end;
                }

                if(cs != Settings.instance.checksum) {
                    ret = Auth_Codes.InvalidChecksum;
                    goto end;
                }

                if(mac == "000000000000") {
                    ret = Auth_Codes.InvalidMac;
                    goto end;
                }

                if(psid == "00000000000000000000000000000000") {
                    ret = Auth_Codes.InvalidPsid;
                    goto end;
                }

                con.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE license = '{lic}'", con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();

                if(rdr["banned"].ToString() == "1") {
                    ret = Auth_Codes.UserIsBanned;
                    goto end;
                }

                if(convDate(Convert.ToDouble(rdr["enddate"].ToString())) < DateTime.Now && !isFreemode()) {
                    ret = Auth_Codes.TimeExpired;
                    goto end;
                }

                if(rdr["setlock"].ToString() == "1") {
                    if(rdr["mac"].ToString() == "") {
                        UpdateMac(lic, mac);
                    }

                    UpdatePsid(lic, psid);
                    goto mac;
                } else {
                    if(psid != rdr["psid"].ToString()) {
                        ret = Auth_Codes.InvalidPsid;
                        goto end;
                    }
                }
            mac:
                if(mac != rdr["mac"].ToString()) {
                    ret = Auth_Codes.InvalidMac;
                }

            end:
                con.Close();
            } catch (Exception ex) {
                Logger.inst.Error(ex.ToString());
                if(con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
                ret = Auth_Codes.UnknownError;
            }
            return ret;
        }

        private DateTime convDate(double unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
