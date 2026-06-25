using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using JsonToCSharp.App;

var inputFileOption = new Option<FileInfo>(
    aliases: ["-i", "--input"],
    description: "Path to the JSON file to convert")
{
    IsRequired = true
};

var outputDirOption = new Option<DirectoryInfo>(
    aliases: ["-o", "--output"],
    description: "Output directory for generated C# files")
{
    IsRequired = true
};

var namespaceOption = new Option<string>(
    aliases: ["-n", "--namespace"],
    description: "Namespace for generated classes",
    getDefaultValue: () => "GeneratedModels");

var rootCommand = new RootCommand("Convert JSON to C# model classes using System.Text.Json");
rootCommand.AddOption(inputFileOption);
rootCommand.AddOption(outputDirOption);
rootCommand.AddOption(namespaceOption);

rootCommand.SetHandler((inputFile, outputDir, namespaceName) =>
{
    try
    {
        if (!inputFile.Exists)
        {
            Console.Error.WriteLine($"Error: Input file not found: {inputFile.FullName}");
            return;
        }

        var json = File.ReadAllText(inputFile.FullName);
        var classes = JsonAnalyzer.Analyze(json);
        
        FileWriter.WriteClasses(classes, outputDir.FullName, namespaceName);
        
        Console.WriteLine($"Generated {classes.Count} class(es) in {outputDir.FullName}");
        foreach (var c in classes)
        {
            Console.WriteLine($"  - {c.ClassName}.cs");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
    }
}, inputFileOption, outputDirOption, namespaceOption);

var parser = new CommandLineBuilder(rootCommand).Build();
return await parser.InvokeAsync(args);
