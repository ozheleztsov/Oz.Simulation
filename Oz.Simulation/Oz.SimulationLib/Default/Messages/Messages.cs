using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default.Messages;

public sealed record ObjectAddedMessage(ISimObject Object, ISimLevel Level);

public sealed record ObjectRemovedMessage(ISimObject Object, ISimLevel Level);

