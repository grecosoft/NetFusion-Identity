using System.Diagnostics.CodeAnalysis;
using NetFusion.Identity.App.Services;

namespace NetFusion.Identity.Tests.Mocks;

public class ServiceContext : IServiceContext
{
    private readonly Dictionary<RecordedValue, object> _values = new();

    private record RecordedValue(string SourceType, string Name);

    public void RecordValue<TValue>(string sourceType, string name, [DisallowNull] TValue value) 
    {
        if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

        var valueKey = new RecordedValue(sourceType, name);
        _values[valueKey] = value;
    }

    public bool TryGetValue<TValue>(string sourceType, string name, out TValue? value)
    {
        var valueKey = new RecordedValue(sourceType, name);
        if (_values.TryGetValue(valueKey, out var recordedValue))
        {
            value = (TValue)recordedValue;
            return true;
        }

        value = default;
        return false;
    }

    public void DeleteValue(string sourceType, string name) => 
        _values.Remove(new RecordedValue(sourceType, name));
}

