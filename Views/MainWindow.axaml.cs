using Avalonia.Controls;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace IniConfigTroubleshooting.Views;

public partial class MainWindow : Window
{
    private readonly RegistryOptions _registryOptions;
    private readonly string? _iniScopeName;
    private const string Extension = ".ini";

    public MainWindow()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(ThemeName.Light);
        _iniScopeName = _registryOptions.GetScopeByExtension(Extension);
        InitTextMate();
    }
    private void InitTextMate()
    {
        if (_iniScopeName is null)
            return;

        AvalonEditor.InstallTextMate(_registryOptions)
                    .SetGrammar(_iniScopeName);
    }
    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        // This is where you can handle text changes in the editor`
    }


}
