namespace SingleExe.Tool.Extensions;

internal static class FileExtension
{
    public static void CopyToDirectory(this string sourceDir, string destinationDir, bool recursive, Action<FileInfo> fileCopied = null)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"目录不存在:'{dir.FullName}'");

        var dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
            fileCopied?.Invoke(file);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyToDirectory(subDir.FullName, newDestinationDir, true, fileCopied);
            }
        }
    }
}
