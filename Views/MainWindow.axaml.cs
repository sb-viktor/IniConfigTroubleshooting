using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using IniConfigTroubleshooting.Resources;
using System.Resources;
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

        //string scopeName = RegistryOptions.GetScopeByLanguageId(IniFileLanguage.Id);

        //TextEditor.Document = new TextDocument(ResourceLoader.LoadSampleFile(scopeName));

        TextMateInstallation.SetGrammar(RegistryOptions.GetScopeByLanguageId(IniFileLanguage.Id));

    }
}