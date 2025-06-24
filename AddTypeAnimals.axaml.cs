using Avalonia.Controls;
using Avalonia.Interactivity;
using Npgsql;
using System;

namespace woww;

public partial class AddTypeAnimals : Window
{
    private const string ConnStr = "Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres";

    public AddTypeAnimals()
    {
        InitializeComponent();
        LoadViewsAsync(); 
    }

    private async void LoadViewsAsync()
    {
        try
        {
            using var conn = new NpgsqlConnection(ConnStr);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"SELECT ""View"" FROM ""zoo"".""AnimalView"";", conn);
            var reader = await cmd.ExecuteReaderAsync();

            ViewComboBox.Items.Clear();

            while (await reader.ReadAsync())
            {
                var viewName = reader.GetString(0);
                ViewComboBox.Items.Add(new ComboBoxItem { Content = viewName });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки видов: {ex.Message}");
        }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        var newViewName = NewViewTextBox.Text?.Trim();
        var selectedViewItem = ViewComboBox.SelectedItem as ComboBoxItem;
        var selectedViewName = selectedViewItem?.Content?.ToString();

        var typeItem = NewTypeComboBox.SelectedItem as ComboBoxItem;
        var typeName = typeItem?.Content?.ToString();

        if (string.IsNullOrWhiteSpace(typeName))
        {
            Console.WriteLine("Выберите тип");
            return;
        }

        if (string.IsNullOrWhiteSpace(newViewName) && string.IsNullOrWhiteSpace(selectedViewName))
        {
            Console.WriteLine("Введите новый вид или выберите из списка");
            return;
        }

        try
        {
            using var conn = new NpgsqlConnection(ConnStr);
            await conn.OpenAsync();

            int viewId;

            if (!string.IsNullOrWhiteSpace(newViewName))
            {
                var insertView = new NpgsqlCommand(@"INSERT INTO ""zoo"".""AnimalView"" (""View"") 
                                                     VALUES (@view) RETURNING ""AnimalView_ID"";", conn);
                insertView.Parameters.AddWithValue("view", newViewName);
                viewId = (int)(await insertView.ExecuteScalarAsync());
            }
            else
            {
                var findView = new NpgsqlCommand(@"SELECT ""AnimalView_ID"" FROM ""zoo"".""AnimalView"" 
                                                   WHERE ""View"" = @view;", conn);
                findView.Parameters.AddWithValue("view", selectedViewName);
                var result = await findView.ExecuteScalarAsync();

                if (result == null)
                {
                    Console.WriteLine("Не удалось найти выбранный вид");
                    return;
                }

                viewId = (int)result;
            }

            var insertType = new NpgsqlCommand(@"INSERT INTO ""zoo"".""AnimalType"" (""Type"") 
                                     VALUES (@type) 
                                     RETURNING ""AnimalType_ID"";", conn);
            insertType.Parameters.AddWithValue("type", typeName);
            var typeId = (int)(await insertType.ExecuteScalarAsync());

            Console.WriteLine($"Создан вид ID: {viewId}, тип ID: {typeId}");
            this.Tag = (viewId, typeId);
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}
