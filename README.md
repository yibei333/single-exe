# single-exe [![](https://github.com/yibei333/single-exe/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/yibei333/single-exe/actions/workflows/release.yml)
将你的应用打包为单个可执行文件(exe)，以便于分发。

## 截图
![screenshoot](https://github.com/yibei333/single-exe/blob/main/assets/screenshot.gif?raw=true)

## 使用

#### 1. 前置条件

* 安装了[.NET8 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

#### 2. 安装工具

``` shell
dotnet tool install --global SingleExe.Tool
```

#### 3.USAGE
``` shell
single-exe --binary-folder <value> --entry-point <value> [options]

OPTIONS
* -b|--binary-folder  应用目录
* -e|--entry-point  可执行文件路径,【binary-folder】的相对地址
  -n|--name         应用名称,默认为【entry-point】的文件名
  -o|--output       输出文件路径,默认为【binary-folder】上级目录
  -v|--app-version  应用版本,默认为1.0.0.0
  -i|--icon         图标路径,如果为空将尝试从【entry-point】中提取,如果提取失败则用默认图标
  -h|--help         Shows help text.
  --version         Shows version information.
```

#### 4. 示例

``` shell
single-exe -b yourPath/yourAppRootFolder -e yourApp.exe -o outputDirectory
```
