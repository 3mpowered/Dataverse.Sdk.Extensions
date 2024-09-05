using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class MoneyExtensionsTests
    {
        [Fact]
        public void ShouldFormatMoneyAsDecimal()
        {
            const decimal value = 9.99m;
            const string format = "E";
            new Money(value).Format(format).Should().Be(value.ToString(format));
        }
    }
}
