using Serilog;

internal class CLIApi() {
    public static string ChooseFile(string prompt) {
        Console.Write(prompt);
        string path;
        do {
            path = Console.ReadLine() ?? "";
        } while (path.Trim().Trim('\"', '\'') == "");
        return path.Trim().Trim('\"', '\'');
    }
    public static bool CheckStart(string prompt) {
        Console.Write(prompt);
        string input = Console.ReadLine() ?? "";
        return input.Trim() switch {
            "y" => true,
            _ => false,
        };
    }
    public static void Tips(string prompt) {
        Console.WriteLine(prompt + "\n");
    }
    public static void Exit() {
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }

    public static void WelcomeInfomation() {
        Log.Information("Github Url\t: https://github.com/CircleCrop/ShumeiEncoder");
        Log.Information("Author Url\t: https://aiccrop.com");
        Log.Information($"Welcome to ShumeiEncoder!");
        Console.WriteLine(
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
");
    }
}
