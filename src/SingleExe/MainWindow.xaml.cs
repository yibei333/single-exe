using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace SingleExe;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StartAsync();
    }

    async void StartAsync()
    {
        await Task.Yield();
        try
        {
            var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{App.Name}\\{App.Version}");
            var integrityFile = Path.Combine(tempFolder, "integrity.txt");
            var exePath = Path.Combine(tempFolder, App.EntryPoint);
            if (!File.Exists(integrityFile) || !File.Exists(exePath))
            {
                if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
                var zipFile = Path.Combine(tempFolder, "Source.zip");
                if (File.Exists(zipFile)) File.Delete(zipFile);

                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SingleExe.Source.zip");
                var zipStream = new FileStream(zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                stream.CopyTo(zipStream);
                zipStream.Flush();
                zipStream.Dispose();
                stream.Dispose();
                ZipFile.ExtractToDirectory(zipFile, tempFolder);
                File.Delete(zipFile);
            }

            if (!File.Exists(exePath)) throw new Exception($"file not found:{exePath}");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(exePath)
                {
                    WorkingDirectory = new FileInfo(exePath).DirectoryName,
                }
            };
            process.Start();

            if (!File.Exists(integrityFile)) File.Create(integrityFile).Close();
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Application.Current.Shutdown(1);
        }
    }
}