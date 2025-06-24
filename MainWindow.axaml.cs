using Avalonia.Controls;
using Avalonia.Interactivity;

namespace woww
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenAnimalWindow_Click(object sender, RoutedEventArgs e)
        {
            AnimalWindow wnd = new AnimalWindow();
            wnd.Show();
        }
    }
}