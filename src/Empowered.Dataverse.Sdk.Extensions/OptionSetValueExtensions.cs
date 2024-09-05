using System;
using Microsoft.Xrm.Sdk;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class OptionSetValueExtensions
    {
        public static TEnum ToEnum<TEnum>(this OptionSetValue optionSetValue)
            where TEnum : Enum =>
            optionSetValue?.Value == null ? default : (TEnum)Enum.ToObject(typeof(TEnum), optionSetValue.Value);

        public static string Format(this OptionSetValue optionSetValue) =>
            optionSetValue == null ? string.Empty : optionSetValue.Value.ToString();
    }
}
