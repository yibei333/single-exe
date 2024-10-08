using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace SingleExe.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{Config.Name}\\{Config.Version}");
            var versionFile = Path.Combine(tempFolder, "version.txt");
            var exePath = Path.Combine(tempFolder, Config.EntryPoint);
            if (!File.Exists(versionFile) || !File.Exists(exePath))
            {
                if (Directory.Exists(tempFolder)) Directory.Delete(tempFolder, true);
                if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
                var zipFile = Path.Combine(tempFolder, "Source.zip");
                if (File.Exists(zipFile)) File.Delete(zipFile);

                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SingleExe.ConsoleApp.Source.zip");
                var zipStream = new FileStream(zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                stream.CopyTo(zipStream);
                zipStream.Flush();
                zipStream.Dispose();
                stream.Dispose();
                ZipFile.ExtractToDirectory(zipFile, tempFolder);
                File.Delete(zipFile);
            }

            if (!File.Exists(exePath)) throw new Exception($"file not found:{exePath}");
            if (!File.Exists(versionFile)) File.WriteAllText(versionFile, Config.Version);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(exePath, string.Join(" ", args))
                {
                    WorkingDirectory = new FileInfo(exePath).DirectoryName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };
            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data)) Console.WriteLine($"error: {args.Data}");
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
