using NativeFileDialogSharp;

public class Program {
    internal static void Main(string[] args) {
        Console.WriteLine("Welcome to ShumeiEncoder");
        /*Console.WriteLine("Please input a file...");
        string? newInputPath = Console.ReadLine();
        if (newInputPath == null) {
            return;
        }
        */
        string filePath = Menu.InputFile();
        string presetPath = Menu.ChoosePreset();
        Boolean ifChecked = Menu.CheckStart();
        if (ifChecked) {
        }
    }

    public class Preset {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Version { get; set; }
        public Stream? Video { get; set; }
        public Stream? Audio { get; set; }
        public Stream? Container { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public Dictionary<string, string>? Custom { get; set; }
        public class Stream {
            public required string Fmt { get; set; } // 格式，例如 h264, aac
            public Dictionary<string, string>? Args { get; set; } // 动态参数
        }
    }

    public class InputFileInfo {
        // 通用, 封装
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public required string FileExt { get; set; }
        public required DateTime FileDate { get; set; }
        public required TimeOnly Duration { get; set; }
        // Video
        public string? VideoFormat { get; set; }
        public (int width, int height)? VideoResolution { get; set; }
        public float? FrameRate { get; set; } // 23.98/24/25/29.97/30/50/59.94/60
        public int? VideoBitrate { get; set; } // kbps
        public int? BitDepth { get; set; } // 8/10
        public int? ChromaSubsampling { get; set; } // 420/422/444
        public string? ColorPrimaries { get; set; } // BT.709
        public string? ColorRange { get; set; } // Limited
        // Audio
        public string? AudioFormat { get; set; }
        public int? AudioBitrate { get; set; } // kbps
        public int? AudioChannel { get; set; } // 默认 2，立体声
        public int? AudioOffset { get; set; } // ms
    }
    internal static string? lastSelectMediaFilePath;
    internal static string? lastSelectPresetFilePath;
    internal class Menu() {
        /*
         * 主菜单 - ：输入文件
         * 子菜单 - 文件处理：1. 添加音/视频流；2. 选择预设。选择 1 后循环一次菜单，选择 2 进入预设载入。
         * 子菜单 - 预设选择：交互对话框，读取到变量。
         * 子菜单 - 执行确认
         * 子菜单 - 按任意键退出
         */
        public static string InputFile() {
            var result = Dialog.FileOpen(
                "mp4,mov,mkv,m4a,flac,wav",
                lastSelectMediaFilePath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            );
            return result.Path;
        }
        public static string ChoosePreset() {
            var result = Dialog.FileOpen(
                "yml,yaml",
                lastSelectPresetFilePath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                );
            return result.Path;
        }
        public static Boolean CheckStart() {
            Console.WriteLine("Start Processing(y/n)?:");
            string? input = Console.ReadLine();
            return input switch {
                "y" => true,
                _ => false,
            };
        }
        public static void Exit() {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}