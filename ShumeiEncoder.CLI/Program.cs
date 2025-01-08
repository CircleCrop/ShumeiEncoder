internal class Program {
    private static void Main(string[] args) {
        Console.WriteLine("Welcome to ShumeiEncoder");
        Console.WriteLine("Please input a file...");
        string? newInputPath = Console.ReadLine();
        if (newInputPath == null) {
            return;
        }

    }

    private class Preset {
        public string Name { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string>? MetaData { get; set; }
        public Dictionary<string, string>? VideoStaticArgs { get; set; }
        public Dictionary<string, string>? AudioStaticArgs { get; set; }

        public Dictionary<string, string>? VideoDynamicArgs { get; set; }
        public Dictionary<string, string>? AudioDynamicArgs { get; set; }

        // 构造函数
        public Preset(string name = "Untitled Preset") {
            Name = name; // Untitled Preset
            Version = "1.0";
            MetaData = null;
            VideoStaticArgs = new Dictionary<string, string>();
            AudioStaticArgs = new Dictionary<string, string>();

            VideoDynamicArgs = null;
            AudioDynamicArgs = null;
        }

    }


    private interface InputFileInfo {
        // 通用, 封装
        string fileName { get; }
        string filePath { get; }
        string fileExt { get; }
        DateTime fileDate { get; }
        TimeOnly Duration { get; }
        // Video
        string videoFormat { get; }
        (int width, int height) videoResolution { get; }
        float frameRate { get; } // 23.98/24/25/29.97/30/50/59.94/60
        int videoBitrate { get; } // kbps
        int bitDepth { get; } // 8/10
        int chromaSubsampling { get; } // 420/422/444
        string colorPrimaries { get; } // BT.709
        string colorRange { get; } // Limited
        // Audio
        string audioFormat { get; }
        int audioBitrate { get; } // kbps
        int audioChannel { get; } // 默认 2，立体声

    }
}