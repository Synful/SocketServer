using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketAdmin.Commands {
    public class command {
        public cmdType type { get; set; }

        public enum cmdType {
            Login = 2,
            Ping,
            Update,
            Disconnect = 666
        }

        public command(cmdType type) {
            this.type = type;
        }
    }
}
