// === 加载依赖 ==========================================
#module nuget:?package=Cake.DotNetTool.Module&version=0.3.0
#tool dotnet:?package=coverlet.console&version=1.5.3
#tool nuget:?package=Cake.CoreCLR&version=0.33.0
#addin nuget:?package=Cake.DocFx&version=0.13.0
#tool nuget:?package=docfx.console&version=2.42.4
#addin nuget:?package=Cake.Coverlet&version=2.3.4

#load "cake_script/settings.cake"


// === 参数 ==========================================
const string SCRIPT_VER= "0.0.1";
const string AUTHOR= "viacooky";

var settings = new Settings{
   WorkDirPath = Context.Environment.WorkingDirectory.FullPath,
   Target = Argument("target", "Help"),
   Configuration = Argument("configuration", "Release"),
   buildCsprojFilePath="src/dotNetCore.Lib/dotNetCore.Lib.csproj",
   testCsprojFilePath="src/dotNetCore.Lib.MSTest/dotNetCore.Lib.MSTest.csproj",
   NuGetVersion = Argument("nugetVer","0.0.0"),
   AssemblyVersion = Argument("assemblyVer", "0.0.0"),
};


// === 启动/结束 ==========================================

Setup(ctx =>
{
   // 参数检查
   if(settings.Target.ToLower().Equals("build")){
      if(string.IsNullOrEmpty(settings.Configuration)) throw new CakeException("--configuration 参数不能为空");
   }

   if(settings.Target.ToLower().Equals("nuget")){
      if(string.IsNullOrEmpty(settings.NuGetVersion)) throw new CakeException("--nugetVer 参数不能为空");
   }

   Information($"任务执行开始 : {settings.Target}");
   Information($"参数");
   Information($"Work Path: {settings.WorkDirPath}");
   Information($"NuGet Version: {settings.NuGetVersion}");
   Information($"Configuration: {settings.Configuration}");
   Information($"Assembly Version: {settings.AssemblyVersion}");
});

Teardown(ctx =>
{
   Information("任务执行结束");
});


// === 内部任务 ==========================================

Task("_clean")
   .Does(() =>
{
   Information("清理");
   DotNetCoreClean(settings.buildCsprojFilePath);
   CleanDirectory(new FilePath(settings.buildCsprojFilePath).GetDirectory().Combine("bin"));
   CleanDirectory(new FilePath(settings.buildCsprojFilePath).GetDirectory().Combine("obj"));
   
   DotNetCoreClean(settings.testCsprojFilePath);
   CleanDirectory(new FilePath(settings.testCsprojFilePath).GetDirectory().Combine("bin"));
   CleanDirectory(new FilePath(settings.testCsprojFilePath).GetDirectory().Combine("obj"));
   
   CleanDirectory(settings.ReleaseDirPath);
});

Task("_dotNetCore_Build")
   .Does(() =>
{
   Information($"还原NuGet包: {settings.buildCsprojFilePath}");
   DotNetCoreRestore(settings.buildCsprojFilePath);

   Information($"开始构建: {settings.buildCsprojFilePath}");
   DotNetCoreBuild(settings.buildCsprojFilePath, new DotNetCoreBuildSettings{
      Configuration = settings.Configuration,
      OutputDirectory = settings.ProjectBuildDirPath,
      ArgumentCustomization = args => args.Append($"/p:Version={settings.NuGetVersion}")
                                          .Append($"/p:AssemblyVersion={settings.AssemblyVersion}")
   });
});

Task("_dotNetCore_Test")
   .Does(() =>
{
   Information("开始测试");
   var testSettings = new DotNetCoreTestSettings{
      Configuration = settings.Configuration,
      NoBuild = false,
      NoRestore = false,
      VSTestReportPath = settings.VSTestResultFilePath,
   };
   DotNetCoreTest(settings.testCsprojFilePath, testSettings);
});

