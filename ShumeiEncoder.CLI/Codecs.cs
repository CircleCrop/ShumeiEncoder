using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ShumeiEncoder.CLI {
    public class CodecPath {
        // 获取运行路径前缀环境变量
        private static string PathPrefix => Environment.GetEnvironmentVariable("RUNNING_PATH_PREFIX") ?? ".";

        // 判断当前平台是否为 Mac
        private static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        private static string codecDir = "codecs";
        // 编解码器属性
        public static string FFmpeg => Path.Combine(PathPrefix, codecDir, "ffmpeg.exe");
        public static string x264 => Path.Combine(PathPrefix, codecDir, "x264.exe");
        public static string x265 => Path.Combine(PathPrefix, codecDir, "x265.exe");
        public static string x26510b => Path.Combine(PathPrefix, codecDir, "x265-10b.exe");
        public static string Qaac64 => Path.Combine(PathPrefix, codecDir, "qaac64.exe");
        public static string Flac => Path.Combine(PathPrefix, codecDir, "flac.exe");
        public static string Opus => Path.Combine(PathPrefix, codecDir, "opusenc.exe");
        public static string Webp => Path.Combine(PathPrefix, codecDir, "cwebp.exe");

        // 字典映射编解码器名称到路径
        private static readonly Dictionary<string, Func<string>> codecPaths = new() {
        { "x264", () => x264 },
        { "x265", () => x265 },
        { "ffmpeg", () => FFmpeg },
        { "x26510b", () => x26510b },
        { "qaac", () => Qaac64 },
        { "aac", () => Qaac64 },
        { "flac", () => Flac },
        { "opus", () => Opus },
        { "webp", () => Webp }
    };

        // 获取指定的 codec 路径
        public static string GetCodecPath(string codecName) {
            if (codecPaths.TryGetValue(codecName, out var getPath)) {
                string path = getPath();
                if (IsMac) {
                    path = path.Replace(".exe", "");
                }
                return path;
            } else {
                return Path.Combine(PathPrefix, codecDir);
            }
        }
    }
}