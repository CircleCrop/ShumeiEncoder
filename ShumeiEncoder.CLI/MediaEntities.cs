public class Preset {
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Version { get; set; }
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
