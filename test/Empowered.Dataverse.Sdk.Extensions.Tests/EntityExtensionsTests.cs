using System;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class EntityExtensionsTests
    {
        [Fact]
        public void ShouldFormatEntityWithLogicalNameAndId()
        {
            const string entityName = "account";
            var entityId = Guid.NewGuid();
            var entity = new Entity(entityName, entityId);

            entity.Format().Should().Be($"{entityName}/{entityId}");
        }

        [Fact]
        public void ShouldFormatEntityToEmptyStringIfNull()
        {
            Entity entity = null;

            entity.Format().Should().Be(string.Empty);
        }
    }
}
