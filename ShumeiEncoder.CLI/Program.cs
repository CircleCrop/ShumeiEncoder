//using YamlDotNet.Serialization.NamingConventions;
using System.Text;
using YamlDotNet.Serialization;

public class Program {
    internal static void Main(string[] args) {


        Console.WriteLine("Welcome to ShumeiEncoder");
        Console.WriteLine();

        /*Console.WriteLine("Please input a file...");
        string? newInputPath = Console.ReadLine();
        if (newInputPath == null) {
            return;
        }
        */

        // 选择文件和预设
        string filePath = CLIConsole.InputFile();
        Console.WriteLine();

        string presetPath = CLIConsole.ChoosePreset();
        Console.WriteLine();

        string outputPath = CLIConsole.ChooseOutputPath();
        Console.WriteLine();

        // YAML 预设反序列化到 Preset 类
        var deserializer = new DeserializerBuilder().Build();
        var yaml = File.ReadAllText(presetPath);
        Preset preset = deserializer.Deserialize<Preset>(yaml);
        if (yaml != null) {
            Console.WriteLine(preset.Name);
            Console.WriteLine(preset.Description);
            //Console.WriteLine(preset.Metadata);
            //Console.WriteLine(preset.Video.Fmt);
        }
        //ffmpeg -i input.avs -f yuv4mpegpipe -an -v 0 - | x264 [options] --demuxer y4m -o output.264 -
        StringBuilder VideoEncodeArgs = new();
        string cachePath = outputPath[..outputPath.LastIndexOf('\\')];
        // 构造编码参数
        if (preset.Video?.Fmt != null && preset.Video?.Args != null) {
            VideoEncodeArgs.Append($"--demuxer y4m");
            if (preset.Video.Fmt == "h264" || preset.Video.Fmt == "avc") {
                ArgsToCLIString(preset, VideoEncodeArgs);
            }

            VideoEncodeArgs.Append($" -o {cachePath}\\output_cache.264\" -");
            Console.WriteLine(VideoEncodeArgs.ToString());
            Console.WriteLine();
        }

        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "aac") {
            // 读取音频位深，避免 16 位量化误差【To Do】
            StringBuilder AudioEncodeCommand =
                new($"ffmpeg -i {filePath} -f wav - | qaac64 --tvbr {preset.Audio.Args?["quality"] ?? "127"} -o {cachePath}\\output_cache.m4a\" -");
            // 支持其他 AAC 参数【To Do】
            Console.WriteLine(VideoEncodeArgs.ToString());
            Console.WriteLine();
        }

        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "flac") {

        }

        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "wav") {

        }
    }


    private static void ArgsToCLIString(Preset preset, StringBuilder VideoEncodeCommand) {
        foreach (var videoArgs in preset.Video!.Args!) {
            VideoEncodeCommand.Append($" --{videoArgs.Key}={videoArgs.Value}");
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
        public class Video {
            public string? Format { get; set; }
            public (int width, int height)? Resolution { get; set; }
            public float? FrameRate { get; set; } // 23.98/24/25/29.97/30/50/59.94/60
            public int? Bitrate { get; set; } // kbps
            public int? Depth { get; set; } // 8/10
            public int? ChromaSubsampling { get; set; } // 420/422/444
            public string? ColorPrimaries { get; set; } // BT.709
            public string? ColorRange { get; set; } // Limited
        }
        // Audio
        public class Audio {
            public string? AudioFormat { get; set; }
            public int? AudioBitrate { get; set; } // kbps
            public int? AudioChannel { get; set; } // 默认 2，立体声
            public int? AudioOffset { get; set; } // ms}
        }
    }

    internal class CLIConsole() {
        /*
         * 主菜单 - ：输入文件
         * 子菜单 - 文件处理：1. 添加音/视频流；2. 选择预设。选择 1 后循环一次菜单，选择 2 进入预设载入。
         * 子菜单 - 预设选择：交互对话框，读取到变量。
         * 子菜单 - 执行确认
         * 子菜单 - 按任意键退出
         */
        public static string InputFile() {
            /*var result = Dialog.FileOpen(
                "mp4,mov,mkv,m4a,flac,wav",
                lastSelectMediaFilePath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            );*/
            Console.Write("Input File: ");
            string? path = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(path)) {
                return path.Trim() ?? "";
            } else {
                return "";
            }
        }
        public static string ChoosePreset() {
            /*var result = Dialog.FileOpen(
                "yml,yaml",
                lastSelectPresetFilePath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                );*/
            Console.Write("Choose Preset: ");
            string? path = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(path)) {
                return path.Trim() ?? "";
            } else {
                return "";
            }
        }
        public static Boolean CheckStart() {
            Console.Write("Start Processing? (y/n) ");
            string? input = Console.ReadLine();
            return input?.Trim() switch {
                "y" => true,
                _ => false,
            };
        }
        public static string ChooseOutputPath() {
            Console.Write("Output Path: ");
            string? path = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(path)) {
                return path.Trim() ?? "";
            } else {
                return "";
            }
        }
        public static void Exit() {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}