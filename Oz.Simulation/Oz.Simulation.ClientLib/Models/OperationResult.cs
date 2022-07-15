using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Oz.Simulation.ClientLib.Models;

public class OperationResult<T> : OperationResult
{
    public OperationResult(T result) =>
        Result = result;

    public OperationResult() =>
        Result = default;

    public T? Result { get; private set; }

    public void SetResult(T result) =>
        Result = result;
}

public class OperationResult
{
    public List<FieldError> Errors { get; } = new();

    public bool HasErrors => Errors.Any();

    public void AddError(string property, string error) =>
        Errors.Add(new FieldError(property, error));

    public string GetErrorsString() =>
        JsonSerializer.Serialize(Errors);

    public static OperationResult<U> CombineOperationResults<K, U>(
        IEnumerable<OperationResult<K>> sourceOperationResults, string errorPrefix)
    {
        List<FieldError> fieldErrors = new();
        foreach (var operationResult in sourceOperationResults)
        {
            fieldErrors.AddRange(operationResult.Errors.Select(sourceFieldError => sourceFieldError with {Property = $"{errorPrefix}_{sourceFieldError.Property}"}));
        }

        var newOperationResult = new OperationResult<U>();
        newOperationResult.Errors.AddRange(fieldErrors);
        return newOperationResult;
    }
}

public record FieldError(string Property, string Error);