using Serilog;
using System.Diagnostics;
using System.Text;
using YamlDotNet.Serialization;

public class Program {
    internal static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}]] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        if (!args.Contains("disable-utf8")) {
            Console.OutputEncoding = Encoding.UTF8;
            Log.Information("Current output encoding: UTF-8.");
        }

        CLIApi.WelcomeInfomation();

        /*Console.WriteLine("Please input a file...");
        string? newInputPath = Console.ReadLine();
        if (newInputPath == null) {
            return;
        }*/

        /*
         * 主菜单 - ：输入文件
         * 子菜单 - 文件处理：1. 添加音/视频流；2. 选择预设。选择 1 后循环一次菜单，选择 2 进入预设载入。
         * 子菜单 - 预设选择：交互对话框，读取到变量。
         * 子菜单 - 执行确认
         * 子菜单 - 按任意键退出
         */


        // 选择文件和预设
        string filePath = CLIApi.ChooseFile("Input File: ");
        Log.Information($"Select: {filePath}");

        string presetPath = CLIApi.ChooseFile("YAML Preset File: ");
        Log.Information($"Select: {presetPath}");

        string outputPath;
        bool shouldReselectOutputFile = false;
        do {
            outputPath = CLIApi.ChooseFile("Output Path: ");
            Log.Information($"Select: {outputPath}");
            if (File.Exists(outputPath)) {
                shouldReselectOutputFile = CLIApi.CheckStart("Output file exists. Override? (y/n): ");
            } else if (Directory.Exists(outputPath)) {
                CLIApi.Tips("Output file is a dictionary. Select again.\n");
                shouldReselectOutputFile = true;
            } else if (!SupportFormat.Container.Contains(Path.GetExtension(outputPath))) {
                CLIApi.Tips("Unsupport format. Select again.\n");
                shouldReselectOutputFile = true;
            } else {
                shouldReselectOutputFile = true;
            }
        } while (!shouldReselectOutputFile);

        CLIApi.CheckStart("Start Processing? (y/n):");

        // YAML 预设反序列化到 Preset 类
        // 异常处理【To Do】
        Preset preset = DeserializerYAML(presetPath);

        string cachePath = Path.GetDirectoryName(outputPath)!;
        List<(string Category, string FilePath)> outputStreams = new();

        // 视频部分
        string videoCodec;
        StringBuilder VideoEncodeArgs;
        BuildArgs.VideoEncodeArgs(cachePath,
                                  preset,
                                  out videoCodec,
                                  out string? videoStreamCacheFilePath,
                                  out VideoEncodeArgs);
        outputStreams.Add(("video", videoStreamCacheFilePath));

        // 音频部分
        string audioCodec;
        StringBuilder AudioEncodeArgs;
        BuildArgs.AudioEncodeArgs(cachePath,
                                  preset,
                                  out audioCodec,
                                  out string? audioStreamCacheFilePath,
                                  out AudioEncodeArgs);
        outputStreams.Add(("audio", audioStreamCacheFilePath));

        string videoEncodeCommand = $"\"{CodecPath.FFmpeg}\" -hide_banner -i \"{filePath}\" -f yuv4mpegpipe -pix_fmt yuv420p -an -blocksize 262144 - | \"{videoCodec}\" {VideoEncodeArgs.ToString()}";
        // Blocksize = 256KB

        string audioEncodeCommand = $"\"{CodecPath.FFmpeg}\" -hide_banner -i \"{filePath}\" -f wav - | \"{audioCodec}\" {AudioEncodeArgs.ToString()}";

        string muxCommand = $"\"{CodecPath.FFmpeg}\" -hide_banner -i \"{videoStreamCacheFilePath}\" -i \"{audioStreamCacheFilePath}\" -c copy \"{outputPath}\"";

        /*
         * Console.Clear();
         * Console.WriteLine(videoEncodeCommand);
         * Console.WriteLine(audioEncodeCommand);
         * Console.WriteLine(muxCommand);
         * Console.WriteLine();
         */

        Console.WriteLine(videoEncodeCommand);
        CreateComputeProcess(videoEncodeCommand, Path.GetFileNameWithoutExtension(videoCodec));

        Console.WriteLine(audioEncodeCommand);
        CreateComputeProcess(audioEncodeCommand, Path.GetFileNameWithoutExtension(audioCodec));

        Console.WriteLine(muxCommand);
        CreateComputeProcess(muxCommand, "FFmpeg");

        Console.WriteLine("\nEncoding Success! File Path: " + outputPath);

        CLIApi.Exit();
    }

    private static Preset DeserializerYAML(string presetPath) {
        var deserializer = new DeserializerBuilder().WithNamingConvention(new CustomNamingConvention()).Build();
        var yaml = File.ReadAllText(presetPath);
        Preset preset = deserializer.Deserialize<Preset>(yaml);
        Log.Information($"{preset.Name}: {preset.Description ?? ""}");
        if (preset.Audio != null) {
            if (preset.Audio.Args == null) {
                preset.Audio.Args = new();
            }
            if (preset.Audio.Fmt == "aac" && !preset.Audio!.Args!.TryGetValue("quality", out _)) {
                preset.Audio!.Args!.Add("quality", "127");
            }
        }

        return preset;
    }

    internal static void CreateComputeProcess(string command, string codec = "Compute") {
        ProcessStartInfo startInfo = new() {
            FileName = "cmd.exe",
            Arguments = $"/c \"chcp 65001 >nul && {command} && timeout /t 0 >nul\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // 启动编码
        using (Process process = new()) {
            process.StartInfo = startInfo;

            process.OutputDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data)) {
                    Log.Information($"[{codec}] {args.Data}");
                }
            };

            process.ErrorDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data)) {
                    Log.Information($"[{codec}] {args.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}


internal class BuildArgs {
    internal static void VideoEncodeArgs(string cachePath,
                                         Preset preset,
                                         out string codec,
                                         out string cacheStreamFilePath,
                                         out StringBuilder VideoEncodeArgs) {
        //ffmpeg -i input.mp4 -f yuv4mpegpipe -an -blocksize 262144 - | x264 [options] --demuxer y4m -o output.264 -
        VideoEncodeArgs = new();
        codec = "";
        cacheStreamFilePath = "";

        // 构造编码参数
        if (preset.Video?.Fmt != null && preset.Video?.Args != null) {
            VideoEncodeArgs.Append($"--demuxer y4m");
            ArgsToCLIString(preset.Video, VideoEncodeArgs);
            if (preset.Video.Fmt == "h264" || preset.Video.Fmt == "avc") {
                codec = CodecPath.x264;
                cacheStreamFilePath = Path.Combine(cachePath ?? "", "output_cache.264") ?? "";
                VideoEncodeArgs.Append($" -o \"{cacheStreamFilePath}\" -");
            } else if (preset.Video.Fmt == "h265" || preset.Video.Fmt == "hevc") {
                codec = CodecPath.x265;
                cacheStreamFilePath = Path.Combine(cachePath ?? "", "output_cache.265") ?? "";
                //
            }

            //Console.WriteLine($"VideoEncodeArgs: {VideoEncodeArgs.ToString()}\n");
        }
    }

    internal static void AudioEncodeArgs(string cachePath,
                                         Preset preset,
                                         out string codec,
                                         out string cacheStreamFilePath,
                                         out StringBuilder AudioEncodeArgs) {
        AudioEncodeArgs = new();
        codec = "";
        cacheStreamFilePath = "";
        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "aac") {
            codec = CodecPath.Qaac64;
            cacheStreamFilePath = Path.Combine(cachePath, "output_cache.m4a");
            // 读取音频位深，避免 16 位量化误差【To Do】
            ArgsToCLIString(preset.Audio, AudioEncodeArgs);
            AudioEncodeArgs.Append($" -o \"{cacheStreamFilePath}\" -");
            // 支持其他 AAC 参数【已完成】

        }

        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "flac") {

        }

        if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "wav") {

        }
    }
    private static void ArgsToCLIString(Preset.Stream p_stream, StringBuilder VideoEncodeCommand) {
        if (p_stream != null) {
            if (p_stream.Args != null) {
                foreach (var videoArgs in p_stream!.Args!) {
                    VideoEncodeCommand.Append($" --{videoArgs.Key}={videoArgs.Value}");
                }
            }
            if (p_stream.Flags != null) {
                foreach (string flags in p_stream!.Flags!) {
                    VideoEncodeCommand.Append($" --{flags}");
                }
            }
        }
    }
}


public class CustomNamingConvention : INamingConvention {
    // 应用命名约定（首字母大写）
    public string Apply(string value) {
        if (string.IsNullOrEmpty(value)) {
            return value;
        }
        return char.ToUpper(value[0]) + value.Substring(1).ToLower();
    }

    // 还原命名约定（如果需要将名称还原到原始形式）
    public string Reverse(string value) => value;
}