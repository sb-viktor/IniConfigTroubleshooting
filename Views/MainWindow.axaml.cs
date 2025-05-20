using Avalonia.Controls;
using AvaloniaEdit;
using IniConfigTroubleshooting.ViewModels;

namespace IniConfigTroubleshooting.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        var editor = this.FindControl<TextEditor>("Editor");
        if (editor is not null)
            viewModel.AttachEditor(editor);
    }
}
