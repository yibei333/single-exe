using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System.Reflection;
using System.Text;

namespace SingleExe.Tool;

/// <summary>
/// build command
/// </summary>
[Command]
public class BuildCommand : ICommand
{
    /// <summary>
    /// app name
    /// </summary>
    [CommandOption("name", 'n', IsRequired = true, Description = "应用名称")]
    public string Name { get; set; }

    /// <summary>
    /// output file path
    /// </summary>
    [CommandOption("output", 'o', IsRequired = true, Description = "输出文件路径")]
    public string Output { get; set; }

    /// <summary>
    /// app version
    /// </summary>
    [CommandOption("ver", 'v', IsRequired = true, Description = "应用版本,如1.0.0.0")]
    public string Version { get; set; }

    /// <summary>
    /// app binary folder
    /// </summary>
    [CommandOption("binary", 'b', IsRequired = true, Description = "应用路径")]
    public string BinaryFolder { get; set; }

    /// <summary>
    /// app entrypoint file path
    /// </summary>
    [CommandOption("entrypoint", 'e', IsRequired = true, Description = "可执行文件路径,应用路径的相对地址")]
    public string EntrypointPath { get; set; }

    /// <summary>
    /// icon file path
    /// </summary>
    [CommandOption("icon", 'i', IsRequired = false, Description = "图标路径,如果为空将尝试从EntryPointPath中提取")]
    public string IconPath { get; set; }

    /// <summary>
    /// ovveride tostring
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();
        var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            builder.AppendLine($"{property.Name}={property.GetValue(this)}");
        }
        return builder.ToString();
    }

    /// <summary>
    /// build
    /// </summary>
    /// <param name="console">console interface</param>
    /// <returns>task</returns>
    public async ValueTask ExecuteAsync(IConsole console)
    {
        await Task.Yield();
        if (!EnsureParameterCorrect(console)) return;
        console.Output.WriteLine("updated");
        console.Output.WriteLine(this.ToString());
    }

    bool EnsureParameterCorrect(IConsole console)
    {
        if (!Directory.Exists(BinaryFolder))
        {
            console.Output.WriteLine($"directory '{BinaryFolder}' not found");
            return false;
        }

        var entrypointPath = Path.Combine(BinaryFolder, EntrypointPath);
        if (!File.Exists(entrypointPath))
        {
            console.Output.WriteLine($"file '{entrypointPath}' not found");
            return false;
        }

        return true;
    }
}