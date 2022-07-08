using Microsoft.Extensions.Options;
using Oz.Simulation.Client.Models.Settings;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Oz.Simulation.Client.Services;

public class ObjectModelFileReader : IObjectModelReader
{
    private readonly IOptions<SimulationSettings> _settings;

    public ObjectModelFileReader(IOptions<SimulationSettings> settings) =>
        _settings = settings;

    public async Task<ObjectModelCollection> LoadAsync()
    {
        await using var fileStream = new FileStream(_settings.Value.ObjectFileName ?? throw new ArgumentException(nameof(_settings.Value.ObjectFileName)),
            FileMode.Open, FileAccess.Read);
        return await JsonSerializer.DeserializeAsync<ObjectModelCollection>(fileStream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new ObjectModelCollection(new List<ObjectModel>());
    }
}