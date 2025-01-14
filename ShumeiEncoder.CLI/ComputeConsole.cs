
public abstract class ComputeTask {
    public abstract StreamReader DecodeTask(string inputFilePath, string args, StreamWriter pipe);
    public abstract void EncodeTask(StreamReader pipe, string args, string outputFilePath);
    // FrameProcessTask
    // MuxTask
    // DemuxTask
}

public class ContainTask {

}

internal class ComputeConsole {


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
}