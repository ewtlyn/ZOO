using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;

namespace woww;

public partial class Catalog : Window
{
    private List<DisplayAnimal> _animals = new();

    public Catalog()
    {
        InitializeComponent();
        LoadAnimals();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void BackButton_Click(object sender, RoutedEventArgs e) => this.Close();

    private async void LoadAnimals()
    {
        try
        {
            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT a.""Animal_ID"", a.""Name"", a.""Age"", t.""Type"", v.""View"",
                       g.""Gender"", vo.""Number"", a.""IsHungry"", a.""Photo""
                FROM ""Animals"" a
                JOIN ""AnimalType"" t ON a.""AnimalType_ID"" = t.""AnimalType_ID""
                JOIN ""AnimalView"" v ON a.""AnimalView_ID"" = v.""AnimalView_ID""
                JOIN ""Gender"" g ON a.""Gender_ID"" = g.""Gender_ID""
                JOIN ""Voiler"" vo ON a.""Volier_ID"" = vo.""Voiler_ID"";", conn);

            var reader = await cmd.ExecuteReaderAsync();
            _animals.Clear();

            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var age = reader.GetInt32(2);
                var type = reader.GetString(3);
                var view = reader.GetString(4);
                var gender = reader.GetString(5);
                var voiler = reader.GetString(6);
                var isHungry = reader.GetBoolean(7);
                var photoPath = reader.IsDBNull(8) ? null : reader.GetString(8);

                _animals.Add(new DisplayAnimal
                {
                    Id = id,
                    Name = name,
                    Age = age,
                    Type = type,
                    View = view,
                    Gender = gender,
                    Voiler = voiler,
                    IsHungry = isHungry,
                    PhotoPath = photoPath
                });
            }

            DisplayAnimals(_animals);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки: {ex.Message}");
        }
    }

    private void DisplayAnimals(List<DisplayAnimal> animals)
    {
        var panel = this.FindControl<StackPanel>("AnimalsPanel");
        panel.Children.Clear();

        foreach (var animal in animals)
        {
            var image = new Image
            {
                Width = 80,
                Height = 80,
                Margin = new Thickness(0, 0, 10, 0),
                Source = File.Exists(animal.PhotoPath)
                    ? new Bitmap(File.OpenRead(animal.PhotoPath))
                    : null
            };

            var description = new TextBlock
            {
                Text = $"{animal.Name}, {animal.View}, {animal.Type}, {animal.Gender}, {animal.Age} лет, Вольер {animal.Voiler}\nСтатус: {(animal.IsHungry ? "Голодный" : "Сытый")}",
                Width = 500,
                Foreground = Brushes.Black
            };

            var feedButton = new Button
            {
                Content = "Покормить",
                Background = animal.IsHungry ? Brushes.Orange : Brushes.Gray,
                IsEnabled = animal.IsHungry
            };
            feedButton.Click += async (_, _) =>
            {
                using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
                await conn.OpenAsync();

                var updateCmd = new NpgsqlCommand(@"UPDATE ""Animals"" SET ""IsHungry"" = false WHERE ""Animal_ID"" = @id", conn);
                updateCmd.Parameters.AddWithValue("id", animal.Id);
                await updateCmd.ExecuteNonQueryAsync();

                Console.WriteLine($"{animal.Name} покормлен");
                LoadAnimals();
            };

            var horizontal = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            horizontal.Children.Add(image);
            horizontal.Children.Add(description);
            horizontal.Children.Add(feedButton);

            panel.Children.Add(horizontal);
        }
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
        if (view != "Не сортировать") sorted = sorted.Where(a => a.View == view);
        if (type != "Не сортировать") sorted = sorted.Where(a => a.Type == type);
        if (gender != "Не сортировать") sorted = sorted.Where(a => a.Gender == gender);
        sorted = age switch
        {
            "По возрасту (↑)" => sorted.OrderBy(a => a.Age),
            "По возрасту (↓)" => sorted.OrderByDescending(a => a.Age),
            _ => sorted
        };

        DisplayAnimals(sorted.ToList());
    }
}

public class DisplayAnimal
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Type { get; set; } = "";
    public string View { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Voiler { get; set; } = "";
    public bool IsHungry { get; set; }
    public string? PhotoPath { get; set; }
}
