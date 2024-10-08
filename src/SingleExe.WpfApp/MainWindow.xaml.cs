﻿using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace SingleExe.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartAsync();
        }

        void StartAsync()
        {
            Task.Factory.StartNew(() =>
            {
                try
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

                        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SingleExe.WpfApp.Source.zip");
                        var zipStream = new FileStream(zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.CopyTo(zipStream);
                        zipStream.Flush();
                        zipStream.Dispose();
                        stream.Dispose();
                        ZipFile.ExtractToDirectory(zipFile, tempFolder);
                        File.Delete(zipFile);
                    }

                    if (!File.Exists(exePath)) throw new Exception($"file not found:{exePath}");
                    string[] args = new string[0];
                    var environmentArgs = Environment.GetCommandLineArgs();
                    if (environmentArgs.Length > 0) args = environmentArgs.Skip(1).ToArray();

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo(exePath, string.Join(" ", args))
                        {
                            WorkingDirectory = new FileInfo(exePath).DirectoryName,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();

                    if (!File.Exists(versionFile)) File.WriteAllText(versionFile, Config.Version);
                    Dispatcher.Invoke(() => Application.Current.Shutdown());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Dispatcher.Invoke(() => Application.Current.Shutdown());
                }
            });
        }
    }
}
