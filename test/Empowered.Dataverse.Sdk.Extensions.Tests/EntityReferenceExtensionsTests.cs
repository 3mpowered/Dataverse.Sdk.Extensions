using System;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class EntityReferenceExtensionsTests
    {
        [Fact]
        public void ShouldFormatEntityReferenceToEmptyStringWhenNull()
        {
            EntityReference entityReference = null;
            entityReference.Format().Should().Be(string.Empty);
        }

        [Fact]
        public void ShouldFormatEntityReferenceToLogicalNameAndId()
        {
            var entityReference = new EntityReference("account", Guid.NewGuid());
            entityReference.Format().Should().Be($"{entityReference.LogicalName}/{entityReference.Id}");
        }
    }
}
