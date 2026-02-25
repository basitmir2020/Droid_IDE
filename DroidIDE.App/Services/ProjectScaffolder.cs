namespace DroidIDE.App.Services;

/// <summary>
/// Creates .NET project scaffolds directly on disk without requiring the dotnet CLI.
/// Supports console, classlib, webapi, and maui templates.
/// </summary>
public static class ProjectScaffolder
{
    /// <summary>
    /// Creates a new .NET project at the given path using the specified template.
    /// </summary>
    public static void Create(string template, string projectDir, string projectName)
    {
        Directory.CreateDirectory(projectDir);

        var csprojContent = template.ToLowerInvariant() switch
        {
            "console"  => ConsoleCsproj(projectName),
            "classlib" => ClassLibCsproj(projectName),
            "webapi"   => WebApiCsproj(projectName),
            "maui"     => MauiCsproj(projectName),
            _          => ConsoleCsproj(projectName)
        };

        var mainFile = template.ToLowerInvariant() switch
        {
            "console"  => ("Program.cs", ConsoleProgram(projectName)),
            "classlib" => ("Class1.cs",  ClassLibClass(projectName)),
            "webapi"   => ("Program.cs", WebApiProgram(projectName)),
            "maui"     => ("Program.cs", MauiProgram(projectName)),
            _          => ("Program.cs", ConsoleProgram(projectName))
        };

        File.WriteAllText(Path.Combine(projectDir, projectName + ".csproj"), csprojContent);
        File.WriteAllText(Path.Combine(projectDir, mainFile.Item1), mainFile.Item2);

        switch (template.ToLowerInvariant())
        {
            case "webapi":
                WriteWebApiExtras(projectDir, projectName);
                break;
            case "maui":
                WriteMauiExtras(projectDir, projectName);
                break;
        }
    }

    // ── Console ──────────────────────────────────────────────────

    private static string ConsoleCsproj(string name) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
        "  <PropertyGroup>\n" +
        "    <OutputType>Exe</OutputType>\n" +
        "    <TargetFramework>net9.0</TargetFramework>\n" +
        "    <RootNamespace>" + name + "</RootNamespace>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "  </PropertyGroup>\n" +
        "</Project>\n";

    private static string ConsoleProgram(string ns) =>
        "// " + ns + " — Console Application\n\n" +
        "namespace " + ns + ";\n\n" +
        "class Program\n" +
        "{\n" +
        "    static void Main(string[] args)\n" +
        "    {\n" +
        "        Console.WriteLine(\"Hello, World!\");\n" +
        "    }\n" +
        "}\n";

    // ── Class Library ────────────────────────────────────────────

    private static string ClassLibCsproj(string name) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net9.0</TargetFramework>\n" +
        "    <RootNamespace>" + name + "</RootNamespace>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "  </PropertyGroup>\n" +
        "</Project>\n";

    private static string ClassLibClass(string ns) =>
        "namespace " + ns + ";\n\n" +
        "public class Class1\n" +
        "{\n" +
        "    // TODO: Implement your library here\n" +
        "}\n";

    // ── Web API ──────────────────────────────────────────────────

    private static string WebApiCsproj(string name) =>
        "<Project Sdk=\"Microsoft.NET.Sdk.Web\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net9.0</TargetFramework>\n" +
        "    <RootNamespace>" + name + "</RootNamespace>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "  </PropertyGroup>\n" +
        "</Project>\n";

    private static string WebApiProgram(string ns) =>
        "var builder = WebApplication.CreateBuilder(args);\n" +
        "var app = builder.Build();\n\n" +
        "app.MapGet(\"/\", () => \"Hello from " + ns + "!\");\n\n" +
        "app.MapGet(\"/api/hello/{name}\", (string name) =>\n" +
        "    Results.Ok(new { Message = $\"Hello, {name}!\" }));\n\n" +
        "app.Run();\n";

    private static void WriteWebApiExtras(string dir, string name)
    {
        var propertiesDir = Path.Combine(dir, "Properties");
        Directory.CreateDirectory(propertiesDir);

        File.WriteAllText(Path.Combine(propertiesDir, "launchSettings.json"),
            "{\n" +
            "  \"profiles\": {\n" +
            "    \"" + name + "\": {\n" +
            "      \"commandName\": \"Project\",\n" +
            "      \"dotnetRunMessages\": true,\n" +
            "      \"launchBrowser\": false,\n" +
            "      \"applicationUrl\": \"http://localhost:5000\",\n" +
            "      \"environmentVariables\": {\n" +
            "        \"ASPNETCORE_ENVIRONMENT\": \"Development\"\n" +
            "      }\n" +
            "    }\n" +
            "  }\n" +
            "}\n");

        File.WriteAllText(Path.Combine(dir, "appsettings.json"),
            "{\n" +
            "  \"Logging\": {\n" +
            "    \"LogLevel\": {\n" +
            "      \"Default\": \"Information\",\n" +
            "      \"Microsoft.AspNetCore\": \"Warning\"\n" +
            "    }\n" +
            "  },\n" +
            "  \"AllowedHosts\": \"*\"\n" +
            "}\n");
    }

    // ── MAUI ─────────────────────────────────────────────────────

    private static string MauiCsproj(string name) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFrameworks>net9.0-android</TargetFrameworks>\n" +
        "    <OutputType>Exe</OutputType>\n" +
        "    <RootNamespace>" + name + "</RootNamespace>\n" +
        "    <UseMaui>true</UseMaui>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "  </PropertyGroup>\n" +
        "</Project>\n";

    private static string MauiProgram(string ns) =>
        "using Microsoft.Maui.Hosting;\n\n" +
        "namespace " + ns + ";\n\n" +
        "public static class MauiProgram\n" +
        "{\n" +
        "    public static MauiApp CreateMauiApp()\n" +
        "    {\n" +
        "        var builder = MauiApp.CreateBuilder();\n" +
        "        builder\n" +
        "            .UseMauiApp<App>()\n" +
        "            .ConfigureFonts(fonts =>\n" +
        "            {\n" +
        "                fonts.AddFont(\"OpenSans-Regular.ttf\", \"OpenSansRegular\");\n" +
        "            });\n\n" +
        "        return builder.Build();\n" +
        "    }\n" +
        "}\n";

    private static void WriteMauiExtras(string dir, string name)
    {
        File.WriteAllText(Path.Combine(dir, "App.xaml.cs"),
            "namespace " + name + ";\n\n" +
            "public partial class App : Application\n" +
            "{\n" +
            "    public App()\n" +
            "    {\n" +
            "        MainPage = new MainPage();\n" +
            "    }\n" +
            "}\n");

        File.WriteAllText(Path.Combine(dir, "MainPage.xaml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
            "<ContentPage xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"\n" +
            "             xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"\n" +
            "             x:Class=\"" + name + ".MainPage\">\n" +
            "    <VerticalStackLayout Spacing=\"25\" Padding=\"30\">\n" +
            "        <Label Text=\"Hello, World!\" FontSize=\"32\" HorizontalOptions=\"Center\" />\n" +
            "        <Label Text=\"Welcome to .NET MAUI\" FontSize=\"18\" HorizontalOptions=\"Center\" />\n" +
            "    </VerticalStackLayout>\n" +
            "</ContentPage>\n");

        File.WriteAllText(Path.Combine(dir, "MainPage.xaml.cs"),
            "namespace " + name + ";\n\n" +
            "public partial class MainPage : ContentPage\n" +
            "{\n" +
            "    public MainPage()\n" +
            "    {\n" +
            "        InitializeComponent();\n" +
            "    }\n" +
            "}\n");
    }
}
