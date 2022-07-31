using Microsoft.Extensions.Logging;
using Oz.Simulation.ClientLib.Extensions;
using Oz.Simulation.ClientLib.Messages;
using Oz.SimulationLib;
using Oz.SimulationLib.Components;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Default.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Components;

public class ObjectSystemStatisticsComponent : SimComponent
{
    private const double UpdateIntervalInSecs = 2.0;
    private readonly ISimContext _context;

    private IAsyncDisposable? _objectAddedSubscription;
    private IAsyncDisposable? _objectRemovedSubscription;
    private bool _shouldUpdateStats;

    private double _timer;


    public ObjectSystemStatisticsComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null,
        string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public double InitialTotalEnergy { get; private set; }
    public double CurrentTotalEnergy { get; private set; }
    public Vector3 InitialTotalMomentum { get; private set; }
    public Vector3 CurrentTotalMomentum { get; private set; }

    public override async Task OnInitializeAsync()
    {
        _objectAddedSubscription =
            await Context.MessageChannel.RegisterAsync(new ObjectAddedHandler(this)).ConfigureAwait(false);
        _objectRemovedSubscription =
            await Context.MessageChannel.RegisterAsync(new ObjectRemovedHandler(this)).ConfigureAwait(false);
    }

    public override async Task OnDestroyAsync()
    {
        if (_objectAddedSubscription != null)
        {
            await _objectAddedSubscription.DisposeAsync();
            _objectAddedSubscription = null;
        }

        if (_objectRemovedSubscription != null)
        {
            await _objectRemovedSubscription.DisposeAsync();
            _objectRemovedSubscription = null;
        }
    }

    public override async Task OnUpdateAsync()
    {
        if (Destroyed)
        {
            return;
        }
        
        if (_shouldUpdateStats)
        {
            var objects = await Get3dObjectsAsync().ConfigureAwait(false);
            UpdateTotalEnergy(true, objects);
            await SendStatsMessage().ConfigureAwait(false);
        }
        else
        {
            _timer += Context.Time.DeltaTime;
            if (_timer >= UpdateIntervalInSecs)
            {
                _timer -= UpdateIntervalInSecs;
                var objects = await Get3dObjectsAsync().ConfigureAwait(false);
                UpdateTotalEnergy(false, objects);
                await SendStatsMessage().ConfigureAwait(false);
            }
        }

        _shouldUpdateStats = false;
    }

    private async Task SendStatsMessage() =>
        await Context.MessageChannel.SendMessageAsync(new StatsMessage(InitialTotalEnergy, InitialTotalMomentum,
            CurrentTotalEnergy, CurrentTotalMomentum)).ConfigureAwait(false);

    private async Task<ImmutableList<PositionVelocityMass>> Get3dObjectsAsync()
    {
        if (Context.Level is null)
        {
            return new List<PositionVelocityMass>().ToImmutableList();
        }
        var objects3d = (await Context.Level.FindAsync(obj => obj is ISimObject3D).ConfigureAwait(false))
            .Cast<ISimObject3D>();
        var positionVelocitiesMasses = await objects3d.ToPositionVelocityMassAsync().ConfigureAwait(false);
        return positionVelocitiesMasses;
    }

    private void UpdateTotalEnergy(bool updateInitialEnergy, ImmutableList<PositionVelocityMass> objects)
    {
        var kinEnergyCom = ComputeKineticEnergyOfCom(objects);
        var potEnergy = ComputePotentialEnergy(objects);
        var kinEnergy = ComputeKineticEnergy(objects);
        var totalEnergy = kinEnergyCom + potEnergy + kinEnergy;
        var totalMomentum = ComputeTotalMomentum(objects);

        CurrentTotalEnergy = totalEnergy;
        CurrentTotalMomentum = totalMomentum;
        if (updateInitialEnergy)
        {
            InitialTotalEnergy = totalEnergy;
            InitialTotalMomentum = totalMomentum;
        }
    }

    private Vector3 ComputeTotalMomentum(ImmutableList<PositionVelocityMass> objects)
    {
        var result = new Vector3();
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            result += obj.Mass * Vector3.Cross(obj.Position, obj.Velocity);
        }

        return result;
    }

    private static double ComputeKineticEnergy(ImmutableList<PositionVelocityMass> objects)
    {
        var total = 0.0;
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            total += obj.Mass * obj.Velocity.MagnitudeSqr;
        }

        total *= 0.5;
        return total;
    }

    private static double ComputeKineticEnergyOfCom(ImmutableList<PositionVelocityMass> objects)
    {
        var massTotal = objects.Sum(x => x.Mass);
        var velCom = new Vector3();

        foreach (var obj in objects)
        {
            velCom += obj.Mass * obj.Velocity;
        }

        velCom *= 1.0 / massTotal;
        var velComNorm = velCom.Magnitude;
        var kinEnergyCom = 0.5 * massTotal * velComNorm * velComNorm;
        return kinEnergyCom;
    }

    private static double ComputePotentialEnergy(ImmutableList<PositionVelocityMass> objects)
    {
        var total = 0.0;
        for (var i = 0; i < objects.Count; i++)
        {
            for (var j = 0; j < objects.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var obji = objects[i];
                var objj = objects[j];
                var direction = objj.Position - obji.Position;
                var distance = direction.Magnitude;
                total += obji.Mass * objj.Mass / distance;
            }
        }

        total *= -0.5 * Constants.G;
        return total;
    }

    private void SetUpdateStatsFlag() => _shouldUpdateStats = true;


    private class ObjectAddedHandler : IMessageObserver<ObjectAddedMessage>
    {
        private readonly ObjectSystemStatisticsComponent _parent;

        public ObjectAddedHandler(ObjectSystemStatisticsComponent parent) =>
            _parent = parent;

        public Task ReceiveAsync(ObjectAddedMessage? message)
        {
            _parent.SetUpdateStatsFlag();
            return Task.CompletedTask;
        }
    }

    private class ObjectRemovedHandler : IMessageObserver<ObjectRemovedMessage>
    {
        private readonly ObjectSystemStatisticsComponent _parent;

        public ObjectRemovedHandler(ObjectSystemStatisticsComponent parent) =>
            _parent = parent;

        public Task ReceiveAsync(ObjectRemovedMessage? message)
        {
            _parent.SetUpdateStatsFlag();
            return Task.CompletedTask;
        }
    }
}