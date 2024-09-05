using Microsoft.Xrm.Sdk;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class EntityReferenceExtensions
    {
        public static string Format(this EntityReference entityReference) => entityReference == null
            ? string.Empty
            : $"{entityReference.LogicalName}/{entityReference.Id}";
    }
}
