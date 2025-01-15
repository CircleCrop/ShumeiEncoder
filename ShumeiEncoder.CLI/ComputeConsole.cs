using System.Diagnostics;
using System.Reflection;

/*
 * 此文件暂时废弃，改用 cmd 直接输出，避免自己实现管道。
 */


public abstract class ComputeTask {
    public abstract void DecodeTask(string codec,
                                    string args,
                                    out StreamReader output,
                                    out StreamReader stderr);
    public abstract void EncodeTask(string codec,
                                    string args,
                                    out StreamReader stderr,
                                    StreamReader input);
    // FrameProcessTask
    // MuxTask
    // DemuxTask
}

internal class ComputeApi : ComputeTask {
    public override void DecodeTask(string codec,
                                    string args,
                                    out StreamReader output,
                                    out StreamReader stderr) {
        string codecPath = /*GetCodecPath(codec) ?? */CodecPath.FFmpeg;

        // Set up the process
        Process process = NewProcess(codecPath, args);
        process.Start();

        // Assign out parameters for the output and error streams
        output = process.StandardOutput;
        stderr = process.StandardError;

    }

    public override void EncodeTask(string codec,
                                    string args,
                                    out StreamReader stderr,
                                    StreamReader input) {
        string codecPath = GetCodecPath(codec);

        // Set up the process
        Process process = NewProcess(codecPath, args);
        process.Start();

        // Assign out parameters for the output and error streams

        stderr = process.StandardError;

        while (!input.EndOfStream) {
            string line = input.ReadLine()!;
            process.StandardInput.WriteLine(line);  // 将 FFmpeg 输出的 YUV 数据传递给 x264
        }

        process.WaitForExit();
    }

    void PrepareTask(string VideoCodec) {
        /*if (CLIConsole.CheckStart()) {
            Process ffmpegProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = CodecPath.FFmpegPath,
                    Arguments = $"-i {filePath} -f yuv4mpegpipe -an -v 0 - ",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            StreamReader ffmpegOutput = ffmpegProcess.StandardOutput;
            StreamReader ffmpegError = ffmpegProcess.StandardError;

            Process EncodecProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = CodecPath.x264Path,
                    Arguments = VideoEncodeArgs.ToString(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            StreamReader EncodecOutput = EncodecProcess.StandardOutput;
            StreamReader EncodecError = EncodecProcess.StandardError;

            CLIConsole.Exit();
        }*/
    }

    private static string GetCodecPath(string codec) {
        // Use reflection to get the codec path (ChatGPT)
        string? codecPath;
        try {
            // 获取 CodecPath 类的类型
            Type codecPathType = typeof(CodecPath);

            // 通过反射获取静态属性的值
            PropertyInfo? property = codecPathType.GetProperty(codec, BindingFlags.Public | BindingFlags.Static);
            if (property == null) {
                throw new InvalidOperationException($"Codec '{codec}' is not defined in CodecPath.");
            }

            // 获取属性的值
            codecPath = property.GetValue(null)?.ToString();
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to resolve codec path for '{codec}'.", ex);
        }

        if (string.IsNullOrEmpty(codecPath)) {
            throw new InvalidOperationException($"Codec path for '{codec}' is null or empty.");
        }

        return codecPath;
    }

    private static Process NewProcess(string? codecPath, string args) {
        return new Process {
            StartInfo = new ProcessStartInfo {
                FileName = codecPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }
}