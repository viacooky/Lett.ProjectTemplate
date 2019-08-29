public class Settings
{

    private const string SCRIPT_VER= "0.0.1";
    private const string AUTHOR= "viacooky";

    /// <summary>
    /// 运行时上下文
    /// </summarty>
    public ICakeContext Context{get;set;}
    /// <summary>
    /// 构建命令
    /// </summarty>
    public string Target { get; set; }
    /// <summary>
    /// 编译设置
    /// </summary>
    public string Configuration { get; set; }
    /// <summary>
    /// 构建项目 csproj 文件路径
    /// </summary>
    public string buildCsprojFilePath { get; set; }
    /// <summary>
    /// 测试项目 csproj 文件路径
    /// </summary>
    public string testCsprojFilePath { get; set; }
    /// <summary>
    /// 程序集版本
    /// </summary>
    public string AssemblyVersion { get; set; }
    /// <summary>
    /// NuGet包版本
    /// </summary>
    public string NuGetVersion { get; set; }
    /// <summary>
    /// 项目目录
    /// </summary>
    public string ProjectBasePath { 
        get { return new DirectoryPath(new DirectoryInfo(Context.Environment.WorkingDirectory.FullPath).Parent.FullName).FullPath; }
    }
    /// <summary>
    /// 结果目录
    /// </summarty>
    public string ReleaseDirPath
    {
        get { return new DirectoryPath(ProjectBasePath).Combine("build_result").FullPath; }
    }
    /// <summary>
    /// 结果目录
    /// </summarty>
    public string ProjectBuildDirPath
    {
        get { return new DirectoryPath(ReleaseDirPath).Combine(Configuration).FullPath; }
    }
    /// <summary>
    /// NuGet包构建目录
    /// </summarty>
    public string NuGetPackageDirPath
    {
        get { return new DirectoryPath(ReleaseDirPath).Combine("nuget_package").FullPath; }
    }
    /// <summary>
    /// VSTest测试结果目录
    /// </summarty>
    public string VSTestResultDirPath
    {
        get { return new DirectoryPath(ReleaseDirPath).Combine("nuget_package").FullPath; }
    }
    /// <summary>
    /// VSTest测试结果文件目录
    /// </summarty>
    public string VSTestResultFilePath
    {
        get { return new DirectoryPath(VSTestResultDirPath).CombineWithFilePath("VSTestResult.xml").FullPath; }
    }
    /// <summary>
    /// 代码覆盖率结果目录
    /// </summarty>
    public string CodeCoverageDirPath
    {
        get { return new DirectoryPath(ReleaseDirPath).Combine("code_coverage").FullPath; }
    }
    /// <summary>
    /// 文档目录
    /// </summarty>
    public string DocsDirPath
    {
        get { return new DirectoryPath(ReleaseDirPath).Combine("docs_site").FullPath; }
    }

    /// <summary>
    /// 运行时信息
    /// </summarty>
    public string GetRuntimeMsg(){
        var sb = new StringBuilder();
        var systemBit = Context.Environment.Platform.Is64Bit ? "x64" : "x86";
        sb.Append("\r\n");
        sb.Append($"Platform: \t{Context.Environment.Platform.Family} {systemBit}");
        sb.Append("\r\n");
        sb.Append($"Runtime: \t{Context.Environment.Runtime.Runtime}");
        sb.Append("\r\n");
        sb.Append($"Cake Version: \t{Context.Environment.Runtime.CakeVersion}");
        return sb.ToString();
    }

    /// <summary>
    /// 帮助信息
    /// </summarty>
    public string GetHelpMsg(){
        var sb = new StringBuilder();
        sb.Append("\r\n");
        sb.Append("Cake build script");
        sb.Append("\r\n");
        sb.Append($"版本: {SCRIPT_VER}");
        sb.Append("\r\n");
        sb.Append($"作者: {AUTHOR}");
        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("用法: ./build.sh --target=Build --configuration=Release");
        sb.Append("\r\n");
        sb.Append("参数说明");
        sb.Append("\r\n");
        sb.Append("    --target=Build    \t编译");
        sb.Append("\r\n");
        sb.Append("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
        sb.Append("\r\n");
        sb.Append("        --assemblyVer    \t程序集版本号,默认为 0.0.0");
        sb.Append("\r\n");
        sb.Append("    --target=Test    \t测试,并生成 [测试报告] 及 [代码覆盖率报告] ");
        sb.Append("\r\n");
        sb.Append("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("    --target=Nuget    \t编译,并生成 NuGet Package");
        sb.Append("\r\n");
        sb.Append("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
        sb.Append("\r\n");
        sb.Append("        --nugetVer    \t\tNuGet版本号,默认为 0.0.0");
        sb.Append("\r\n");
        sb.Append("        --assemblyVer    \t程序集版本号,默认为 0.0.0");
        sb.Append("\r\n");
        sb.Append("    --target=Docs    \t生成 docfx 文档");
        return sb.ToString();
    }
}




