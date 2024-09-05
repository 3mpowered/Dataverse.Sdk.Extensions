using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class EnumExtensionsTests
    {
        private enum Test
        {
            Value1,
            Value2
        }

        [Fact]
        public void ShouldReturnIntegerValueFromEnum()
        {
            Test.Value2.ToInt().Should().Be((int)Test.Value2);
        }

        [Fact]
        public void ShouldReturnOptionSetValueFromEnum()
        {
            var optionSetValue = Test.Value1
                .ToOptionSetValue();
            optionSetValue
                .Should()
                .NotBeNull();
            optionSetValue
                .Value
                .Should()
                .Be((int)Test.Value1);
        }
    }
}
