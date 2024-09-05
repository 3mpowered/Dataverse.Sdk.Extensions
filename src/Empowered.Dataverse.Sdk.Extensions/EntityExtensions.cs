using Microsoft.Xrm.Sdk;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class EntityExtensions
    {
        public static string Format(this Entity entity)
        {
            return entity == null ? string.Empty : $"{entity.LogicalName}/{entity.Id}";
        }
    }
}
