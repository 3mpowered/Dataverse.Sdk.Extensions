using System;
using System.Collections.Generic;
using System.Linq;
using Empowered.Dataverse.Sdk.Extensions.Tests.Extensions;
using Empowered.Dataverse.Sdk.Extensions.Tests.Model;
using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Xrm.Sdk.Query;
using NSubstitute;
using Xunit;

namespace Empowered.Dataverse.Sdk.Extensions.Tests
{
    public class OrganizationServiceExtensionsTests
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationServiceExtensionsTests()
        {
            _organizationService = Substitute.For<IOrganizationService>();
            _organizationService.Create(null).ReturnsForAnyArgs(Guid.NewGuid());
        }

        [Fact]
        public void ShouldCreateRange()
        {
            var entities = new[]
            {
                new Entity("account"),
                new Entity("account")
            };

            var ids = _organizationService.CreateRange(entities);
            ids
                .Should().NotBeNull()
                .And.HaveCount(entities.Length)
                .And.NotContain(id => id == Guid.Empty);
        }

        [Fact]
        public void ShouldUpdateRange()
        {
            var entities = new[]
            {
                new Entity("account", Guid.NewGuid()),
                new Entity("account", Guid.NewGuid())
            };

            _organizationService.UpdateRange(entities);
            _organizationService.Received(entities.Length).Update(Arg.Any<Entity>());
        }

        [Fact]
        public void ShouldDeleteRange()
        {
            var entities = new[]
            {
                new Entity("account", Guid.NewGuid()),
                new Entity("account", Guid.NewGuid())
            };

            _organizationService.DeleteRange(entities);
            _organizationService.Received(2).Delete(Arg.Any<string>(), Arg.Any<Guid>());
        }

        [Fact]
        public void ShouldRetrieveMultipleStronglyTyped()
        {
            var account = new Entity(Account.EntityLogicalName, Guid.NewGuid())
            {
                [Account.Fields.Name] = "Test"
            };
            var query = new QueryExpression(Account.EntityLogicalName);
            _organizationService.RetrieveMultiple(query).Returns(new EntityCollection(
                new List<Entity>
                {
                    account
                })
            );

            _organizationService.RetrieveMultiple<Account>(query).Should()
                .HaveCount(1).And
                .ContainSingle(entity =>
                    entity.Id == account.Id && entity.Name == account.GetAttributeValue<string>(Account.Fields.Name));
        }

        [Fact]
        public void ShouldOnlyRetrieveIdWhenNotSpecifingColumnSet()
        {
            var entity = new Entity(Account.EntityLogicalName, Guid.NewGuid());
            entity[Account.Fields.Id] = entity.Id;

            _organizationService.Retrieve(entity.LogicalName, entity.Id, Arg.Any<ColumnSet>())
                .Returns(entity);

            var account = _organizationService.Retrieve<Account>(entity.ToEntityReference());
            account.Id.Should().Be(entity.Id);
            account.LogicalName.Should().Be(entity.LogicalName);
            account.Attributes.Should().HaveCount(1);
            account.Attributes
                .Should()
                .ContainSingle(attribute => attribute.Key == Account.Fields.Id);
        }

        [Fact]
        public void ShouldRetrieveStronglyTyped()
        {
            var entity = new Entity(Account.EntityLogicalName, Guid.NewGuid())
            {
                [Account.Fields.Name] = "test"
            };

            _organizationService.Retrieve(entity.LogicalName, entity.Id, Arg.Any<ColumnSet>())
                .Returns(entity);

            var account = _organizationService.Retrieve<Account>(entity.ToEntityReference(), Account.Fields.Name);
            account.Id.Should().Be(entity.Id);
            account.LogicalName.Should().Be(entity.LogicalName);
            account.Name.Should().Be(entity.GetAttributeValue<string>(Account.Fields.Name));
        }

        [Fact]
        public void ShouldRetrieveStronglyTypedWithRelatedEntities()
        {
            // When using early bound types the IOrganizationService already casts from entity to early bound entity
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                [Contact.Fields.FullName] = "Testo Tester"
            };
            var account = new Entity(Account.EntityLogicalName, Guid.NewGuid())
            {
                [Account.Fields.Name] = "test",
                RelatedEntities = new RelatedEntityCollection
                {
                    {
                        new Relationship(Account.Fields.contact_customer_accounts),
                        new EntityCollection(new List<Entity> { contact })
                    }
                }
            };

            var contactExpression = new QueryExpression(Contact.EntityLogicalName);

            _organizationService.Execute(Arg.Any<RetrieveRequest>())
                .Returns(new RetrieveResponse
                    {
                        [nameof(RetrieveResponse.Entity)] = account
                    }
                );

