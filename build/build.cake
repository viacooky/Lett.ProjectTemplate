// === 加载依赖 ==========================================
#module nuget:?package=Cake.DotNetTool.Module&version=0.3.0
#tool dotnet:?package=coverlet.console&version=1.5.3
#tool nuget:?package=Cake.CoreCLR&version=0.33.0
#addin nuget:?package=Cake.DocFx&version=0.13.0
#tool nuget:?package=docfx.console&version=2.42.4
#addin nuget:?package=Cake.Coverlet&version=2.3.4

#load "cake_script/settings.cake"


// === 参数 ==========================================

var settings = new Settings{
   Context            = Context,
   Target             = Argument("target", "Help"),
   Configuration      = Argument("configuration", "Release"),
   buildCsprojFilePath="../src/dotNetCore.Lib/dotNetCore.Lib.csproj",
   testCsprojFilePath ="../src/dotNetCore.Lib.MSTest/dotNetCore.Lib.MSTest.csproj",
   NuGetVersion       = Argument("nugetVer","0.0.0"),
   AssemblyVersion    = Argument("assemblyVer", "0.0.0"),
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

   if(settings.Target.ToLower().Equals("help")) return;

   Information($"任务 [{settings.Target}] 开始执行...");
   Information(settings.GetRuntimeMsg());
   Information("");
   Information($"Project Base Path: {settings.ProjectBasePath}");
   Information($"ReleaseDirPath Path: {settings.ReleaseDirPath}");
   Information($"NuGet Version: {settings.NuGetVersion}");
   Information($"Configuration: {settings.Configuration}");
   Information($"Assembly Version: {settings.AssemblyVersion}");
});

Teardown(ctx =>
{
   Information($"任务 [{settings.Target}] 执行完成");
});


// === 内部任务 ==========================================

Task("_clean")
   .Does(() =>
{
   Information("任务 [清理] 开始执行...");
   DotNetCoreClean(settings.buildCsprojFilePath);
   DotNetCoreClean(settings.testCsprojFilePath);
   CleanDirectory(settings.ReleaseDirPath);
   Information("任务 [清理] 执行完成");
});

Task("_dotNetCore_Build")
   .Does(() =>
{
   Information($"任务 [项目构建任务] 开始执行: {settings.buildCsprojFilePath}");
   Information($"还原NuGet包: {settings.buildCsprojFilePath}");
   DotNetCoreRestore(settings.buildCsprojFilePath);
   DotNetCoreBuild(settings.buildCsprojFilePath, new DotNetCoreBuildSettings{
      Configuration         = settings.Configuration,
      OutputDirectory       = settings.ProjectBuildDirPath,
      ArgumentCustomization = args => args.Append($"/p:Version={settings.NuGetVersion}")
                                          .Append($"/p:AssemblyVersion={settings.AssemblyVersion}")
   });
   Information($"任务 [项目构建任务] 执行完成: {settings.buildCsprojFilePath}");
});

Task("_dotNetCore_Test")
   .Does(() =>
{
   Information("任务 [测试] 开始执行...");
   var testSettings = new DotNetCoreTestSettings{
      Configuration    = settings.Configuration,
      NoBuild          = false,
      NoRestore        = false,
      VSTestReportPath = settings.VSTestResultFilePath,
   };
   DotNetCoreTest(settings.testCsprojFilePath, testSettings);
   Information("任务 [测试] 执行完成");
   Information("\r\n");
   Information("VSTest results:");
   var vsTestResults = GetFiles($"{settings.VSTestResultDirPath}/**/*.*");
   foreach(var f in vsTestResults ) Information($"{f.FullPath}");
});

Task("_codeCoverage")
   .Does(() =>
{
   Information("任务 [代码覆盖率] 执行开始...");
   EnsureDirectoryExists(settings.CodeCoverageDirPath);
   var coverletSettings = new CoverletSettings {
          CollectCoverage         = true,
          CoverletOutputFormat    = CoverletOutputFormat.opencover | CoverletOutputFormat.cobertura | CoverletOutputFormat.json | CoverletOutputFormat.lcov,
          CoverletOutputDirectory = settings.CodeCoverageDirPath,
          CoverletOutputName      = "Coverage"
      };
   DotNetCoreBuild(settings.testCsprojFilePath, new DotNetCoreBuildSettings {Configuration = "Debug"});
   Coverlet(new FilePath(settings.testCsprojFilePath), coverletSettings);
   Information("任务 [代码覆盖率] 执行完成");
   Information("\r\n");
   Information("Code Coverage results:");
   var codeCoverageResults = GetFiles($"{settings.CodeCoverageDirPath}/**/*.*");
   foreach(var f in codeCoverageResults ) Information($"{f.FullPath}");
});

Task("_dotNetCore_Pack")
   .Does(() =>
{
   Information("任务 [构建NuGet包] 执行开始...");
   var packSettings = new DotNetCorePackSettings{
      Configuration         = settings.Configuration,
      IncludeSymbols        = false,
      OutputDirectory       = settings.NuGetPackageDirPath,
      ArgumentCustomization = args => args.Append($"/p:Version={settings.NuGetVersion}")
                                          .Append($"/p:AssemblyVersion={settings.AssemblyVersion}")
   };
   DotNetCorePack(settings.buildCsprojFilePath, packSettings);
   Information("任务 [构建NuGet包] 执行完成");
   Information("\r\n");
   Information("nuget package results:");
   var nugetPackageResults = GetFiles($"{settings.NuGetPackageDirPath}/**/*.*");
   foreach(var f in nugetPackageResults ) Information($"{f.FullPath}");
});

Task("_docfx")
   .Does(() =>
{
   Information("任务 [docfx 文档生成] 执行开始...");
   CleanDirectory($"{settings.ProjectBasePath}/docs/_site");
   CleanDirectory($"{settings.ProjectBasePath}/docs/obj");
   CleanDirectory($"{settings.ProjectBasePath}/docs/api");
   Information(settings.ProjectBasePath);
   DocFxMetadata($"{settings.ProjectBasePath}/docs/docfx.json");
   DocFxBuild($"{settings.ProjectBasePath}/docs/docfx.json");
   Context.CopyDirectory(Directory($"{settings.ProjectBasePath}/docs/_site"), Directory(settings.DocsDirPath));
   Information("任务 [docfx 文档生成] 执行完成");
   Information($"docs_site path: {settings.DocsDirPath}");
});

// === 公开命令 ==========================================

Task("Help")
.Does(() => {
   Information(settings.GetHelpMsg());
});

Task("Build")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.Does(() => {
});

Task("Test")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.IsDependentOn("_dotNetCore_Test")
.IsDependentOn("_codeCoverage")
.Does(() => {
});

Task("Nuget")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Pack")
.Does(() => {
});

Task("Docs")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.IsDependentOn("_docfx")
.Does(() => {
});

Task("All")
.IsDependentOn("_clean")
.IsDependentOn("_dotNetCore_Build")
.IsDependentOn("_dotNetCore_Test")
.IsDependentOn("_codeCoverage")
.IsDependentOn("_dotNetCore_Pack")
.IsDependentOn("_docfx")
.Does(() => {
});

RunTarget(settings.Target);
