using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Text;

public class Program {
    internal static void Main(string[] args) {
        // Logger
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u4}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // UTF-8
        if (!args.Contains("--disable-utf8")) {
            Console.OutputEncoding = Encoding.UTF8;
            Log.Debug("Current output encoding: UTF-8");
        }

        // 彩色 Logo
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

        string filePath;
        bool shouldReselectInputFile = false;
        do {
            filePath = CLIApi.ChooseFile("Input File: ");

            Log.Debug(@"Path.GetExtension(filePath)" + Path.GetExtension(filePath).ToLower());

            if (!SupportFormat.Container.Contains(Path.GetExtension(filePath).ToLower())) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectInputFile = true;
            } else { shouldReselectInputFile = false; }
        } while (shouldReselectInputFile);
        Log.Information($"Select: {filePath}");

        // FFprobe 输出 INFO
        //char showStreams = 'v';
        StringBuilder inputJsonInfo = new();
        string inputStreamsJsonInfo;
        string inputContainerJsonInfo;
        List<string> jsonInfoArgs = [
            "-show_format",
            "-show_streams",
            "-show_programs",
            "-show_chapter",
            "-show_private_data"
        ];
        inputStreamsJsonInfo = FFprobeJsonInfo(filePath: filePath, jsonInfoArgs[1]);

        int start = inputStreamsJsonInfo.IndexOf('[');
        int end = inputStreamsJsonInfo.LastIndexOf(']');
        inputStreamsJsonInfo = inputStreamsJsonInfo.Remove(end + 1);
        inputStreamsJsonInfo = inputStreamsJsonInfo.Remove(0, start);

        Console.WriteLine(inputStreamsJsonInfo);

        //inputStreamsJsonInfo.
        //inputContainerJsonInfo = FFprobeJsonInfo(filePath, jsonInfoArgs[1]);

        foreach (string arg in jsonInfoArgs) {
            try {
                Log.Debug(System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<dynamic>(FFprobeJsonInfo(filePath, arg))));
            } catch { }
        }

        FileInfo inputFileInfo = new() {
            FileName = Path.GetFileName(filePath),
            FileExt = Path.GetExtension(filePath),
            FilePath = filePath
        };

        inputFileInfo.Streams = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(inputStreamsJsonInfo);
        if (inputFileInfo.Streams == null) {
            throw new ArgumentNullException();
        }
        foreach (var stream in inputFileInfo.Streams) {
            foreach (var tag in stream!) { // 键值
                if (tag.Value is Dictionary<string, string> nestedDict) {
                    foreach (var kvp in nestedDict) {
                        Console.WriteLine($"{kvp.Key.PadRight(24)} : {kvp.Value}");
                    }

                }
                Console.WriteLine($"{tag.Key.PadRight(24)} : {tag.Value}");
            }
        }

        // YAML 预设反序列化到 Preset 类
        // 异常处理【To Do】
        Preset? preset = null;
        string presetPath;
        bool shouldReselectInputPreset = false;
        do {
            presetPath = CLIApi.ChooseFile("YAML Preset File: ");
            if (!SupportFormat.Preset.Contains(Path.GetExtension(presetPath).ToLower())) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectInputPreset = true;
            } else { shouldReselectInputFile = false; }

            preset = DeserializerPreset(presetPath);

        } while (shouldReselectInputPreset);
        Log.Information($"Select: {presetPath}");

        if (preset == null) {
            throw new ArgumentNullException();
        }

        // Select Output && Check
        string outputPath;
        bool shouldReselectOutputFile = false;
        do {
            outputPath = CLIApi.ChooseFile("Output Path: ");
            Log.Information($"Select: {outputPath}");
            if (Directory.Exists(outputPath)) {
                CLIApi.Tips("Output file is a dictionary. Select again.");
                shouldReselectOutputFile = true;
            } else if (!SupportFormat.Container.Contains(Path.GetExtension(outputPath).ToLower())) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectOutputFile = true;
            } else if (File.Exists(outputPath)) {
                shouldReselectOutputFile = !CLIApi.CheckStart("Output file exists. Override? (y/n): ");
            }
        } while (shouldReselectOutputFile);

        do { } while (!CLIApi.CheckStart("Start Processing? (y/n):"));


        // cache path 可被修改【To Do】
        string cachePath = Path.GetDirectoryName(outputPath)!;

        List<(string Category, string FilePath)> outputStreams = [];

        // 视频部分
        string videoCodec;
        StringBuilder VideoEncodeArgs;
        BuildCommand.VideoEncodeArgs(cachePath, preset, out videoCodec, out string? videoStreamCacheFilePath,
            out VideoEncodeArgs);
        outputStreams.Add(("video", videoStreamCacheFilePath));

        // 音频部分
        string audioCodec;
        StringBuilder AudioEncodeArgs;
        BuildCommand.AudioEncodeArgs(cachePath, preset, out audioCodec, out string? audioStreamCacheFilePath,
            out AudioEncodeArgs);
        outputStreams.Add(("audio", audioStreamCacheFilePath));

        string videoEncodeCommand = BuildCommand.CreateVideoEncodeCommand(filePath, videoCodec, VideoEncodeArgs);

        string audioEncodeCommand = BuildCommand.CreateAudioEncodeCommand(filePath, audioCodec, AudioEncodeArgs);

        string muxCommand = BuildCommand.CreateMuxCommand(outputPath, videoStreamCacheFilePath, audioStreamCacheFilePath);

        Log.Information(videoEncodeCommand);
        Compute.CreateProcess(videoEncodeCommand, Path.GetFileNameWithoutExtension(videoCodec));

        Log.Information(audioEncodeCommand);
        Compute.CreateProcess(audioEncodeCommand, Path.GetFileNameWithoutExtension(audioCodec));

        Log.Information(muxCommand);
        Compute.CreateProcess(muxCommand, "FFmpeg");

        Console.WriteLine("\nEncoding Success! File Path: " + outputPath);

        CLIApi.Exit();
    }

    private static string FFprobeJsonInfo(string filePath, string showInfo) {
        using (Process process = new()) {
            StringBuilder output = new();
            process.StartInfo = new() {
                FileName = "cmd.exe",
                Arguments = $"/c \"chcp 65001 >nul && ffprobe -hide_banner -print_format json {showInfo} \"{filePath}\" && timeout /t 0 >nul\"",
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            ;

            process.OutputDataReceived += (_, args) => {
                if (args.Data != null) {
                    output.Append(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            //Log.Debug(output.ToString().Replace("\r", "").Replace("\n", "").Replace("  ", ""));

            return output.ToString();
        }
    }

    private static Preset? DeserializerPreset(string presetPath) {
        Preset? preset = null;
        try {
            preset = global::DeserializerPreset.New(presetPath);
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        } finally {
            Log.Information($"{preset?.Name ?? ""}: {preset?.Description ?? ""}");
        }

        return preset;
    }
}
