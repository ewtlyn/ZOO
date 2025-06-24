using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Npgsql;
using System;
using System.IO;

namespace woww;

public partial class AddAnimals : Window
{
    private const string ConnStr = "Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo";
    private int _animalTypeId = 1;
    private int _animalViewId = 1;
    private string? _photoPath = null;

    public AddAnimals()
    {
        InitializeComponent();
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            using var conn = new NpgsqlConnection(ConnStr);
            await conn.OpenAsync();

            var name = NameTextBox.Text?.Trim() ?? "";
            var age = int.TryParse(AgeTextBox.Text, out var a) ? a : 0;
            var genderText = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Женский";
            var volierNumber = VoilerTextBox.Text?.Trim() ?? "1";
            var isHungry = EatingBox.SelectedIndex == 2;
            
            var getVoiler = new NpgsqlCommand(@"SELECT ""Voiler_ID"" FROM ""Voiler"" WHERE ""Number"" = @num", conn);
            getVoiler.Parameters.AddWithValue("num", volierNumber);
            var voilerId = await getVoiler.ExecuteScalarAsync();
            if (voilerId == null)
            {
                var insertVoiler = new NpgsqlCommand(@"INSERT INTO ""Voiler"" (""Number"") VALUES (@num) RETURNING ""Voiler_ID""", conn);
                insertVoiler.Parameters.AddWithValue("num", volierNumber);
                voilerId = await insertVoiler.ExecuteScalarAsync();
            }
            
            var getGender = new NpgsqlCommand(@"SELECT ""Gender_ID"" FROM ""Gender"" WHERE ""Gender"" = @gender", conn);
            getGender.Parameters.AddWithValue("gender", genderText);
            var genderId = await getGender.ExecuteScalarAsync();
            if (genderId == null)
            {
                var insertGender = new NpgsqlCommand(@"INSERT INTO ""Gender"" (""Gender"") VALUES (@gender) RETURNING ""Gender_ID""", conn);
                insertGender.Parameters.AddWithValue("gender", genderText);
                genderId = await insertGender.ExecuteScalarAsync();
            }
            
            var cmd = new NpgsqlCommand(@"
                INSERT INTO ""Animals"" 
                (""Name"", ""Age"", ""Photo"", ""AnimalType_ID"", ""AnimalView_ID"", ""Volier_ID"", ""Gender_ID"", ""IsHungry"") 
                VALUES 
                (@name, @age, @photo, @type, @view, @voiler, @gender, @hungry);", conn);

            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("age", age);
            cmd.Parameters.AddWithValue("photo", _photoPath ?? "no_photo.jpg");
            cmd.Parameters.AddWithValue("type", _animalTypeId);
            cmd.Parameters.AddWithValue("view", _animalViewId);
            cmd.Parameters.AddWithValue("voiler", voilerId!);
            cmd.Parameters.AddWithValue("gender", genderId!);
            cmd.Parameters.AddWithValue("hungry", isHungry);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Животное добавлено");
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
        }
    }

    private async void OpenTypeAnimalWindow_Click(object? sender, RoutedEventArgs e)
    {
        var win = new AddTypeAnimals();
        await win.ShowDialog(this);
        if (win.Tag is ValueTuple<int, int> ids)
        {
            _animalViewId = ids.Item1;
            _animalTypeId = ids.Item2;
            SelectedViewTextBlock.Text = $"ID вида: {_animalViewId}, ID типа: {_animalTypeId}";
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
            PhotoPathTextBlock.Text = _photoPath;
            if (File.Exists(_photoPath))
            {
                using var stream = File.OpenRead(_photoPath);
                AnimalPhoto.Source = new Bitmap(stream);
            }
        }
    }
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
