using System.Diagnostics;
using System.Reflection;

public abstract class ComputeTask {
    public abstract void DecodeTask(string inputFilePath,
                                            string codec,
                                            string args,
                                            StreamReader pipe,
                                            StreamReader stderr);
    public abstract void EncodeTask(StreamReader pipe,
                                    string codec,
                                    string args,
                                    string outputFilePath,
                                    StreamReader stderr);
    // FrameProcessTask
    // MuxTask
    // DemuxTask
}

internal class ComputeApi : ComputeTask {
    public override void DecodeTask(string inputFilePath, string codec, string args, StreamReader pipe, StreamReader stderr) {
        string codecPath = GetCodecPath(codec) ?? CodecPath.FFmpeg;

        // Set up the process
        Process process = NewProcess(inputFilePath, codecPath, args);
        process.Start();

        // Assign out parameters for the output and error streams
        pipe = process.StandardOutput;
        stderr = process.StandardError;

        process.WaitForExit();
    }

    public override void EncodeTask(StreamReader pipe, string codec, string args, string outputFilePath, StreamReader stderr) {
        throw new NotImplementedException();
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

    private static Process NewProcess(string inputFilePath, string? codecPath, string args) {
        return new Process {
            StartInfo = new ProcessStartInfo {
                FileName = codecPath,
                Arguments = $"-i \"{inputFilePath}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }
}