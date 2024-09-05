using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class OptionSetValueExtensionsTests
    {
        private enum Choice
        {
            Value1 = 123000000,
        }
        [Fact]
        public void ShouldConvertOptionSetToEnum()
        {
            new OptionSetValue((int)Choice.Value1).ToEnum<Choice>().Should().Be(Choice.Value1);
        }

        [Fact]
        public void ShouldReturnNullIfOptionSetIsNull()
        {
            OptionSetValue optionSetValue = null;
            optionSetValue.ToEnum<Choice>().Should().Be(default);
        }

        [Fact]
        public void ShouldReturnEmptyStringOnFormattingNull()
        {
            OptionSetValue optionSetValue = null;
            optionSetValue.Format().Should().Be(string.Empty);
        }

        [Fact]
        public void ShouldFormatOptionSetValueToValue()
        {
            const int value = 1;
            new OptionSetValue(value).Format().Should().Be(value.ToString());
        }
    }
}
