using NetFusion.Identity.App.Services;

namespace NetFusion.Identity.Infra.Services;

public class NullServiceContext : IServiceContext
{
    public void RecordValue<TValue>(string sourceType, string name, TValue value)
    { 
        // This method should never be implemented for a Null Implementation
    }

    public bool TryGetValue<TValue>(string sourceType, string name, out TValue? value)
    {
        throw new NotImplementedException("Null Implementation should never be called to received value");
    }
}
