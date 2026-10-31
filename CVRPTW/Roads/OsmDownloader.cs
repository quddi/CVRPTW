using System.Diagnostics;

namespace CVRPTW;

public static class OsmDownloader
{
    private static readonly TimeSpan ReportInterval = TimeSpan.FromMilliseconds(250);

    // Завантажує файл у тимчасовий .part і лише після повного завантаження замінює ним destination,
    // тож збій посеред завантаження не псує наявний файл. onProgress: (отримано байтів, усього байтів або null).
    public static async Task DownloadAsync(string url, string destination, Action<long, long?>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);

        var temporaryPath = destination + ".part";

        try
        {
            using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength;

            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var target = File.Create(temporaryPath))
            {
                var buffer = new byte[81920];
                long received = 0;
                var timer = Stopwatch.StartNew();

                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    received += read;

                    if (timer.Elapsed < ReportInterval) continue;

                    timer.Restart();
                    onProgress?.Invoke(received, total);
                }

                onProgress?.Invoke(received, total);
            }

            File.Move(temporaryPath, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