            var retrievedAccount = _organizationService.Retrieve<Account>(account.ToEntityReference(),
                new[]
                {
                    new KeyValuePair<string, QueryBase>(Account.Fields.contact_customer_accounts, contactExpression)
                });

            retrievedAccount.Id.Should().Be(account.Id);
            retrievedAccount.Name.Should().Be(account.GetAttributeValue<string>(Account.Fields.Name));
            retrievedAccount.LogicalName.Should().Be(account.LogicalName);

            retrievedAccount.contact_customer_accounts
                .Should()
                .ContainSingle(c =>
                    c.Id == contact.Id
                    && c.LogicalName == contact.LogicalName
                    && c.FullName == contact.GetAttributeValue<string>(Contact.Fields.FullName)
                );
        }

        [Fact]
        public void ShouldReturnStronglyTypedExecuteResponse()
        {
            var response = new WhoAmIResponse
            {
                [nameof(WhoAmIResponse.OrganizationId)] = Guid.NewGuid(),
                [nameof(WhoAmIResponse.BusinessUnitId)] = Guid.NewGuid(),
                [nameof(WhoAmIResponse.UserId)] = Guid.NewGuid(),
            };
            _organizationService.Execute(Arg.Any<WhoAmIRequest>())
                .Returns(response);

            var whoAmIResponse = _organizationService.Execute<WhoAmIResponse>(new WhoAmIRequest());

            whoAmIResponse.OrganizationId.Should().Be(response.OrganizationId);
            whoAmIResponse.BusinessUnitId.Should().Be(response.BusinessUnitId);
            whoAmIResponse.UserId.Should().Be(response.UserId);
        }

        [Fact]
        public void ShouldGetEntityMetadataByLogicalName()
        {
            var response = new RetrieveEntityResponse
            {
                [nameof(RetrieveEntityResponse.EntityMetadata)] = new EntityMetadata
                {
                    LogicalName = Account.EntityLogicalName,
                    MetadataId = Guid.NewGuid()
                }
            };
            _organizationService.Execute(Arg.Any<RetrieveEntityRequest>())
                .Returns(response);

            var entityMetadata =
                _organizationService.GetEntityMetadata(response.EntityMetadata.LogicalName, EntityFilters.All, false);

            entityMetadata.MetadataId.Should().Be(response.EntityMetadata.MetadataId);
            entityMetadata.LogicalName.Should().Be(response.EntityMetadata.LogicalName);
        }

        [Fact]
        public void ShouldGetEntityMetadataByMetadataId()
        {
            var metadataId = Guid.NewGuid();
            var response = new RetrieveEntityResponse
            {
                [nameof(RetrieveEntityResponse.EntityMetadata)] = new EntityMetadata
                {
                    LogicalName = Account.EntityLogicalName,
                    MetadataId = metadataId
                }
            };
            _organizationService.Execute(Arg.Any<RetrieveEntityRequest>())
                .Returns(response);

            var entityMetadata = _organizationService.GetEntityMetadata(metadataId, EntityFilters.All, false);

            entityMetadata.MetadataId.Should().Be(response.EntityMetadata.MetadataId);
            entityMetadata.LogicalName.Should().Be(response.EntityMetadata.LogicalName);
        }

        [Fact]
        public void ShouldGetGlobalOptionSetMetadataByLogicalName()
        {
            var metadataId = Guid.NewGuid();
            var logicalName = "test";
            var response = new RetrieveOptionSetResponse
            {
                [nameof(RetrieveOptionSetResponse.OptionSetMetadata)] = new OptionSetMetadata
                {
                    MetadataId = metadataId,
                    Name = logicalName
                }
            };
            _organizationService.Execute(Arg.Any<RetrieveOptionSetRequest>())
                .Returns(response);

            var optionSetMetadata =
                _organizationService.GetGlobalOptionSetMetadata<OptionSetMetadata>(logicalName, false);

            optionSetMetadata.MetadataId.Should().Be(metadataId);
            optionSetMetadata.Name.Should().Be(logicalName);
        }


        [Fact]
        public void ShouldGetGlobalOptionSetMetadataByMetadataId()
        {
            var metadataId = Guid.NewGuid();
            var logicalName = "test";
            var response = new RetrieveOptionSetResponse
            {
                [nameof(RetrieveOptionSetResponse.OptionSetMetadata)] = new OptionSetMetadata
                {
                    MetadataId = metadataId,
                    Name = logicalName
                }
            };
            _organizationService.Execute(Arg.Any<RetrieveOptionSetRequest>())
                .Returns(response);

            var optionSetMetadata =
                _organizationService.GetGlobalOptionSetMetadata<OptionSetMetadata>(metadataId, false);

            optionSetMetadata.MetadataId.Should().Be(metadataId);
            optionSetMetadata.Name.Should().Be(logicalName);
        }

        [Fact]
        public void ShouldGetAttributeMetadataByLogicalName()
        {
            var metadataId = Guid.NewGuid();
            const string entityLogicalName = Account.EntityLogicalName;
            const string logicalName = Account.Fields.Name;
            var nameAttributeMetadata = new StringAttributeMetadata
            {
                MetadataId = metadataId,
                LogicalName = logicalName
            };
            // Why has entity logical name no setter 🙄
            nameAttributeMetadata.SetSealedPropertyValue(nameof(nameAttributeMetadata.EntityLogicalName),
                entityLogicalName);

            var response = new RetrieveAttributeResponse
            {
                [nameof(RetrieveAttributeResponse.AttributeMetadata)] = nameAttributeMetadata
            };
            _organizationService.Execute(Arg.Any<RetrieveAttributeRequest>())
                .Returns(response);

            var attributeMetadata =
                _organizationService.GetAttributeMetadata<StringAttributeMetadata>(entityLogicalName, logicalName,
                    false);

            attributeMetadata.MetadataId.Should().Be(nameAttributeMetadata.MetadataId);
            attributeMetadata.LogicalName.Should().Be(nameAttributeMetadata.LogicalName);
            attributeMetadata.EntityLogicalName.Should().Be(nameAttributeMetadata.EntityLogicalName);
        }

        [Fact]
        public void ShouldGetAttributeMetadataByMetadataId()
        {
            var metadataId = Guid.NewGuid();
            const string entityLogicalName = Account.EntityLogicalName;
            const string logicalName = Account.Fields.Name;
            var nameAttributeMetadata = new StringAttributeMetadata
            {
                MetadataId = metadataId,
                LogicalName = logicalName
            };
            // Why has entity logical name no setter 🙄
            nameAttributeMetadata.SetSealedPropertyValue(nameof(nameAttributeMetadata.EntityLogicalName),
                entityLogicalName);

            var response = new RetrieveAttributeResponse
            {
                [nameof(RetrieveAttributeResponse.AttributeMetadata)] = nameAttributeMetadata
            };
            _organizationService.Execute(Arg.Any<RetrieveAttributeRequest>())
                .Returns(response);

            var attributeMetadata =
                _organizationService.GetAttributeMetadata<StringAttributeMetadata>(metadataId, false);

            attributeMetadata.MetadataId.Should().Be(nameAttributeMetadata.MetadataId);
            attributeMetadata.LogicalName.Should().Be(nameAttributeMetadata.LogicalName);
            attributeMetadata.EntityLogicalName.Should().Be(nameAttributeMetadata.EntityLogicalName);
        }

        [Fact]
        public void ShouldReturnWhoAmiResponse()
        {
            var response = new WhoAmIResponse
            {
                [nameof(WhoAmIResponse.OrganizationId)] = Guid.NewGuid(),
                [nameof(WhoAmIResponse.BusinessUnitId)] = Guid.NewGuid(),
                [nameof(WhoAmIResponse.UserId)] = Guid.NewGuid(),
            };
            _organizationService.Execute(Arg.Any<WhoAmIRequest>())
                .Returns(response);

            var whoAmIResponse = _organizationService.WhoAmI();

            whoAmIResponse.UserId.Should().Be(response.UserId);
            whoAmIResponse.BusinessUnitId.Should().Be(response.BusinessUnitId);
            whoAmIResponse.OrganizationId.Should().Be(response.OrganizationId);
        }

        [Fact]
        public void ShouldRetrieveCurrentOrganization()
        {
            var organizationUrl = "https://test.crm4.dynamics.com";
            var organizationDetail = new OrganizationDetail
            {
                OrganizationId = Guid.NewGuid(),
                Endpoints = new EndpointCollection
                {
                    { EndpointType.WebApplication, organizationUrl }
                }
            };
            var response = new RetrieveCurrentOrganizationResponse
            {
                [nameof(RetrieveCurrentOrganizationResponse.Detail)] = organizationDetail
            };

            _organizationService.Execute(Arg.Any<RetrieveCurrentOrganizationRequest>())
                .Returns(response);

            var organization = _organizationService.RetrieveCurrentOrganization();

            organization.OrganizationId.Should().Be(organizationDetail.OrganizationId);
            organization.Endpoints[EndpointType.WebApplication].Should().Be(organizationUrl);
        }
    }
}
