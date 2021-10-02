using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.Http;
using SocketServer.Utils;
using System.Collections.Specialized;

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

        private static Database _instance;
        public static Database inst {
            get {
                if(_instance == null) {
                    _instance = new Database();
                }
                return _instance;
            }
        }

        public Database() { }

        public Auth_Codes AuthClient(ClientInfo i) {
            try {
                var response = new WebClient().DownloadString($"http://147.135.120.177/index.php?" +
                                                              $"lic={i.lic}" +
                                                              $"&mac={i.mac}" +
                                                              $"&psid={i.psid}" +
                                                              $"&cs={i.checksum}" +
                                                              $"&ver={i.version}" +
                                                              $"&reauth={i.reauth}" +
                                                              $"&free_mode={Settings.inst.freemode}" +
                                                              $"&checksum={Settings.inst.checksum}" +
                                                              $"&version={Settings.inst.version}").Split('|');
                
                i.name = response[0];
                return (Auth_Codes)(int.Parse(response[1]));
            } catch(Exception ex) {
                Logger.inst.Error(ex.ToString());
                return Auth_Codes.UnknownError;
            }
        }
    }
}
