using System.Text;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IniConfigTroubleshooting.Services;
using IniConfigTroubleshooting.Views;
using AvaloniaEdit.Document;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace IniConfigTroubleshooting.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Ini Config Troubleshooting";

    [ObservableProperty]
    private TextDocument _sourceDocument = new("Please, load file :)");

    [ObservableProperty]
    private TextEditor? _editor = new();

    private RedSquiggleRenderer? _squiggleRenderer;
    private RegistryOptions? _registryOptions;
    private string? _iniScopeName;

    private int _startLine = 0;
    private int _endLine = 0;

    public MainWindowViewModel()
    {
        Editor = new TextEditor
        {
            Document = SourceDocument,
            FontFamily = new Avalonia.Media.FontFamily("Cascadia Code,Consolas,Menlo,Monospace"),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            ShowLineNumbers = true,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible
        };
        Editor.TextChanged += OnEditorTextChanged;
        InitTextMate();
        UpdateSquiggle(_startLine, _endLine);
    }

    private void InitTextMate()
    {
        _registryOptions = new RegistryOptions(ThemeName.Light);
        _iniScopeName = _registryOptions.GetScopeByExtension(".ini");
        if (Editor is null || _registryOptions is null || _iniScopeName is null)
            return;
        Editor.InstallTextMate(_registryOptions)
              .SetGrammar(_iniScopeName);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (Editor?.Document is not null)
            SourceDocument = Editor.Document;
        UpdateSquiggle(_startLine, _endLine);
    }

    private void UpdateSquiggle(int startLine = 0, int endLine = 0)
    {
        if (Editor is null) return;
        _squiggleRenderer?.Detach(Editor);
        _squiggleRenderer = new RedSquiggleRenderer(Editor.Document, startLine, endLine);
        _squiggleRenderer.Attach(Editor);
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>()
            ?? throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        var tempFilePath = TransformFileEncoding(file.Path.LocalPath);

        try
        {
            var content = await File.ReadAllTextAsync(tempFilePath, Encoding.UTF8);
            SourceDocument = new TextDocument(content);
            if (Editor is not null)
                Editor.Document = SourceDocument;

            Title = file.Name; // Update the window title to the loaded file name

            TryBuildCondiguration(tempFilePath);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }

    private void TryBuildCondiguration(string filePath)
    {
        try
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddIniFile(filePath)
                .Build();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading INI file: {ex.InnerException?.Message}");
            Debug.WriteLine($"Document line count: {SourceDocument.LineCount}");
            // Try to extract the error line number from the exception message, fallback to a default if not found
            const int DefaultErrorLine = 1;
            int errorLine = DefaultErrorLine;

            var message = ex.InnerException?.Message ?? ex.Message;

            // Try to extract section and key from message like "A duplicate key 'parametr3:Offset' was found."
            var keyMatch = System.Text.RegularExpressions.Regex.Match(message, @"'([^:']+):([^']+)'\s*was found");
            if (keyMatch.Success)
            {
                var section = keyMatch.Groups[1].Value;
                var key = keyMatch.Groups[2].Value;
                var lines = SourceDocument.Text.Split('\n');
                int sectionLine = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                    {
                        sectionLine = i;
                        break;
                    }
                }
                if (sectionLine != -1)
                {
                    // Search for key after section
                    for (int i = sectionLine + 1; i < lines.Length; i++)
                    {
                        var line = lines[i].TrimStart();
                        if (line.StartsWith("[")) break; // next section
                        if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) || line.StartsWith(key + " ", StringComparison.OrdinalIgnoreCase) || line.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            errorLine = i + 1; // 1-based
                            break;
                        }
                    }
                }
            }
            else
            {
                // Fallback: try to extract line number as before
                var lineMatch = System.Text.RegularExpressions.Regex.Match(message, @"Line\\s+(\\d+)");
                if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out int parsedLine))
                {
                    errorLine = parsedLine;
                }
            }

            Debug.WriteLine($"Highlighting document line: {errorLine}");
            _startLine = errorLine;
            _endLine = errorLine;
            UpdateSquiggle(_startLine, _endLine);
        }
    }

    private string TransformFileEncoding(string file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var fileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, fileContent, Encoding.UTF8);
        return tempFilePath;
    }

    [RelayCommand]
    private void CopyMouse(TextArea textArea) => ApplicationCommands.Copy.Execute(null, textArea);
    [RelayCommand]
    private void CutMouse(TextArea textArea) => ApplicationCommands.Cut.Execute(null, textArea);
    [RelayCommand]
    private void PasteMouse(TextArea textArea) => ApplicationCommands.Paste.Execute(null, textArea);
    [RelayCommand]
    private void SelectAllMouse(TextArea textArea) => ApplicationCommands.SelectAll.Execute(null, textArea);
    [RelayCommand]
    private void UndoMouse(TextArea textArea) => ApplicationCommands.Undo.Execute(null, textArea);
    [RelayCommand]
    private void RedoMouse(TextArea textArea) => ApplicationCommands.Redo.Execute(null, textArea);
}
