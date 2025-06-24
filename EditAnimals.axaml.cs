using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace woww;

public partial class EditAnimals : Window
{
    private record AnimalInfo(int Id, string Name);
    private readonly List<AnimalInfo> _animals = new();
    private int _selectedId = -1;
    private string? _photoPath = null;

    public EditAnimals()
    {
        InitializeComponent();
        LoadAnimalList();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void LoadAnimalList()
    {
        try
        {
            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"SELECT ""Animal_ID"", ""Name"" FROM ""Animals""", conn);
            var reader = await cmd.ExecuteReaderAsync();
            var combo = this.FindControl<ComboBox>("AnimalSelect");

            combo.Items.Clear();
            _animals.Clear();

            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                _animals.Add(new AnimalInfo(id, name));
                combo.Items.Add(new ComboBoxItem { Content = name });
            }

            combo.SelectionChanged += AnimalSelected;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка загрузки животных: " + ex.Message);
        }
    }

    private async void AnimalSelected(object? sender, SelectionChangedEventArgs e)
    {
        var selectedIndex = (sender as ComboBox)?.SelectedIndex ?? -1;
        if (selectedIndex < 0 || selectedIndex >= _animals.Count)
            return;

        _selectedId = _animals[selectedIndex].Id;

        try
        {
            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT ""Name"", ""Age"", ""Photo"", ""IsHungry"", ""EatingInterval"", g.""Gender""
                FROM ""Animals"" a
                JOIN ""Gender"" g ON a.""Gender_ID"" = g.""Gender_ID""
                WHERE a.""Animal_ID"" = @id", conn);

            cmd.Parameters.AddWithValue("id", _selectedId);
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                this.FindControl<TextBox>("NameBox").Text = reader.GetString(0);
                this.FindControl<TextBox>("AgeBox").Text = reader.GetInt32(1).ToString();
                this.FindControl<TextBox>("IntervalBox").Text = reader.GetTimeSpan(4).ToString(@"hh\:mm");
                _photoPath = reader.GetString(2);
                this.FindControl<TextBlock>("PhotoPathTextBlock").Text = _photoPath;

                if (File.Exists(_photoPath))
                    this.FindControl<Image>("AnimalPhoto").Source = new Bitmap(File.OpenRead(_photoPath));

                this.FindControl<ComboBox>("StatusBox").SelectedIndex = reader.GetBoolean(3) ? 1 : 2;
                var genderText = reader.GetString(5);
                this.FindControl<ComboBox>("GenderBox").SelectedIndex = genderText == "Мужской" ? 1 : 2;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка загрузки животного: " + ex.Message);
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedId == -1) return;

        try
        {
            var name = this.FindControl<TextBox>("NameBox").Text ?? "";
            var age = int.TryParse(this.FindControl<TextBox>("AgeBox").Text, out var a) ? a : 0;
            var interval = TimeSpan.TryParse(this.FindControl<TextBox>("IntervalBox").Text, out var ts) ? ts : TimeSpan.FromHours(6);
            var gender = (this.FindControl<ComboBox>("GenderBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Женский";
            var isHungry = (this.FindControl<ComboBox>("StatusBox").SelectedItem as ComboBoxItem)?.Content?.ToString() == "Голодный";

            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
            await conn.OpenAsync();

            var getGender = new NpgsqlCommand(@"SELECT ""Gender_ID"" FROM ""Gender"" WHERE ""Gender"" = @gender", conn);
            getGender.Parameters.AddWithValue("gender", gender);
            var genderId = await getGender.ExecuteScalarAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE ""Animals"" 
                SET ""Name"" = @name, 
                    ""Age"" = @age, 
                    ""Photo"" = @photo, 
                    ""Gender_ID"" = @genderId,
                    ""IsHungry"" = @hungry,
                    ""EatingInterval"" = @interval
                WHERE ""Animal_ID"" = @id", conn);

            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("age", age);
            cmd.Parameters.AddWithValue("photo", _photoPath ?? "no_photo.jpg");
            cmd.Parameters.AddWithValue("genderId", genderId!);
            cmd.Parameters.AddWithValue("hungry", isHungry);
            cmd.Parameters.AddWithValue("interval", interval);
            cmd.Parameters.AddWithValue("id", _selectedId);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Животное обновлено");
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при сохранении: {ex.Message}");
        }
    }

    private async void ChoosePic_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото",
            AllowMultiple = false
        };
        var result = await dialog.ShowAsync(this);
        if (result?.Length > 0)
        {
            _photoPath = result[0];
            this.FindControl<TextBlock>("PhotoPathTextBlock").Text = _photoPath;

            if (File.Exists(_photoPath))
            {
                using var stream = File.OpenRead(_photoPath);
                this.FindControl<Image>("AnimalPhoto").Source = new Bitmap(stream);
            }
        }
    }
}
