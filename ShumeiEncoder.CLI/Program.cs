﻿using Serilog;
using System.Text;

public class Program {
    internal static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u4}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        if (!args.Contains("--disable-utf8")) {
            Console.OutputEncoding = Encoding.UTF8;
            Log.Information("Current output encoding: UTF-8");
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


        string filePath;
        bool shouldReselectInputFile = false;
        do {
            filePath = CLIApi.ChooseFile("Input File: ");
            if (!SupportFormat.Container.Contains(Path.GetExtension(filePath))) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectInputFile = true;
            }
        } while (shouldReselectInputFile);
        Log.Information($"Select: {filePath}");

        string presetPath;
        bool shouldReselectInputPreset = false;
        do {
            presetPath = CLIApi.ChooseFile("YAML Preset File: ");
            if (!SupportFormat.Preset.Contains(Path.GetExtension(presetPath))) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectInputPreset = true;
            }
        } while (shouldReselectInputPreset);
        Log.Information($"Select: {presetPath}");

        // Select Output && Check
        string outputPath;
        bool shouldReselectOutputFile = false;
        do { 
            outputPath = CLIApi.ChooseFile("Output Path: ");
            Log.Information($"Select: {outputPath}");
            if (Directory.Exists(outputPath)) {
                CLIApi.Tips("Output file is a dictionary. Select again.");
                shouldReselectOutputFile = true;
            } else if (!SupportFormat.Container.Contains(Path.GetExtension(outputPath))) {
                CLIApi.Tips("Unsupport format. Select again.");
                shouldReselectOutputFile = true;
            } else if (File.Exists(outputPath)) {
                shouldReselectOutputFile = !CLIApi.CheckStart("Output file exists. Override? (y/n): ");
            }
        } while (shouldReselectOutputFile);

        do { } while (!CLIApi.CheckStart("Start Processing? (y/n):"));

        // YAML 预设反序列化到 Preset 类
        // 异常处理【To Do】
        Preset preset = DeserializerPreset.New(presetPath);
        Log.Information($"{preset.Name}: {preset.Description ?? ""}");

        // cache path 可被修改【To Do】
        string cachePath = Path.GetDirectoryName(outputPath)!;

        List<(string Category, string FilePath)> outputStreams = new();

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
}
