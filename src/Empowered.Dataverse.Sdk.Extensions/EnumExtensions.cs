using System;
using Microsoft.Xrm.Sdk;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class EnumExtensions
    {
        public static int ToInt(this Enum @enum) => Convert.ToInt32(@enum);
        public static OptionSetValue ToOptionSetValue(this Enum @enum) => new OptionSetValue(@enum.ToInt());
    }
}

