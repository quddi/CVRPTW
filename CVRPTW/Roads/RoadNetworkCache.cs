using System.Runtime.InteropServices;

namespace CVRPTW;

/// <summary>
/// Бінарний кеш дорожньої мережі поруч із розбором .osm.pbf: повторне завантаження незмінного файлу не перечитує OSM.
/// </summary>
public static class RoadNetworkCache
{
    private const string Magic = "CVRPTWRN";

    // Збільшувати при зміні профілю доріг або формату, щоб старі кеші відкидалися.
    private const int FormatVersion = 1;

    public static RoadNetwork LoadOrBuild(string pbfPath, string cacheDirectory, IProgress<string>? progress = null)
    {
        var cachePath = Path.Combine(cacheDirectory, Path.GetFileName(pbfPath) + ".roads");

        if (TryLoad(cachePath, pbfPath, progress, out var cached)) return cached;

        var network = RoadNetworkBuilder.BuildFromPbf(pbfPath, progress);

        progress?.Report("Збереження кешу...");
        Directory.CreateDirectory(cacheDirectory);
        Save(cachePath, pbfPath, network);

        return network;
    }

    private static bool TryLoad(string cachePath, string pbfPath, IProgress<string>? progress, out RoadNetwork network)
    {
        network = null!;

        if (!File.Exists(cachePath)) return false;

        try
        {
            progress?.Report("Читання кешу...");

            using var stream = File.OpenRead(cachePath);
            using var reader = new BinaryReader(stream);

            if (reader.ReadString() != Magic || reader.ReadInt32() != FormatVersion) return false;
            if (reader.ReadString() != Path.GetFullPath(pbfPath)) return false;

            var info = new FileInfo(pbfPath);
            if (reader.ReadInt64() != info.Length || reader.ReadInt64() != info.LastWriteTimeUtc.Ticks) return false;

            var applied = reader.ReadInt32();
            var unsupported = reader.ReadInt32();

            network = new RoadNetwork
            {
                NodeLatitudes = ReadArray<int>(stream),
                NodeLongitudes = ReadArray<int>(stream),
                EdgeFrom = ReadArray<int>(stream),
                EdgeTo = ReadArray<int>(stream),
                EdgeLength = ReadArray<float>(stream),
                EdgeReverse = ReadArray<int>(stream),
                EdgeGeometryStart = ReadArray<int>(stream),
                GeometryLatitudes = ReadArray<int>(stream),
                GeometryLongitudes = ReadArray<int>(stream),
                ForbiddenTurns = ReadArray<long>(stream),
                RestrictionsApplied = applied,
                RestrictionsUnsupported = unsupported
            };

            return true;
        }
        catch (Exception exception) when (exception is IOException or EndOfStreamException)
        {
            return false;
        }
    }

    private static void Save(string cachePath, string pbfPath, RoadNetwork network)
    {
        var info = new FileInfo(pbfPath);
        var temporaryPath = cachePath + ".tmp";

        using (var stream = File.Create(temporaryPath))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(Magic);
            writer.Write(FormatVersion);
            writer.Write(Path.GetFullPath(pbfPath));
            writer.Write(info.Length);
            writer.Write(info.LastWriteTimeUtc.Ticks);
            writer.Write(network.RestrictionsApplied);
            writer.Write(network.RestrictionsUnsupported);

            WriteArray(stream, network.NodeLatitudes);
            WriteArray(stream, network.NodeLongitudes);
            WriteArray(stream, network.EdgeFrom);
            WriteArray(stream, network.EdgeTo);
            WriteArray(stream, network.EdgeLength);
            WriteArray(stream, network.EdgeReverse);
            WriteArray(stream, network.EdgeGeometryStart);
            WriteArray(stream, network.GeometryLatitudes);
            WriteArray(stream, network.GeometryLongitudes);
            WriteArray(stream, network.ForbiddenTurns);
        }

        File.Move(temporaryPath, cachePath, overwrite: true);
    }

    private static void WriteArray<T>(Stream stream, T[] array) where T : unmanaged
    {
        stream.Write(BitConverter.GetBytes(array.Length));
        stream.Write(MemoryMarshal.AsBytes(array.AsSpan()));
    }

    private static T[] ReadArray<T>(Stream stream) where T : unmanaged
    {
        Span<byte> lengthBytes = stackalloc byte[sizeof(int)];
        stream.ReadExactly(lengthBytes);

        var array = new T[BitConverter.ToInt32(lengthBytes)];
        stream.ReadExactly(MemoryMarshal.AsBytes(array.AsSpan()));

        return array;
    }
}
