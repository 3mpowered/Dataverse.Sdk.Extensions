using Microsoft.Xrm.Sdk.Metadata;

namespace Empowered.Dataverse.Sdk.Extensions.Tests.Extensions
{
    public static class MetadataExtensions
    {
        public static void SetSealedPropertyValue(
            this AttributeMetadata attributeMetadata,
            string sPropertyName,
            object value)
        {
            attributeMetadata.GetType().GetProperty(sPropertyName)?.SetValue(attributeMetadata, value, null);
        }
    }
}
