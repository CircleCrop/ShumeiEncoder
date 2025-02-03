using Serilog;
using System.Diagnostics;
using System.Text;

namespace ShumeiEncoder.CLI {
    public abstract class ComputeTask {
        /*
        public abstract void DecodeTask(string codec,
                                        string args,
                                        out StreamReader output,
                                        out StreamReader stderr);
        public abstract void EncodeTask(string codec,
                                        string args,
                                        out StreamReader stderr,
                                        StreamReader input);
        */

        /* 
         * FrameProcessTask
         * MuxTask
         * DemuxTask
         */
    }


    public class Compute {
        public static string FFprobeJsonInfo(string filePath, string showInfo) {
            using Process process = new();
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

            // Log.Debug(output.ToString().Replace("\r", "").Replace("\n", "").Replace("  ", ""));

            string outputJson = output.ToString();

            Log.Debug($"ffprobe -hide_banner -print_format json {showInfo} \"{filePath}\"");

            return outputJson;
        }


        internal static void CreateProcess(string command, string codec = "Compute") {
            // 启动编码
            using (Process process = new()) {
                process.StartInfo = new() {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"chcp 65001 >nul && {command} && timeout /t 0 >nul\"",
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                ;

                process.OutputDataReceived += (_, args) => {
                    /*
                    if (!string.IsNullOrEmpty(args.Data)) {
                        Console.Write($"\r[{DateTime.Now.ToLocalTime()}] [{codec}] {args.Data}");
                    }
                    */
                };

                process.ErrorDataReceived += (_, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) {
                        Console.Write($"\r[{DateTime.Now.ToLocalTime()}] [{codec}] {args.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }

    }


    public class BuildCommand {
        internal static void VideoEncodeArgs(
            string cachePath, Preset preset, out string codec, out string cacheStreamFilePath,
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

        internal static void AudioEncodeArgs(
            string cachePath, Preset preset, out string codec, out string cacheStreamFilePath,
            out StringBuilder AudioEncodeArgs) {

            AudioEncodeArgs = new();
            codec = "";
            cacheStreamFilePath = "";
            if (preset.Audio?.Fmt != null && preset.Audio.Fmt == "aac") {
                codec = CodecPath.Qaac64;
                cacheStreamFilePath = Path.Combine(cachePath, "output_cache.m4a");

                // 读取音频位深，避免 16 位量化误差【To Do】

                if (preset.Audio.Args != null) {
                    string tmp_arg1 = "";
                    string tmp_arg2 = "";
                    // 收集需要移除的键
                    var keysToRemove = new List<string>();

                    foreach (var arg in preset.Audio.Args) {
                        switch (arg.Key) {
                            case "mode":
                                tmp_arg1 = $"--{arg.Value}";
                                keysToRemove.Add(arg.Key);
                                break;

                            case "quality":
                                tmp_arg2 = $" {arg.Value}";
                                keysToRemove.Add(arg.Key);
                                break;
                        }
                    }

                    // 移除收集的键
                    foreach (var key in keysToRemove) {
                        preset.Audio.Args.Remove(key);
                    }

                    // 拼接最终参数
                    AudioEncodeArgs.Append(tmp_arg1 + tmp_arg2);
                }


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
                    foreach (var arg in p_stream!.Args!) {
                        VideoEncodeCommand.Append($" --{arg.Key}={arg.Value}");
                    }
                }
                if (p_stream.Flags != null) {
                    foreach (string flags in p_stream!.Flags!) {
                        VideoEncodeCommand.Append($" --{flags}");
                    }
                }
            }
        }

        public static string CreateAudioEncodeCommand(string filePath, string audioCodec, StringBuilder AudioEncodeArgs) {
            return $"\"{CodecPath.FFmpeg}\" -hide_banner -loglevel error -i \"{filePath}\" -vn -f wav - | \"{audioCodec}\" {AudioEncodeArgs.ToString()}";
        }

        public static string CreateMuxCommand(string outputPath, string videoStreamCacheFilePath, string audioStreamCacheFilePath) {
            return $"\"{CodecPath.FFmpeg}\" -hide_banner -loglevel error -i \"{videoStreamCacheFilePath}\" -i \"{audioStreamCacheFilePath}\" -c copy \"{outputPath}\" -y";
        }

        public static string CreateVideoEncodeCommand(string filePath, string videoCodec, StringBuilder VideoEncodeArgs) {
            return $"\"{CodecPath.FFmpeg}\" -hide_banner -loglevel error -i \"{filePath}\" -f yuv4mpegpipe -pix_fmt yuv420p -an -blocksize 262144 - | \"{videoCodec}\" {VideoEncodeArgs.ToString()}";
            // Blocksize = 256KB
        }
    }
}