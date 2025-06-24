using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using System.Globalization;
using System.Text;

namespace Zircon.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<VirtualMachineBenchmarks>();
        AppendToCsv(summary);
    }

    private static void AppendToCsv(Summary summary)
    {
        const string csvPath = "benchmark_results.csv";
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        
        var fileExists = File.Exists(csvPath);
        using var writer = new StreamWriter(csvPath, append: true, Encoding.UTF8);
        
        if (!fileExists)
        {
            writer.WriteLine("Date,Benchmark,Mean,Error,StdDev,Gen0,Gen1,Gen2,Allocated");
        }

        foreach (var report in summary.Reports)
        {
            var benchmark = report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
            var statistics = report.ResultStatistics;
            var gcStats = report.GcStats;
            
            var mean = FormatNanoseconds(statistics?.Mean);
            var error = FormatNanoseconds(statistics?.StandardError);
            var stdDev = FormatNanoseconds(statistics?.StandardDeviation);
            var gen0 = gcStats.Gen0Collections.ToString(CultureInfo.InvariantCulture);
            var gen1 = gcStats.Gen1Collections.ToString(CultureInfo.InvariantCulture);
            var gen2 = gcStats.Gen2Collections.ToString(CultureInfo.InvariantCulture);
            var allocated = FormatBytes(gcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase));

            writer.WriteLine($"{timestamp},{benchmark},{mean},{error},{stdDev},{gen0},{gen1},{gen2},{allocated}");
        }
    }

    private static string FormatNanoseconds(double? nanoseconds)
    {
        if (!nanoseconds.HasValue || double.IsNaN(nanoseconds.Value))
            return "";
            
        return nanoseconds.Value < 1000 
            ? $"{nanoseconds.Value:F2}" 
            : $"{nanoseconds.Value / 1000:F2}";
    }

    private static string FormatBytes(long? bytes)
    {
        if (!bytes.HasValue || bytes == 0) return "0 B";
        
        string[] units = ["B", "KB", "MB", "GB"];
        var unitIndex = 0;
        var size = (double)bytes;
        
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }
        
        return $"{size:F2} {units[unitIndex]}";
    }
}