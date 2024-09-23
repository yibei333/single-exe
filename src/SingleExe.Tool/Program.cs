using SingleExe.Tool.Extensions;

var exePath = @"C:\Windows\explorer.exe";
var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.ico");
IconExtension.SaveIcon(exePath, iconPath);
Console.WriteLine("ok");
Console.ReadLine();