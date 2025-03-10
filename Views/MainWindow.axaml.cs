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

        TextEditor? TextEditor = this.FindControl<TextEditor>("Editor");

        var RegistryOptions = new RegistryOptions(ThemeName.Light);

        var TextMateInstallation = TextEditor.InstallTextMate(RegistryOptions);

        Language IniFileLanguage = RegistryOptions.GetLanguageByExtension(".ini");

        TextMateInstallation.SetGrammar(RegistryOptions.GetScopeByLanguageId(IniFileLanguage.Id));

    }
}