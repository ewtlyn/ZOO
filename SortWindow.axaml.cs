using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Npgsql;

namespace woww;

public partial class SortWindow : Window
{
    private readonly Catalog? _parent;

    public SortWindow()
    {
        InitializeComponent();
    }

    public SortWindow(Catalog parent) : this()
    {
        _parent = parent;
        LoadViews(); 
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void LoadViews()
    {
        try
        {
            var viewBox = this.FindControl<ComboBox>("ViewBox");
            viewBox.Items.Clear();

            viewBox.Items.Add(new ComboBoxItem { Content = "Не сортировать" });

            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"SELECT ""View"" FROM ""AnimalView"";", conn);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var viewName = reader.GetString(0);
                viewBox.Items.Add(new ComboBoxItem { Content = viewName });
            }

            viewBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки видов: {ex.Message}");
        }
    }

    private void ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_parent == null)
        {
            Console.WriteLine("Родительское окно не задано");
            return;
        }

        var sortBy = (this.FindControl<ComboBox>("SortAgeBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var gender = (this.FindControl<ComboBox>("GenderBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var view = (this.FindControl<ComboBox>("ViewBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var type = (this.FindControl<ComboBox>("TypeBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";

        _parent.ApplySort(view, sortBy, type, gender);
        this.Close();
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e) => this.Close();
}
