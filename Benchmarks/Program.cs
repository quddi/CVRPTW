using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

#if DEBUG
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("[DEBUG MODE] Бенчмарки запускаються в режимі InProcess.");
Console.WriteLine("Для точних вимірювань запускайте в режимі Release: dotnet run -c Release --project Benchmarks");
Console.ResetColor();
Console.WriteLine();
var config = new DebugInProcessConfig();
#else
var config = DefaultConfig.Instance;
#endif

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);