Task("_codeCoverage")
   .Does(() =>
{
   Information("代码覆盖率");
   EnsureDirectoryExists(settings.CodeCoverageDirPath);
   var coverletSettings = new CoverletSettings {
          CollectCoverage = true,
          CoverletOutputFormat = CoverletOutputFormat.opencover | CoverletOutputFormat.cobertura | CoverletOutputFormat.json | CoverletOutputFormat.lcov,
          CoverletOutputDirectory = settings.CodeCoverageDirPath,
          CoverletOutputName = "Coverage"
      };
      DotNetCoreBuild(settings.testCsprojFilePath, new DotNetCoreBuildSettings {Configuration = "Debug"});
      Coverlet(new FilePath(settings.testCsprojFilePath), coverletSettings);
});

Task("_dotNetCore_Pack")
   .Does(() =>
{
   var packSettings = new DotNetCorePackSettings{
      Configuration = settings.Configuration,
      IncludeSymbols = false,
      OutputDirectory = settings.NuGetPackageDirPath,
      ArgumentCustomization = args => args.Append($"/p:Version={settings.NuGetVersion}")
                                          .Append($"/p:AssemblyVersion={settings.AssemblyVersion}")
   };
   DotNetCorePack(settings.buildCsprojFilePath, packSettings);
});

Task("_docfx")
   .Does(() =>
{
   Information("生成文档");
   DocFxMetadata("./docfx/docfx.json");
   DocFxBuild("./docfx/docfx.json");
   Context.CopyDirectory(Directory("./docfx/_site"), Directory(settings.DocsDirPath));
});

// === 公开命令 ==========================================

Task("Help")
.Does(() => {
   Information("Lett cake build script");
   Information($"版本: {SCRIPT_VER}");
   Information($"作者: {AUTHOR}");
   Information("用法: ./build.sh --target=Build --configuration=Release");
   Information("参数说明");
   Information("    --target=Build    \t编译");
   Information("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
   Information("        --assemblyVer    \t程序集版本号,默认为 0.0.0");
   Information("");
   Information("    --target=Test    \t测试,并生成 [测试报告] 及 [代码覆盖率报告] ");
   Information("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
   Information("");
   Information("    --target=Nuget    \t编译,并生成 NuGet Package");
   Information("        --configuration    \t编译参数, 默认为 Release, 可选: Release | Debug");
   Information("        --nugetVer    \t\tNuGet版本号,默认为 0.0.0");
   Information("        --assemblyVer    \t程序集版本号,默认为 0.0.0");
});

Task("Build")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.Does(() => {
   Information("构建任务完成");
});

Task("Test")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Test")
.IsDependentOn("_codeCoverage")
.Does(() => {
   Information("测试任务完成");
   Information(".Net Core Test results:");
   var vsTestResults = GetFiles($"{settings.VSTestResultDirPath}/**/*.*");
   foreach(var f in vsTestResults ) Information($"{f.FullPath}");
   Information("\r\n");
   Information("Code Coverage results:");
   var codeCoverageResults = GetFiles($"{settings.CodeCoverageDirPath}/**/*.*");
   foreach(var f in codeCoverageResults ) Information($"{f.FullPath}");
});

Task("Nuget")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Pack")
.Does(() => {
   Information(" NuGet 打包任务完成");
   Information("NuGet Package Version: {settings.nugetVer}}");
   Information("NuGet Package Path:");
   var nugetPackResults = GetFiles($"{settings.NuGetPackageDirPath}/**/*.*");
   foreach(var f in nugetPackResults ) Information($"{f.FullPath}");
});

Task("Docs")
.IsDependentOn("_clean")
.IsDependentOn("_docfx")
.Does(() => {
   Information(" docfx 文档生成任务完成");
   Information($"docs_site path: {settings.DocsDirPath}");
});

Task("All")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.IsDependentOn("_dotNetCore_Test")
.IsDependentOn("_codeCoverage")
.IsDependentOn("_dotNetCore_Pack")
.IsDependentOn("_docfx")
.Does(() => {
   Information("执行所有任务");
});

RunTarget(settings.Target);
