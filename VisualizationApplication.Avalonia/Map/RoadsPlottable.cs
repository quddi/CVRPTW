using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CVRPTW;
using ScottPlot;
using SkiaSharp;

namespace VisualizationApplication.Avalonia.Map;

/// <summary>Малює лише ті дороги, що потрапили у видиму область.</summary>
public class RoadsPlottable : IPlottable
{
    // Більше кандидатів не малюємо: користувач має збільшити масштаб.
    private const int MaxCandidateEdges = 300_000;

    private bool _tooManyEdges;

    public RoadNetwork? Network { get; set; }

    public RoadSpatialIndex? Index { get; set; }

    public bool IsVisible { get; set; } = true;

    public IAxes Axes { get; set; } = new Axes();

    public IEnumerable<LegendItem> LegendItems => LegendItem.None;

    public event Action<bool>? TooManyEdgesChanged;

    public AxisLimits GetAxisLimits() => AxisLimits.NoLimits;

    public void Render(RenderPack rp)
    {
        if (Network == null || Index == null) return;

        var (minLat, minLon) = MapProjection.ToLatLon(Axes.XAxis.Min, Axes.YAxis.Min);
        var (maxLat, maxLon) = MapProjection.ToLatLon(Axes.XAxis.Max, Axes.YAxis.Max);

        const double scale = RoadNetwork.CoordinateScale;
        var minLatE7 = (int)(minLat * scale);
        var minLonE7 = (int)(minLon * scale);
        var maxLatE7 = (int)(maxLat * scale);
        var maxLonE7 = (int)(maxLon * scale);

        var tooMany = Index.EstimateEdgeCount(minLatE7, minLonE7, maxLatE7, maxLonE7) > MaxCandidateEdges;
        SetTooManyEdges(tooMany);

        if (tooMany) return;

        using var path = new SKPath();
        using var paint = new SKPaint
        {
            Color = new SKColor(150, 150, 150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f
        };

        foreach (var edge in Index.QueryEdges(minLatE7, minLonE7, maxLatE7, maxLonE7))
        {
            // Двосторонню дорогу малюємо один раз.
            if (Network.EdgeReverse[edge] is var reverse and >= 0 && reverse < edge) continue;

            var hasPrevious = false;
            float previousX = 0, previousY = 0;
            var last = Network.EdgeGeometryStart[edge + 1] - 1;

            for (var i = Network.EdgeGeometryStart[edge]; i <= last; i++)
            {
                var (x, y) = MapProjection.ToMap(Network.GeometryLatitudes[i] / scale, Network.GeometryLongitudes[i] / scale);
                var pixel = Axes.GetPixel(new ScottPlot.Coordinates(x, y));

                if (!hasPrevious)
                    path.MoveTo(pixel.X, pixel.Y);
                else if (i == last || Math.Abs(pixel.X - previousX) + Math.Abs(pixel.Y - previousY) >= 1)
                    path.LineTo(pixel.X, pixel.Y);
                else
                    continue;

                hasPrevious = true;
                previousX = pixel.X;
                previousY = pixel.Y;
            }
        }

        rp.Canvas.DrawPath(path, paint);
    }

    private void SetTooManyEdges(bool value)
    {
        if (_tooManyEdges == value) return;

        _tooManyEdges = value;
        Dispatcher.UIThread.Post(() => TooManyEdgesChanged?.Invoke(value));
    }
}
