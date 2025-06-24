using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace woww;

public partial class EditAnimals : Window
{
    public EditAnimals()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}