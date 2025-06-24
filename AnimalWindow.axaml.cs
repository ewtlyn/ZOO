using Avalonia.Controls;
using Avalonia.Interactivity;

namespace woww
{
    public partial class AnimalWindow : Window
    {
        public AnimalWindow()
        {
            InitializeComponent();
            
        }
        
        private void OpenAddAnimals_Click(object sender, RoutedEventArgs e)
        {
            AddAnimals wnd = new AddAnimals();
            wnd.Show();
        }
        
        private void OpenEditAnimals_Click(object sender, RoutedEventArgs e)
        {
            EditAnimals wnd = new EditAnimals();
            wnd.Show();
        }
        
        private void ShowAnimals_Click(object sender, RoutedEventArgs e)
        {
            Catalog wnd = new Catalog();
            wnd.Show();
        }
    }
}