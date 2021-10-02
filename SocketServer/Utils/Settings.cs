using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Utils {
    public class Settings {
        private static Settings _inst;
        public static Settings inst {
            get {
                if(_inst == null) {
                    _inst = new Settings();
                    _inst.Load();
                }
                return _inst;
            }
        }

        public uint[] addrs;
        public string checksum;
        public float menu_size;
        public string version;
        public bool freemode;

        public Settings() {
            if(!File.Exists("settings.json")) {
                FileStream f = File.Create("settings.json");
                f.Close();
                this.Save();
            }
        }

        public void Save() {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public void Load() {
            _inst = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
        }
    }
}
