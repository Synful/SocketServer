using SocketServer.PS3.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Utils {
    public class Log {
        public enum lType {
            Info,
            Auth,
            Command,
            Error
        }

        lType type;
        string date;
        string msg;
        ClientInfo info;

        private void writetype(lType t) {
            switch(t) {
                case lType.Info:
                    Console.write("[", ConsoleColor.Gray);
                    Console.write("Info", ConsoleColor.Magenta);
                    Console.write("]", ConsoleColor.Gray);
                    break;
                case lType.Auth:
                    Console.write("[", ConsoleColor.Gray);
                    Console.write(info.reauth ? "Reauth" : "Auth", ConsoleColor.Cyan);
                    Console.write("] ", ConsoleColor.Gray);
                    Console.write(info.name, ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.lic, ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.mac, ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.psid, ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.checksum, ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.version.ToString(), ConsoleColor.Cyan);
                    Console.write(" | ", ConsoleColor.Gray);
                    Console.write(info.code.ToString(), ConsoleColor.Cyan);
                    break;
                case lType.Command:
                    Console.write("[", ConsoleColor.Gray);
                    Console.write("Command", ConsoleColor.Cyan);
                    Console.write("]", ConsoleColor.Gray);
                    break;
                case lType.Error:
                    Console.write("[", ConsoleColor.Gray);
                    Console.write("Error", ConsoleColor.Red);
                    Console.write("]", ConsoleColor.Gray);
                    break;
            }
            Console.write(" ");
        }

        public void render() {
            Console.write($"{date} ", ConsoleColor.Gray);
            writetype(type);
            Console.write($"{msg}\n", ConsoleColor.Gray);
        }

        public Log(lType t, string d, string m) {
            type = t;
            date = d;
            msg = m;
        }
        public Log(lType t, string d, ClientInfo i) {
            type = t;
            date = d;
            info = i;
        }
    }

    public class Logger {
        private static Logger _inst;
        public static Logger inst {
            get {
                if(_inst == null) {
                    _inst = new Logger();
                }
                return _inst;
            }
        }

        private string logfile = "Logs/log.txt";
        private string errorfile = "Logs/error.txt";
        private string authfile = "Logs/auth.txt";

        public Logger() {
            if(!Directory.Exists("Logs")) {
                Directory.CreateDirectory("Logs");
            }
        }

        public List<Log> logs = new List<Log>(10);

        private void pushlog(Log l) {
            if(logs.Count == 10) {
                logs.RemoveAt(0);
                logs.Add(l);
            } else {
                logs.Add(l);
            }
        }

        public void Info(string msg) {
            pushlog(new Log(Log.lType.Info, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")}", msg));
            File.AppendAllText(logfile, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")} [Info] {msg}\n");
        }
        public void Auth(ClientInfo info) {
            pushlog(new Log(Log.lType.Auth, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")}", info));
            File.AppendAllText(authfile, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")} [{(info.reauth ? "Reauth" : "Auth")}] {info.name} | {info.lic} | {info.mac} | {info.psid} | {info.checksum} | {info.code}\n");
            if(!info.reauth) {
                Database.instance.LogAuth(info, DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }
        public void Command(string msg) {
            pushlog(new Log(Log.lType.Command, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")}", msg));
            //File.AppendAllText(logfile, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")} [Command] {msg}\n");
        }
        public void Error(string msg) {
            pushlog(new Log(Log.lType.Error, $"{DateTime.Now.ToString("MM/dd/yy hh:mm::ss tt")}", msg));
            File.AppendAllText(errorfile, $"{DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt")} [Error] {msg}\n");
        }
    }
}
