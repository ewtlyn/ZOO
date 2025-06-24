using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_parent == null)
        {
            Console.WriteLine("родительское окно не задано");
            return;
        }

        var sortBy = (this.FindControl<ComboBox>("SortAgeBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var gender = (this.FindControl<ComboBox>("GenderBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var view = (this.FindControl<ComboBox>("ViewBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";
        var type = (this.FindControl<ComboBox>("TypeBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не сортировать";

        _parent.ApplySort(view, sortBy, type, gender);
        this.Close();
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}