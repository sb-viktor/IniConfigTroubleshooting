using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace IniConfigTroubleshooting.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SetupTextMate();
    }

    private void SetupTextMate()
    {
        //First of all you need to have a reference for your TextEditor for it to be used inside AvaloniaEdit.TextMate project.
        var _textEditor = this.FindControl<TextEditor>("Editor");

        //Here we initialize RegistryOptions with the theme we want to use.
        var _registryOptions = new RegistryOptions(ThemeName.Light);

        //Initial setup of TextMate.
        var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all, you are ready to use AvaloniaEdit with syntax highlighting!
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByExtension(".ini"));
    }
}