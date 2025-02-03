using Serilog;
using System.Text;

namespace ShumeiEncoder.CLI {
    internal class CLIApi() {
        internal static string ChooseFile(string prompt) {
            string path;
            bool isValidPath = false;

            do {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("> " + prompt);
                Console.ResetColor();

                path = Console.ReadLine()?.Trim().Trim('\"', '\'') ?? "";

                try {
                    // 使用 Uri 判断路径是否合法，并检查是否为文件
                    Uri uri = new Uri(path, UriKind.Absolute);
                    if (uri.IsFile) {
                        isValidPath = true;
                    }
                } catch {
                    isValidPath = false;
                }
            } while (!isValidPath);

            return path;
        }

        internal void ProcessBar(string prcesssName) {
            // Running x264.exe | 13.00% | 88.42 f/s, 6652.25 kb/s | ETA 0:05:00
            // Origin Format: 856 frames: 168.44 fps, 5731.65 kb/s
            StringBuilder rendered = new();

            void Update(string output) {
                int width = Console.WindowWidth;
                rendered.Append($"Running {prcesssName}: ");
            }
        }

        internal static bool CheckStart(string prompt) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("> " + prompt);
            Console.ResetColor();
            string input = Console.ReadLine() ?? "";
            return input.Trim() switch {
                "y" => true,
                _ => false,
            };
        }

        internal static void Tips(string prompt) {
            Console.WriteLine(prompt + "\n");
        }

        internal static void Exit() {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        internal static void WelcomeInfomation() {
            Log.Information("Github Url\t: https://github.com/CircleCrop/ShumeiEncoder");
            Log.Information("Author Url\t: https://aiccrop.com");
            Log.Information("Welcome to ShumeiEncoder!");

            const string logoLayer1 =
    @"        _            _       _    _                  _   _         _             _                           
       / /\         / /\    / /\ /\_\               /\_\/\_\ _    /\ \          /\ \                         
      / /  \       / / /   / / // / /         _    / / / / //\_\ /  \ \         \ \ \                        
     / / /\ \__   / /_/   / / / \ \ \__      /\_\ /\ \/ \ \/ / // /\ \ \        /\ \_\                       
    / / /\ \___\ / /\ \__/ / /   \ \___\    / / //  \____\__/ // / /\ \_\      / /\/_/                       
    \ \ \ \/___// /\ \___\/ /     \__  /   / / // /\/________// /_/_ \/_/     / / /                          
     \ \ \     / / /\/___/ /      / / /   / / // / /\/_// / // /____/\       / / /                           
 _    \ \ \   / / /   / / /      / / /   / / // / /    / / // /\____\/      / / /                            
/_/\__/ / /  / / /   / / /      / / /___/ / // / /    / / // / /______  ___/ / /__                           
\ \/___/ /  / / /   / / /      / / /____\/ / \/_/    / / // / /_______\/\__\/_/___\                          
 \_____\/   \/_/    \/_/       \/_________/          \/_/ \/__________/\/_________/                          
                     _            _              _             _            _            _            _      
                    /\ \         /\ \     _    /\ \           /\ \         /\ \         /\ \         /\ \    
                   /  \ \       /  \ \   /\_\ /  \ \         /  \ \       /  \ \____   /  \ \       /  \ \   
                  / /\ \ \     / /\ \ \_/ / // /\ \ \       / /\ \ \     / /\ \_____\ / /\ \ \     / /\ \ \  
                 / / /\ \_\   / / /\ \___/ // / /\ \ \     / / /\ \ \   / / /\/___  // / /\ \_\   / / /\ \_\ 
                / /_/_ \/_/  / / /  \/____// / /  \ \_\   / / /  \ \_\ / / /   / / // /_/_ \/_/  / / /_/ / / 
               / /____/\    / / /    / / // / /    \/_/  / / /   / / // / /   / / // /____/\    / / /__\/ /  
              / /\____\/   / / /    / / // / /          / / /   / / // / /   / / // /\____\/   / / /_____/   
             / / /______  / / /    / / // / /________  / / /___/ / / \ \ \__/ / // / /______  / / /\ \ \     
            / / /_______\/ / /    / / // / /_________\/ / /____\/ /   \ \___\/ // / /_______\/ / /  \ \ \    
            \/__________/\/_/     \/_/ \/____________/\/_________/     \/_____/ \/__________/\/_/    \_\/    
";
            const string logoLayer2 =
    @"        _                         _                                _                            
       / /\                      /\_\                             /\ \                          
      / /  \                    / / /         _                  /  \ \                         
     / / /\ \__                 \ \ \__      /\_\               / /\ \ \                        
    / / /\ \___\                 \ \___\    / / /              / / /\ \_\                       
    \ \ \ \/___/                  \__  /   / / /              / /_/_ \/_/                       
     \ \ \                        / / /   / / /              / /____/\                          
 _    \ \ \                      / / /   / / /              / /\____\/                          
/_/\__/ / /                     / / /___/ / /              / / /______                          
\ \/___/ /                     / / /____\/ /              / / /_______\                         
 \_____\/                      \/_________/               \/__________/                         
                                  _                            _                         _      
                                 /\ \     _                   /\ \                      /\ \    
                                /  \ \   /\_\                /  \ \                    /  \ \   
                               / /\ \ \_/ / /               / /\ \ \                  / /\ \ \  
                              / / /\ \___/ /               / / /\ \ \                / / /\ \_\ 
                             / / /  \/____/               / / /  \ \_\              / /_/_ \/_/ 
                            / / /    / / /               / / /   / / /             / /____/\    
                           / / /    / / /               / / /   / / /             / /\____\/    
                          / / /    / / /               / / /___/ / /             / / /______    
                         / / /    / / /               / / /____\/ /             / / /_______\   
                         \/_/     \/_/                \/_________/              \/__________/   
";

            (int x, int y) startPosition = Console.GetCursorPosition();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logoLayer1);

            Console.ResetColor();
            Console.SetCursorPosition(startPosition.x, startPosition.y);
            Console.ForegroundColor = ConsoleColor.Green;

            foreach (char c in logoLayer2) {
                if (c == ' ') {
                    (int x, int y) = Console.GetCursorPosition();
                    Console.SetCursorPosition(x + 1, y);
                } else {
                    Console.Write(c);
                }
                //Thread.Sleep(1);
            }

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}