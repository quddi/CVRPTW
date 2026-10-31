using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Point = Avalonia.Point;
using Avalonia.Input;
using Avalonia.Interactivity;
using CVRPTW;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.TickGenerators;

namespace VisualizationApplication.Avalonia.Map;

public record MapClick(double Latitude, double Longitude, ScenarioClient? NearbyClient);

/// <summary>Карта сценарію: дороги, склад, клієнти й особливі точки (надто далекі від дороги).</summary>
public class ScenarioMap : IDisposable
{
    // Зсув вказівника, до якого натискання ще вважається кліком, а не перетягуванням.
    private const double ClickTolerance = 5;
    private const double PickRadius = 12;
    private const int MaxLabeledClients = 300;

    private static readonly Color DepotColor = Colors.Crimson;
    private static readonly Color ClientColor = Colors.RoyalBlue;
    private static readonly Color SpecialColor = Colors.DarkOrange;

    private readonly AvaPlot _plotControl;
    private readonly RoadsPlottable _roads = new();
    private readonly List<IPlottable> _dynamic = new();
    private Point? _pressedAt;
    private RoadSpatialIndex? _index;

    public ScenarioMap(AvaPlot plotControl)
    {
        _plotControl = plotControl;

        var plot = plotControl.Plot;
        plot.Add.Plottable(_roads);
        plot.HideGrid();
        plot.Axes.SquareUnits();
        plot.Axes.Left.TickGenerator = new EmptyTickGenerator();
        plot.Axes.Bottom.TickGenerator = new EmptyTickGenerator();

        _roads.TooManyEdgesChanged += OnTooManyEdgesChanged;
        plotControl.AddHandler(InputElement.PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        plotControl.AddHandler(InputElement.PointerReleasedEvent, ReleasedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    public ScenarioDepot? Depot { get; set; }

    public IReadOnlyList<ScenarioClient> Clients { get; set; } = Array.Empty<ScenarioClient>();

    public ScenarioClient? SelectedClient { get; set; }

    public double SnapThresholdMeters { get; set; } = Constants.DefaultSnapThresholdMeters;

    // Скільки точок (склад і клієнти) лежать далі за поріг від найближчої дороги; оновлюється в Refresh.
    public int SpecialPointsCount { get; private set; }

    public event Action<MapClick>? Clicked;

    public event Action<bool>? TooManyRoadsChanged;

    public void SetRoads(RoadNetwork? network, RoadSpatialIndex? index)
    {
        _roads.Network = network;
        _roads.Index = index;
        _index = index;
    }

    public void Refresh()
    {
        var plot = _plotControl.Plot;

        foreach (var item in _dynamic) plot.Remove(item);
        _dynamic.Clear();
        SpecialPointsCount = 0;

        var labeled = Clients.Count <= MaxLabeledClients;

        foreach (var client in Clients)
        {
            AddMarker(client.Latitude, client.Longitude, MarkerShape.FilledCircle, 8, ClientColor);
            AddSnap(client.Latitude, client.Longitude);

            if (!labeled) continue;

            var (x, y) = MapProjection.ToMap(client.Latitude, client.Longitude);
            var label = plot.Add.Text(client.Id.ToString(), x, y);
            label.LabelFontSize = 12;
            label.LabelOffsetY = -12;
            _dynamic.Add(label);
        }

        if (Depot != null)
        {
            AddMarker(Depot.Latitude, Depot.Longitude, MarkerShape.FilledDiamond, 15, DepotColor);
            AddSnap(Depot.Latitude, Depot.Longitude);
        }

        if (SelectedClient != null)
        {
            var selected = AddMarker(SelectedClient.Latitude, SelectedClient.Longitude, MarkerShape.OpenCircle, 18, Colors.Gold);
            selected.LineWidth = 3;
        }

        _plotControl.Refresh();
    }

    public void ZoomToAll()
    {
        var points = Clients.Select(client => (client.Latitude, client.Longitude)).ToList();
        if (Depot != null) points.Add((Depot.Latitude, Depot.Longitude));

        if (points.Count == 0 && _roads.Network != null)
        {
            var (minLat, minLon, maxLat, maxLon) = _roads.Network.GetBounds();
            points.Add((minLat, minLon));
            points.Add((maxLat, maxLon));
        }

        if (points.Count == 0) return;

        var mapPoints = points.Select(point => MapProjection.ToMap(point.Latitude, point.Longitude)).ToList();
        var left = mapPoints.Min(point => point.X);
        var right = mapPoints.Max(point => point.X);
        var bottom = mapPoints.Min(point => point.Y);
        var top = mapPoints.Max(point => point.Y);

        // Мінімальний розмір області (≈1 км), щоб одна точка не збільшувалася до нескінченності.
        var padding = Math.Max(Math.Max(right - left, top - bottom) * 0.1, 500);

        left -= padding;
        right += padding;
        bottom -= padding;
        top += padding;

        // Правило квадратних одиниць зберігає ширину, тому підганяємо межі під пропорції вікна карти самі.
        var viewAspect = _plotControl.Bounds.Height > 0 ? _plotControl.Bounds.Width / _plotControl.Bounds.Height : 0;

        if (viewAspect > 0)
        {
            var width = right - left;
            var height = top - bottom;

            if (width / height < viewAspect)
            {
                var extra = (height * viewAspect - width) / 2;
                left -= extra;
                right += extra;
            }
            else
            {
                var extra = (width / viewAspect - height) / 2;
                bottom -= extra;
                top += extra;
            }
        }

        _plotControl.Plot.Axes.SetLimits(left, right, bottom, top);
        _plotControl.Refresh();
    }

    private ScottPlot.Plottables.Marker AddMarker(double latitude, double longitude, MarkerShape shape, float size, Color color)
    {
        var (x, y) = MapProjection.ToMap(latitude, longitude);
        var marker = _plotControl.Plot.Add.Marker(x, y, shape, size, color);
        _dynamic.Add(marker);

        return marker;
    }

    // Для точки, що далі за поріг від дороги, показуємо позицію на дорозі, яка йде в розрахунок.
    private void AddSnap(double latitude, double longitude)
    {
        if (_index == null || !_index.TryFindNearest(latitude, longitude, out var snap) || snap.DistanceMeters <= SnapThresholdMeters)
            return;

        SpecialPointsCount++;

        var (x, y) = MapProjection.ToMap(latitude, longitude);
        var (snapX, snapY) = MapProjection.ToMap(snap.Latitude, snap.Longitude);

        var line = _plotControl.Plot.Add.Line(x, y, snapX, snapY);
        line.LineColor = SpecialColor;
        line.LinePattern = LinePattern.Dashed;
        line.LineWidth = 1.5f;
        _dynamic.Add(line);

        AddMarker(snap.Latitude, snap.Longitude, MarkerShape.OpenCircle, 10, SpecialColor).LineWidth = 2;
    }

    private void OnTooManyEdgesChanged(bool tooMany) => TooManyRoadsChanged?.Invoke(tooMany);

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        _pressedAt = e.GetCurrentPoint(_plotControl).Properties.IsLeftButtonPressed ? e.GetPosition(_plotControl) : null;
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_pressedAt is not { } start || e.InitialPressMouseButton != MouseButton.Left) return;

        _pressedAt = null;

        var position = e.GetPosition(_plotControl);

        if (Math.Abs(position.X - start.X) + Math.Abs(position.Y - start.Y) > ClickTolerance) return;

        var scale = _plotControl.DisplayScale;
        var coordinates = _plotControl.Plot.GetCoordinates(new Pixel((float)(position.X * scale), (float)(position.Y * scale)));
        var (latitude, longitude) = MapProjection.ToLatLon(coordinates.X, coordinates.Y);

        Clicked?.Invoke(new MapClick(latitude, longitude, FindNearbyClient(position, scale)));
    }

    private ScenarioClient? FindNearbyClient(Point position, double scale)
    {
        ScenarioClient? nearest = null;
        var nearestDistance = PickRadius;

        foreach (var client in Clients)
        {
            var (x, y) = MapProjection.ToMap(client.Latitude, client.Longitude);
            var pixel = _plotControl.Plot.GetPixel(new ScottPlot.Coordinates(x, y));
            var distance = Math.Sqrt(Math.Pow(pixel.X / scale - position.X, 2) + Math.Pow(pixel.Y / scale - position.Y, 2));

            if (distance > nearestDistance) continue;

            nearestDistance = distance;
            nearest = client;
        }

        return nearest;
    }

    public void Dispose()
    {
        _roads.TooManyEdgesChanged -= OnTooManyEdgesChanged;
        _plotControl.RemoveHandler(InputElement.PointerPressedEvent, PressedHandler);
        _plotControl.RemoveHandler(InputElement.PointerReleasedEvent, ReleasedHandler);
    }
}
