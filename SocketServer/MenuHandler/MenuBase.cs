using SocketServer.PC;
using SocketServer.PS3;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = SocketServer.Utils.Console;

namespace SocketServer.MenuHandler {
    public class MenuBase {
        Thread render_t;
        Thread choice_t;
        Thread settings_t;

        public MenuBase() {
            render_t = new Thread(() => render_thread());
            render_t.Start();
            choice_t = new Thread(() => choice_thread());
            choice_t.Start();
            settings_t = new Thread(() => settings_thread());
            settings_t.Start();
        }

        void render_thread() {
            while(true) {
                try {
                    Console.clear();

                    Console.writetitle("Genisys Server Log");

                    foreach(Log l in Logger.inst.logs) {
                        l.render();
                    }
                    Console.writeline("");

                    render_menu();
                } catch { }
                Thread.Sleep(3 * 1000);
            }
        }
        void choice_thread() {
            while(true) {
                try {
                    string cmd = System.Console.ReadLine().ToLower();
                    switch(cmd) {
                        case "kill":
                            Main_PS3.inst.Close();
                            Main_PC.inst.Close();
                            //Environment.Exit(0);
                            break;
                        case "reload":
                            Settings.instance.Load();
                            break;
                    }
                } catch { };
                Thread.Sleep(3 * 1000);
            }
        }
        void settings_thread() {
            while(true) {
                try {
                    Settings.instance.Load();
                } catch { }
                Thread.Sleep(60 * 1000);
            }
        }

        void render_header() {
            Console.writetitle("Genisys Server Info");

            Console.write("Latest Version", ConsoleColor.Cyan);
            Console.write(": ", ConsoleColor.Gray);
            Console.write($"{Settings.instance.version}", ConsoleColor.White);

            Console.write(" | ", ConsoleColor.Gray);

            Console.write("Is Freemode", ConsoleColor.Cyan);
            Console.write(": ", ConsoleColor.Gray);
            Console.write($"{Settings.instance.freemode}", ConsoleColor.White);

            Console.write(" | ", ConsoleColor.Gray);

            Console.write("Users Online", ConsoleColor.Cyan);
            Console.write(": ", ConsoleColor.Gray);
            Console.write($"{Main_PS3.inst.clients.Count}\n\n", ConsoleColor.White);
        }
        void render_commands() {
            Console.writetitle("Genisys Server Command Line");

            Console.writecommand("kill", "Stops the server & disconnects all users.");
            Console.writecommand("reload", "Reloads the server settings.");
            Console.write("\n");
        }
        void render_menu() {
            render_header();
            render_commands();

            Console.write("root", ConsoleColor.Cyan);
            Console.write("@", ConsoleColor.Gray);
            Console.write("genisys", ConsoleColor.Cyan);
            Console.write(".", ConsoleColor.Gray);
            Console.write("auth", ConsoleColor.Cyan);
            Console.write("# ", ConsoleColor.Gray);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
        }
    }
}
