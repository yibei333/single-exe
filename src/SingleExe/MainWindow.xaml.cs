using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SingleExe;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    double _progress;
    string _filename;

    public MainWindow()
    {
        InitializeComponent();
        StartAsync();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress == value) return;
            _progress = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
        }
    }

    public string Filename
    {
        get => _filename;
        set
        {
            if (_filename == value) return;
            _filename = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filename)));
        }
    }

    async void StartAsync()
    {
        await Task.Yield();
        var assembly = Assembly.GetExecutingAssembly();
        try
        {
            var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{App.Name}\\{App.Version}");
            var integrityFile = Path.Combine(tempFolder, "integrity.txt");
            var exePath = Path.Combine(tempFolder, App.EntryPoint);
            if (!File.Exists(integrityFile) || !File.Exists(exePath))
            {
                if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

                var names = assembly.GetManifestResourceNames().Where(x => x.StartsWith("Source")).ToList();
                var totalCount = names.Count;
                var index = 0;
                foreach (var name in names)
                {
                    index++;
                    Progress = Math.Round(index * 100.0 / totalCount, 2);
                    var stream = assembly.GetManifestResourceStream(name);
                    if (stream != null)
                    {
                        var path = Path.Combine(tempFolder, TrimStart(name, "Source\\"));
                        Filename = path;
                        var directoryName = new FileInfo(path).DirectoryName;
                        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
                        using var targetStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.CopyTo(targetStream);
                        targetStream.Flush();
                        stream.Dispose();
                    }
                }
            }

            StartProcess(exePath);
            if (!File.Exists(integrityFile)) File.Create(integrityFile).Close();
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Application.Current.Shutdown(1);
        }
    }

    void StartProcess(string path)
    {
        if (!File.Exists(path)) throw new Exception($"file not found:{path}");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(path)
            {
                WorkingDirectory = new FileInfo(path).DirectoryName,
            }
        };
        process.Start();
    }

    static string TrimStart(string source, string pattern)
    {
        if (source.StartsWith(pattern))
        {
            return source.Substring(pattern.Length);
        }
        return source;
    }
}