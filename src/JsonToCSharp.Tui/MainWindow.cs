using System.Collections.ObjectModel;
using System.Text;
using JsonToCSharp.App;
using JsonToCSharp.App.Models;
using Terminal.Gui;

namespace JsonToCSharp.Tui;

public class MainWindow : Window
{
    private readonly TextField _inputFileField;
    private readonly TextField _outputDirField;
    private readonly TextField _namespaceField;
    private readonly TextView _previewView;
    private readonly ListView _classList;
    private readonly Label _statusLabel;

    private List<ClassDefinition> _analyzed = new();

    public MainWindow()
    {
        Title = "JSON to C#";
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();

        // ── Menu Bar ──
        var menuBar = new MenuBar
        {
            Menus = new[]
            {
                new MenuBarItem
                {
                    Title = "_File",
                    Children = new[]
                    {
                        new MenuItem { Title = "_Open JSON…", Action = () => BrowseInputFile() },
                        new MenuItem { Title = "_Quit", Action = () => Application.RequestStop() }
                    }
                },
                new MenuBarItem
                {
                    Title = "_Help",
                    Children = new MenuItem[]
                    {
                        new MenuItem { Title = "_About", Action = () => MessageBox.Query("About", "JSON to C# TUI\nBuilt with Terminal.Gui v2", "OK") }
                    }
                }
            }
        };
        Add(menuBar);

        // ── Input row ──
        var inputRow = new FrameView { Title = "Input JSON File", X = 1, Y = Pos.Bottom(menuBar), Width = Dim.Fill(1), Height = 5 };

        _inputFileField = new TextField { X = 1, Y = 0, Width = Dim.Fill(12) };
        _inputFileField.TextChanged += (_, _) => OnInputChanged();

        var browseInputButton = new Button { Text = "Browse", X = Pos.Right(_inputFileField) + 1, Y = 0 };
        browseInputButton.Accepting += (_, e) => { BrowseInputFile(); e.Cancel = true; };

        var loadButton = new Button { Text = "Load", X = Pos.Right(browseInputButton) + 1, Y = 0 };
        loadButton.Accepting += (_, e) => { LoadJson(); e.Cancel = true; };

        inputRow.Add(_inputFileField, browseInputButton, loadButton);
        Add(inputRow);

        // ── Options row ──
        var optionsRow = new FrameView { Title = "Options", X = 1, Y = Pos.Bottom(inputRow), Width = Dim.Fill(1), Height = 5 };

        var nsLabel = new Label { Text = "Namespace:", X = 1, Y = 0 };
        _namespaceField = new TextField { Text = "GeneratedModels", X = Pos.Right(nsLabel) + 1, Y = 0, Width = 30 };

        var outLabel = new Label { Text = "Output Dir:", X = Pos.Right(_namespaceField) + 2, Y = 0 };
        _outputDirField = new TextField { X = Pos.Right(outLabel) + 1, Y = 0, Width = Dim.Fill(12) };

        var browseOutputButton = new Button { Text = "Browse", X = Pos.Right(_outputDirField) + 1, Y = 0 };
        browseOutputButton.Accepting += (_, e) => { BrowseOutputDir(); e.Cancel = true; };

        optionsRow.Add(nsLabel, _namespaceField, outLabel, _outputDirField, browseOutputButton);
        Add(optionsRow);

        // ── Generate + Quit buttons ──
        var buttonRow = new View { X = 0, Y = Pos.Bottom(optionsRow), Height = 1, Width = Dim.Fill() };
        var generateButton = new Button { Text = "Generate C# Files", X = Pos.Center() - 10, Y = 0 };
        generateButton.Accepting += (_, e) => { GenerateFiles(); e.Cancel = true; };
        var quitButton = new Button { Text = "Quit", X = Pos.Center() + 10, Y = 0 };
        quitButton.Accepting += (_, e) => { Application.RequestStop(); e.Cancel = true; };
        buttonRow.Add(generateButton, quitButton);
        Add(buttonRow);

        // ── Split: class list + preview ──
        var splitY = Pos.Bottom(buttonRow);

        var classFrame = new FrameView { Title = "Detected Classes", X = 1, Y = splitY, Width = Dim.Percent(30), Height = Dim.Fill(2) };

        _classList = new ListView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        _classList.SelectedItemChanged += (_, e) => ShowClassPreview(e.Item);
        classFrame.Add(_classList);
        Add(classFrame);

        var previewFrame = new FrameView { Title = "Preview", X = Pos.Right(classFrame), Y = splitY, Width = Dim.Fill(1), Height = Dim.Fill(2) };

        _previewView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), ReadOnly = true, WordWrap = true };
        previewFrame.Add(_previewView);
        Add(previewFrame);

        // ── Status bar ──
        _statusLabel = new Label { Text = "Ready. Open a JSON file to begin.", X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill() };
        Add(_statusLabel);
    }

    private void BrowseInputFile()
    {
        var dialog = new OpenDialog { Title = "Select JSON File", OpenMode = OpenMode.File };
        Application.Run(dialog);
        if (!dialog.Canceled && dialog.Path != null)
        {
            _inputFileField.Text = dialog.Path;
            LoadJson();
        }
    }

    private void BrowseOutputDir()
    {
        var dialog = new OpenDialog { Title = "Select Output Directory", OpenMode = OpenMode.Directory };
        Application.Run(dialog);
        if (!dialog.Canceled && dialog.Path != null)
        {
            _outputDirField.Text = dialog.Path;
        }
    }

    private void OnInputChanged()
    {
        var path = _inputFileField.Text?.ToString() ?? "";
        if (File.Exists(path))
        {
            LoadJson();
        }
    }

    private void LoadJson()
    {
        var path = _inputFileField.Text?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(path))
        {
            SetStatus("Please specify an input JSON file.");
            return;
        }

        if (!File.Exists(path))
        {
            SetStatus($"File not found: {path}");
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            _analyzed = JsonAnalyzer.Analyze(json);

            _classList.SetSource(new ObservableCollection<string>(_analyzed.Select(c => c.ClassName).ToList()));

            SetStatus($"Loaded: {_analyzed.Count} class(es) detected from {Path.GetFileName(path)}");

            if (_analyzed.Count > 0)
            {
                _classList.SelectedItem = 0;
                ShowClassPreview(0);
            }

            if (string.IsNullOrWhiteSpace(_outputDirField.Text?.ToString()))
            {
                _outputDirField.Text = Path.Combine(Path.GetDirectoryName(path)!, "output");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
            MessageBox.Query("Error", ex.Message, "OK");
        }
    }

    private void ShowClassPreview(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _analyzed.Count)
        {
            _previewView.Text = "";
            return;
        }

        var classDef = _analyzed[itemIndex];
        var ns = _namespaceField.Text?.ToString() ?? "GeneratedModels";

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine("[ExcludeCodeCoverage]");
        sb.Append(CSharpGenerator.GenerateClass(classDef));
        sb.AppendLine();

        _previewView.Text = sb.ToString();
    }

    private void GenerateFiles()
    {
        if (_analyzed.Count == 0)
        {
            SetStatus("No classes to generate. Load a JSON file first.");
            return;
        }

        var outputDir = _outputDirField.Text?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(outputDir))
        {
            SetStatus("Please specify an output directory.");
            return;
        }

        var ns = _namespaceField.Text?.ToString() ?? "GeneratedModels";

        try
        {
            FileWriter.WriteClasses(_analyzed, outputDir, ns);
            SetStatus($"Generated {_analyzed.Count} file(s) in {outputDir}");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
            MessageBox.Query("Error", ex.Message, "OK");
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }
}
