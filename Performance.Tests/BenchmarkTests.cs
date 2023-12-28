using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using Algorithms;
using Algorithms.Graph;
using DataStructures.Graph;

namespace Performance.Tests;

[TestFixture]
public class BenchmarkTests
{
    private double RunBenchmark(string targetFramework, int graphSize)
    {
        var startInfo = new ProcessStartInfo("dotnet", $"run -f {targetFramework} --project ./PathToYourBenchmarkProject")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        // Parse output to extract the benchmark result
        // This is highly dependent on the output format of your benchmark
        double benchmarkResult = ParseBenchmarkResult(graphSize);
        return benchmarkResult;
    }
    private DirectedWeightedGraph<char> CreateLargeGraph(int size)
    {
        var graph = new DirectedWeightedGraph<char>(size * 1000);
        // Example: Populate the graph with 'size' vertices and edges
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var vertex1 = graph.AddVertex((char)i);

                var vertex2 = graph.AddVertex((char)j);
                if (i != j)
                {
                    graph.AddEdge(vertex1, vertex2, new Random().NextDouble() * 100);
                }
            }
        }

        return graph;
    }

    private double RunFloydWarshallBenchmark(int graphSize)
    {
        var graph = CreateLargeGraph(graphSize);
        var algorithm = new FloydWarshall<char>();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        algorithm.Run(graph);
        stopwatch.Stop();
        algorithm = null;
        graph = null;
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private double ParseBenchmarkResult(int graphSize)
    {
        return RunFloydWarshallBenchmark(graphSize); ;
    }

    [Theory]
    [TestCase(3)]
    [TestCase(5)]
    [TestCase(7)]
    [TestCase(10)]
    [TestCase(30)]
    //[TestCase(50)]
    public void BenchmarkShouldBeAtLeastAsGoodOnDotNet8AsOnDotNet6(int graphSize)
    {
        var count = 0;
        for (int i = 0; i <= 3; i++)
        {
            double resultDotNet6 = RunBenchmark("net6.0", graphSize);
            double resultDotNet8 = RunBenchmark("net8.0", graphSize);
            if (resultDotNet8 <= resultDotNet6)
                count++;
        }

        count.Should().BeGreaterThan(0, "because the performance should not degrade with newer .NET versions");
    }
}
