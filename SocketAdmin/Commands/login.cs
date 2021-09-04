using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketAdmin.Commands {
    public class login : command {
        public string username { get; set; }
        public string password { get; set; }
        public bool success { get; set; }

        public login(string username, string password) : base(cmdType.Login) {
            this.username = username;
            this.password = password;
            this.success = false;
        }
        public login(bool success) : base(cmdType.Login) {
            this.success = success;
        }

    }
}
