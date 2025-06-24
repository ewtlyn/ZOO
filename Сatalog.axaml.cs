using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace woww;

public partial class Catalog : Window
{
    private List<(string Name, int Age, string Type, string View, string Gender, string Voiler, bool IsHungry)> _animals = new();

    public Catalog()
    {
        InitializeComponent();
        LoadAnimals();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        var main = new AddAnimals();
        main.Show();
        this.Close();
    }

    private async void LoadAnimals()
    {
        try
        {
            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT a.""Name"", a.""Age"", t.""Type"", v.""View"", g.""Gender"", vo.""Number"", a.""IsHungry""
                FROM ""Animals"" a
                JOIN ""AnimalType"" t ON a.""AnimalType_ID"" = t.""AnimalType_ID""
                JOIN ""AnimalView"" v ON a.""AnimalView_ID"" = v.""AnimalView_ID""
                JOIN ""Gender"" g ON a.""Gender_ID"" = g.""Gender_ID""
                JOIN ""Voiler"" vo ON a.""Volier_ID"" = vo.""Voiler_ID"";", conn);

            var reader = await cmd.ExecuteReaderAsync();

            _animals.Clear();
            while (await reader.ReadAsync())
            {
                _animals.Add((
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetBoolean(6)
                ));
            }

            DisplayAnimals(_animals);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки: {ex.Message}");
        }
    }

    private void DisplayAnimals(List<(string Name, int Age, string Type, string View, string Gender, string Voiler, bool IsHungry)> list)
    {
        var box = this.FindControl<ListBox>("AnimalsBox");
        box.ItemsSource = list.Select(item =>
            $"{item.Name}, {item.View}, {item.Type}, {item.Gender}, {item.Age} лет, Вольер: {item.Voiler}, Статус: {(item.IsHungry ? "Голодный" : "Сытый")}");
    }

    private void SearchButton_Click(object? sender, RoutedEventArgs e)
    {
        var search = this.FindControl<TextBox>("SearchBox").Text?.Trim().ToLower() ?? "";
        var filtered = _animals.Where(a =>
            a.Name.ToLower().Contains(search) ||
            a.View.ToLower().Contains(search) ||
            a.Type.ToLower().Contains(search)).ToList();

        DisplayAnimals(filtered);
    }

    private void ParametrButton_Click(object? sender, RoutedEventArgs e)
    {
        var sortWindow = new SortWindow(this);
        sortWindow.ShowDialog(this);
    }

    public void ApplySort(string view, string age, string type, string gender)
    {
        var sorted = _animals.AsEnumerable();

        if (view != "Не сортировать")
            sorted = sorted.Where(a => a.View == view);
        if (type != "Не сортировать")
            sorted = sorted.Where(a => a.Type == type);
        if (gender != "Не сортировать")
            sorted = sorted.Where(a => a.Gender == gender);

        sorted = age switch
        {
            "По возрасту (↑)" => sorted.OrderBy(a => a.Age),
            "По возрасту (↓)" => sorted.OrderByDescending(a => a.Age),
            _ => sorted
        };

        DisplayAnimals(sorted.ToList());
    }
}
