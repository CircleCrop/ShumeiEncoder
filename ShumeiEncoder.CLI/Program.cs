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
        string filePath = CLIApi.ChooseFile("Input File: ");

        string presetPath = CLIApi.ChooseFile("YAML Preset File: ");

        string outputPath = CLIApi.ChooseFile("Output Path: ");

        // YAML 预设反序列化到 Preset 类
        // 异常处理【To Do】
        var deserializer = new DeserializerBuilder().Build();
        var yaml = File.ReadAllText(presetPath);
        Preset preset = deserializer.Deserialize<Preset>(yaml);
        Console.WriteLine($"{preset.Name}: {preset.Description ?? ""}");

        StringBuilder VideoEncodeArgs;
        string cachePath;
        string videoCodec;
        BuildVideoEncodeArgs(outputPath, preset, out videoCodec, out VideoEncodeArgs, out cachePath);

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

    private static void BuildVideoEncodeArgs(string outputPath,
                                             Preset preset,
                                             out string codec,
                                             out StringBuilder VideoEncodeArgs,
                                             out string cachePath) {
        //ffmpeg -i input.avs -f yuv4mpegpipe -an -v 0 - | x264 [options] --demuxer y4m -o output.264 -
        codec = "";
        VideoEncodeArgs = new();
        cachePath = outputPath[..outputPath.LastIndexOf('\\')];

        // 构造编码参数
        if (preset.Video?.Fmt != null && preset.Video?.Args != null) {
            VideoEncodeArgs.Append($"--demuxer y4m");
            ArgsToCLIString(preset, VideoEncodeArgs);
            if (preset.Video.Fmt == "h264" || preset.Video.Fmt == "avc") {
                codec = "x264";
                VideoEncodeArgs.Append($" -o {cachePath}\\output_cache.264\" -");
            } else if (preset.Video.Fmt == "h265" || preset.Video.Fmt == "hevc") {
                codec = "x265";
                //
            }

            Console.WriteLine($"VideoEncodeArgs: {VideoEncodeArgs.ToString()}\n");
        }
    }
    private static void ArgsToCLIString(Preset preset, StringBuilder VideoEncodeCommand) {
        foreach (var videoArgs in preset.Video!.Args!) {
            VideoEncodeCommand.Append($" --{videoArgs.Key}={videoArgs.Value}");
        }
    }
}



internal class CLIApi() {
    /*
     * 主菜单 - ：输入文件
     * 子菜单 - 文件处理：1. 添加音/视频流；2. 选择预设。选择 1 后循环一次菜单，选择 2 进入预设载入。
     * 子菜单 - 预设选择：交互对话框，读取到变量。
     * 子菜单 - 执行确认
     * 子菜单 - 按任意键退出
     */
    public static string ChooseFile(string prompt) {
        Console.Write(prompt);
        string? path = Console.ReadLine();
        Console.WriteLine();
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
    public static void Exit() {
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}
