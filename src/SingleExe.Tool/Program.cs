using CliFx;

//var setting = Setting.Create();
//if (setting is null) return;
//Console.WriteLine(setting.ToString());

//var exePath = @"C:\Windows\explorer.exe";
//var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.ico");
//IconExtension.SaveIcon(exePath, iconPath);
//Console.WriteLine("ok");
//Console.ReadLine();

await new CliApplicationBuilder()
           .AddCommandsFromThisAssembly()
           .Build()
           .RunAsync();