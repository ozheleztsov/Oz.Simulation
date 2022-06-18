using Oz.SimulationLib.Console;

await using MessageChannelTestRun messageChannelTestRun = new();
await messageChannelTestRun.RunAsync();
Console.ReadLine();
await messageChannelTestRun.DisposeAsync();
Console.WriteLine("Done");