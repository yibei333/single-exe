using CommandLine;
using CommandLine.Text;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SingleExe.Tool;

public class Setting
{
    [Option('n', "name", Required = true, HelpText = "应用名称")]
    public string Name { get; set; }

    [Option('o', "output", Required = true, HelpText = "输出文件路径")]
    public string Output { get; set; }

    [Option('v', "ver", Required = true, HelpText = "应用版本,如1.0.0.0")]
    public string Version { get; set; }

    [Option('b', "binary", Required = true, HelpText = "应用路径")]
    public string BinaryFolder { get; set; }

    [Option('e', "entrypoint", Required = true, HelpText = "可执行文件路径,应用路径的相对地址")]
    public string EntrypointPath { get; set; }

    [Option('i', "icon", Required = false, HelpText = "图标路径,如果为空将尝试从EntryPointPath中提取")]
    public string IconPath { get; set; }

    static Setting _instance;

    public static Setting Create()
    {
        var args = Environment.GetCommandLineArgs();
        Parser.Default
            .ParseArguments<Setting>(args)
            .WithParsed(o => _instance = o)
            .WithNotParsed(HandleParseError);
        return _instance;
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var item in errs)
        {
            Debug.WriteLine(item.ToString());
        }
        _instance = null;
    }

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
}