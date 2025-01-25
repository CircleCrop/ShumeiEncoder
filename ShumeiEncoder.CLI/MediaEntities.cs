using YamlDotNet.Serialization;

public class Preset {
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public Stream? Video { get; set; }
    public Stream? Audio { get; set; }
    public Stream? Container { get; set; }

    //public Dictionary<string, string>? Metadata { get; set; }
    
    public Dictionary<string, string>? Custom { get; set; }
    public class Stream {
        public required string Fmt { get; set; } // 格式，例如 h264, aac
        public Dictionary<string, string>? Args { get; set; } // 动态参数
        public List<string>? Flags { get; set; }
    }
}


public class FileInfo {
    // 通用, 封装
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string FileExt { get; set; }
    public DateTime? FileDate { get; set; }
    public List<Dictionary<string, object>>? Streams { get; set; }

    //public List<Sub>? SubStream { get; set; }

    /*
    // Video
    public class Video {
        public string? Format { get; set; }
        public (int width, int height)? Resolution { get; set; }
        public float? FrameRate { get; set; } // 23.98/24/25/29.97/30/50/59.94/60
        public int? Bitrate { get; set; } // kbps
        public int? Depth { get; set; } // 8/10
        public int? ChromaSubsampling { get; set; } // 420/422/444
        public string? ColorPrimaries { get; set; } // BT.709
        public string? ColorRange { get; set; } // tv
    }
    // Audio
    public class Audio {
        public string? AudioFormat { get; set; }
        public int? AudioBitrate { get; set; } // kbps
        public int? AudioChannel { get; set; } // 默认 2，立体声
        public int? AudioOffset { get; set; } // ms}
    }*/
    
    /* public JsonDocument document FileInfo(string json) {
     *   JsonDocument document = JsonDocument.Parse(json);
     * }
     */
}


public class DeserializerPreset {
    public static Preset New(string presetPath) {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CustomNamingConvention())
            .Build();

        var yaml = File.ReadAllText(presetPath);
        Preset preset = deserializer.Deserialize<Preset>(yaml);

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

    internal class CustomNamingConvention : INamingConvention {
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
}


public class SupportFormat {
    public static readonly List<string> Container = [".mp4", ".mov", ".mkv", ".m4a", ".flac", ".wav"];
    public static readonly List<string> VideoContainer = [".mp4", ".mov", ".mkv"];
    public static readonly List<string> AudioContainer = [".m4a", ".flac", ".wav"];
    public static readonly List<string> Preset = [".yml", ".yaml", ".txt"];
    public static readonly List<string> VideoStream = [];
    public static readonly List<string> AudioStream = [];
    public SupportFormat() {
        
    }
}