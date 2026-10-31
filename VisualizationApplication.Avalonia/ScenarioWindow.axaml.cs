using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CVRPTW;
using VisualizationApplication.Avalonia.Map;
using VisualizationApplication.Avalonia.Tools;

namespace VisualizationApplication.Avalonia;

public partial class ScenarioWindow : Window
{
    private static readonly FilePickerFileType ScenarioFileType = new("Сценарій (*.json)") { Patterns = ["*.json"] };

    private readonly ObservableCollection<ScenarioClient> _clients = new();
    private readonly ObservableCollection<ScenarioCar> _cars = new();
    private readonly ScenarioMap _map;
    private bool _suspendRefresh;

    public ScenarioWindow()
    {
        InitializeComponent();

        ClientsGrid.ItemsSource = _clients;
        CarsGrid.ItemsSource = _cars;

        _map = new ScenarioMap(MapPlot) { Clients = _clients };
        _map.Clicked += MapClickedHandler;
        _map.TooManyRoadsChanged += tooMany => RoadsHintBorder.IsVisible = tooMany;
        _map.SetRoads(RoadNetworkLoader.Current, RoadNetworkLoader.CurrentIndex);

        _clients.CollectionChanged += (_, _) => RefreshMap();
        ClientsGrid.CellEditEnded += (_, _) => RefreshMap();
        ClientsGrid.SelectionChanged += (_, _) =>
        {
            _map.SelectedClient = ClientsGrid.SelectedItem as ScenarioClient;
            RefreshMap();
        };
        DepotLatitudeTextBox.TextChanged += (_, _) => RefreshMap();
        DepotLongitudeTextBox.TextChanged += (_, _) => RefreshMap();
        SnapThresholdTextBox.TextChanged += (_, _) => RefreshMap();

        LoadOsmButton.Click += async (_, _) => await RoadNetworkLoader.LoadAsync(this);
        ZoomButton.Click += (_, _) => _map.ZoomToAll();

        RoadNetworkLoader.Changed += RoadsChangedHandler;
        Closed += (_, _) =>
        {
            RoadNetworkLoader.Changed -= RoadsChangedHandler;
            _map.Dispose();
        };

        NewButton.Click += (_, _) => ShowScenario(new Scenario(), "Створено порожній сценарій.");
        LoadButton.Click += LoadButtonClickHandler;
        SaveButton.Click += SaveButtonClickHandler;
        AddClientButton.Click += (_, _) => _clients.Add(new ScenarioClient { Id = NextId(_clients.Select(client => client.Id)) });
        AddCarButton.Click += (_, _) => _cars.Add(new ScenarioCar { Id = NextId(_cars.Select(car => car.Id)) });
        RemoveClientButton.Click += (_, _) => Remove(ClientsGrid, _clients);
        RemoveCarButton.Click += (_, _) => Remove(CarsGrid, _cars);

        ShowScenario(new Scenario(), string.Empty);
    }

    private static int NextId(System.Collections.Generic.IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;

    private static void Remove<T>(DataGrid grid, ObservableCollection<T> items)
    {
        if (grid.SelectedItem is T selected) items.Remove(selected);
    }

    private void ShowScenario(Scenario scenario, string status)
    {
        _suspendRefresh = true;

        DepotLatitudeTextBox.Text = scenario.Depot?.Latitude.ToString(CultureInfo.InvariantCulture);
        DepotLongitudeTextBox.Text = scenario.Depot?.Longitude.ToString(CultureInfo.InvariantCulture);
        AverageSpeedTextBox.Text = scenario.AverageSpeedKmh.ToString(CultureInfo.InvariantCulture);
        SnapThresholdTextBox.Text = scenario.SnapThresholdMeters.ToString(CultureInfo.InvariantCulture);
        AlternativeGroupsTextBox.Text = string.Join("; ", scenario.AlternativeGroups.Select(group => string.Join(",", group)));

        _clients.Clear();
        foreach (var client in scenario.Clients) _clients.Add(client);

        _cars.Clear();
        foreach (var car in scenario.Cars) _cars.Add(car);

        StatusTextBlock.Text = status;

        _suspendRefresh = false;
        RefreshMap();
        _map.ZoomToAll();
    }

    private void RefreshMap()
    {
        if (_suspendRefresh) return;

        _map.Depot = ReadDepot();
        _map.SnapThresholdMeters = TryParseDouble(SnapThresholdTextBox.Text, out var threshold) && threshold >= 0
            ? threshold
            : Constants.DefaultSnapThresholdMeters;
        _map.Refresh();

        SpecialPointsTextBlock.Text = _map.SpecialPointsCount > 0 ? $"Особливих точок: {_map.SpecialPointsCount}" : string.Empty;
    }

    private ScenarioDepot? ReadDepot() =>
        TryParseDouble(DepotLatitudeTextBox.Text, out var latitude) && TryParseDouble(DepotLongitudeTextBox.Text, out var longitude)
            ? new ScenarioDepot { Latitude = latitude, Longitude = longitude }
            : null;

    private void RoadsChangedHandler()
    {
        _map.SetRoads(RoadNetworkLoader.Current, RoadNetworkLoader.CurrentIndex);
        RefreshMap();
        _map.ZoomToAll();
    }

    private void MapClickedHandler(MapClick click)
    {
        var latitude = Math.Round(click.Latitude, 6);
        var longitude = Math.Round(click.Longitude, 6);

        if (DepotRadio.IsChecked == true)
        {
            DepotLatitudeTextBox.Text = latitude.ToString(CultureInfo.InvariantCulture);
            DepotLongitudeTextBox.Text = longitude.ToString(CultureInfo.InvariantCulture);
        }
        else if (ClientRadio.IsChecked == true)
        {
            var client = new ScenarioClient { Id = NextId(_clients.Select(existing => existing.Id)), Latitude = latitude, Longitude = longitude };
            _clients.Add(client);
            SelectClient(client);
        }
        else if (click.NearbyClient != null)
        {
            SelectClient(click.NearbyClient);
        }
    }

    private void SelectClient(ScenarioClient client)
    {
        MainTabs.SelectedIndex = 1;
        ClientsGrid.SelectedItem = client;
        ClientsGrid.ScrollIntoView(client, null);
    }

    private async void LoadButtonClickHandler(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Оберіть файл сценарію",
            AllowMultiple = false,
            FileTypeFilter = [ScenarioFileType]
        });

