using Oz.SimulationLib.Console;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

ITime time = new TimeManager();
IMessageChannel messageChannel = new MessageChannel();
ISimulator simulator = new Simulator(time, messageChannel);

async Task TestMessageChannel()
{
    await using MessageChannelTestRun messageChannelTestRun = new();
    await messageChannelTestRun.RunAsync();
    Console.ReadLine();
    await messageChannelTestRun.DisposeAsync();
    Console.WriteLine("Done");
}