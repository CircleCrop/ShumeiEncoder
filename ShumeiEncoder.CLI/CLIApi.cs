﻿using Serilog;
using System;

internal class CLIApi() {
    internal static string ChooseFile(string prompt) {
        string path;
        bool isValidPath = false;

        do {
            Console.Write(prompt);
            path = Console.ReadLine()?.Trim().Trim('\"', '\'') ?? "";

            if (string.IsNullOrWhiteSpace(path)) {
                continue;
            }

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

    internal static bool CheckStart(string prompt) {
        Console.Write(prompt);
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