        if (files.Count == 0) return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            var scenario = ScenarioSerializer.Load(stream);

            var errors = scenario.Validate();
            ShowScenario(scenario, errors.Count == 0 ? $"Завантажено: {files[0].Name}" : "Завантажено з помилками:\n" + string.Join("\n", errors));
        }
        catch (Exception exception) when (exception is IOException or System.Text.Json.JsonException or InvalidDataException)
        {
            await MessageBox.Show(this, $"Не вдалося завантажити файл: {exception.Message}");
        }
    }

    private async void SaveButtonClickHandler(object? sender, RoutedEventArgs e)
    {
        if (!TryReadScenario(out var scenario, out var error))
        {
            await MessageBox.Show(this, error);
            return;
        }

        var errors = scenario.Validate();
        if (errors.Count > 0)
        {
            await MessageBox.Show(this, "Сценарій не збережено:\n" + string.Join("\n", errors));
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Зберегти сценарій",
            DefaultExtension = "json",
            SuggestedFileName = "scenario.json",
            FileTypeChoices = [ScenarioFileType]
        });

        if (file == null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            ScenarioSerializer.Save(scenario, stream);
            StatusTextBlock.Text = $"Збережено: {file.Name}";
        }
        catch (IOException exception)
        {
            await MessageBox.Show(this, $"Не вдалося зберегти файл: {exception.Message}");
        }
    }

    private bool TryReadScenario(out Scenario scenario, out string error)
    {
        scenario = new Scenario();
        error = string.Empty;

        if (!TryParseDouble(AverageSpeedTextBox.Text, out var speed))
        {
            error = "Середня швидкість: некоректне число.";
            return false;
        }

        scenario.AverageSpeedKmh = speed;

        if (!TryParseDouble(SnapThresholdTextBox.Text, out var threshold))
        {
            error = "Поріг відстані до дороги: некоректне число.";
            return false;
        }

        scenario.SnapThresholdMeters = threshold;

        var hasLatitude = !string.IsNullOrWhiteSpace(DepotLatitudeTextBox.Text);
        var hasLongitude = !string.IsNullOrWhiteSpace(DepotLongitudeTextBox.Text);

        if (hasLatitude || hasLongitude)
        {
            if (!TryParseDouble(DepotLatitudeTextBox.Text, out var latitude) ||
                !TryParseDouble(DepotLongitudeTextBox.Text, out var longitude))
            {
                error = "Склад: задайте обидві координати числами.";
                return false;
            }

            scenario.Depot = new ScenarioDepot { Latitude = latitude, Longitude = longitude };
        }

        if (!TryParseGroups(AlternativeGroupsTextBox.Text, scenario))
        {
            error = "Групи альтернатив: очікується формат «1,2,3; 4,5».";
            return false;
        }

        scenario.Clients = _clients.ToList();
        scenario.Cars = _cars.ToList();

        return true;
    }

    private static bool TryParseDouble(string? text, out double value) =>
        double.TryParse(text?.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static bool TryParseGroups(string? text, Scenario scenario)
    {
        foreach (var groupText in (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var ids = groupText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!ids.All(id => int.TryParse(id, out _))) return false;

            scenario.AlternativeGroups.Add(ids.Select(int.Parse).ToList());
        }

        return true;
    }
}
