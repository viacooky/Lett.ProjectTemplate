public class Settings
{
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
    /// 工作目录
    /// </summary>
    public string WorkDirPath { get; set; }
    /// <summary>
    /// 结果目录
    /// </summarty>
    public string ReleaseDirPath
    {
        get { return $"{WorkDirPath}/build_result"; }
    }
    /// <summary>
    /// 结果目录
    /// </summarty>
    public string ProjectBuildDirPath
    {
        get { return $"{ReleaseDirPath}/{Configuration}"; }
    }
    /// <summary>
    /// NuGet包构建目录
    /// </summarty>
    public string NuGetPackageDirPath
    {
        get { return $"{ReleaseDirPath}/nuget_package"; }
    }
    /// <summary>
    /// VSTest测试结果目录
    /// </summarty>
    public string VSTestResultDirPath
    {
        get { return $"{ReleaseDirPath}/vstest"; }
    }
    /// <summary>
    /// VSTest测试结果文件目录
    /// </summarty>
    public string VSTestResultFilePath
    {
        get { return $"{VSTestResultDirPath}/VSTestResult.xml"; }
    }
    /// <summary>
    /// 代码覆盖率结果目录
    /// </summarty>
    public string CodeCoverageDirPath
    {
        get { return $"{ReleaseDirPath}/code_coverage"; }
    }
    /// <summary>
    /// 文档目录
    /// </summarty>
    public string DocsDirPath
    {
        get { return $"{ReleaseDirPath}/docs_site"; }
    }
}




