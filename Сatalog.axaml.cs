using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace woww
{
    public partial class Catalog : Window
    {
        private List<(string Name, int Age, string Type, string View, string Gender, string Voiler, bool IsHungry)> _animals;

        public Catalog()
        {
            InitializeComponent();
            LoadAnimals();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

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

                var cmd = new NpgsqlCommand(
                    @"SELECT a.""Name"", a.""Age"", t.""Type"", v.""View"", g.""Gender"", vo.""Number"", a.""IsHungry""
                      FROM ""Animals"" a
                      JOIN ""AnimalType"" t ON a.""AnimalType_ID"" = t.""AnimalType_ID""
                      JOIN ""AnimalView"" v ON a.""AnimalView_ID"" = v.""AnimalView_ID""
                      JOIN ""Gender"" g ON a.""Gender_ID"" = g.""Gender_ID""
                      JOIN ""Voiler"" vo ON a.""Volier_ID"" = vo.""Voiler_ID""",
                    conn);

                var reader = await cmd.ExecuteReaderAsync();
                _animals = new List<(string, int, string, string, string, string, bool)>();

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
                Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void DisplayAnimals(List<(string Name, int Age, string Type, string View, string Gender, string Voiler, bool IsHungry)> items)
        {
            var animalsBox = this.FindControl<ListBox>("AnimalsBox");
            animalsBox.ItemsSource = items.Select(item =>
                $"{item.Name}, {item.View}, {item.Type}, {item.Gender}, {item.Age} лет, Вольер: {item.Voiler}, Статус: {(item.IsHungry ? "Голодный" : "Сытый")}");
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            var search = this.FindControl<TextBox>("SearchBox").Text?.Trim().ToLower() ?? "";
            var filtered = _animals.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Type.ToLower().Contains(search) ||
                item.View.ToLower().Contains(search)).ToList();

            DisplayAnimals(filtered);
        }
        

        public void ApplySort(string view, string age, string type, string gender)
        {
            try
            {
                using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123;Database=postgres;Search Path=zoo");
                conn.Open();

                string query = @"
                    SELECT a.""Name"", a.""Age"", t.""Type"", v.""View"", g.""Gender"", vo.""Number"", a.""IsHungry""
                    FROM ""Animals"" a
                    JOIN ""AnimalType"" t ON a.""AnimalType_ID"" = t.""AnimalType_ID""
                    JOIN ""AnimalView"" v ON a.""AnimalView_ID"" = v.""AnimalView_ID""
                    JOIN ""Gender"" g ON a.""Gender_ID"" = g.""Gender_ID""
                    JOIN ""Voiler"" vo ON a.""Volier_ID"" = vo.""Volier_ID""";

                var whereClauses = new List<string>();
                if (view != "Не сортировать") whereClauses.Add($"v.\"View\" = '{view}'");
                if (type != "Не сортировать") whereClauses.Add($"t.\"Type\" = '{type}'");
                if (gender != "Не сортировать") whereClauses.Add($"g.\"Gender\" = '{gender}'");

                string orderBy = age switch
                {
                    "По возрастанию" => "ORDER BY a.\"Age\" ASC",
                    "По убыванию" => "ORDER BY a.\"Age\" DESC",
                    _ => "ORDER BY a.\"Name\" ASC"
                };

                if (whereClauses.Any())
                    query += " WHERE " + string.Join(" AND ", whereClauses);
                query += " " + orderBy;

                var cmd = new NpgsqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                var sorted = new List<(string, int, string, string, string, string, bool)>();
                while (reader.Read())
                {
                    sorted.Add((
                        reader.GetString(0),
                        reader.GetInt32(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetBoolean(6)
                    ));
                }

                DisplayAnimals(sorted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сортировке: {ex.Message}");
            }
        }
    }
}
