using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Xrm.Sdk.Query;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class OrganizationServiceExtensions
    {
        public static IEnumerable<Guid> CreateRange(this IOrganizationService service, IEnumerable<Entity> records) =>
            records.Select(service.Create).ToList();

        public static void UpdateRange(this IOrganizationService service, IEnumerable<Entity> records) =>
            records.ToList().ForEach(service.Update);

        public static void DeleteRange(this IOrganizationService service, IEnumerable<Entity> records) =>
            records.ToList().ForEach(record => service.Delete(record.LogicalName, record.Id));

        public static IEnumerable<TEntity> RetrieveMultiple<TEntity>(this IOrganizationService service, QueryBase query)
            where TEntity : Entity =>
            service.RetrieveMultiple(query).Entities?.Select(entity => entity.ToEntity<TEntity>()) ??
            Enumerable.Empty<TEntity>();

        public static TEntity Retrieve<TEntity>(this IOrganizationService service, EntityReference reference,
            params string[] columns)
            where TEntity : Entity
        {
            ColumnSet columnSet = GetColumnSet(columns);

            return service.Retrieve(reference.LogicalName, reference.Id, columnSet).ToEntity<TEntity>();
        }

        public static TEntity Retrieve<TEntity>(this IOrganizationService organizationService,
            EntityReference entityReference, ICollection<KeyValuePair<string, QueryBase>> relatedQueries,
            params string[] columns) where TEntity : Entity
        {
            var columnSet = GetColumnSet(columns);
            var relationshipQueryCollection = new RelationshipQueryCollection();
            var queryCollection = relatedQueries
                .Select(x => new KeyValuePair<Relationship, QueryBase>(new Relationship(x.Key), x.Value));
            relationshipQueryCollection.AddRange(queryCollection);

            var request = new RetrieveRequest
            {
                ColumnSet = columnSet,
                Target = entityReference,
                RelatedEntitiesQuery = relationshipQueryCollection
            };

            return organizationService.Execute<RetrieveResponse>(request).Entity.ToEntity<TEntity>();
        }

        private static ColumnSet GetColumnSet(string[] columns)
        {
            return columns != null && !columns.Any()
                ? new ColumnSet(false)
                : new ColumnSet(columns);
        }

        public static TResponse Execute<TResponse>(this IOrganizationService service, OrganizationRequest request)
            where TResponse : OrganizationResponse
            => (TResponse)service.Execute(request);

        public static EntityMetadata GetEntityMetadata(this IOrganizationService organizationService,
            string logicalName,
            EntityFilters entityFilters = EntityFilters.Default, bool retrieveAsPublished = true)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = entityFilters,
                RetrieveAsIfPublished = retrieveAsPublished,
                LogicalName = logicalName
            };
            var response = organizationService.Execute<RetrieveEntityResponse>(request);
            return response.EntityMetadata;
        }

        public static EntityMetadata GetEntityMetadata(this IOrganizationService organizationService, Guid metadataId,
            EntityFilters entityFilters = EntityFilters.Default, bool retrieveAsPublished = true)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = entityFilters,
                RetrieveAsIfPublished = retrieveAsPublished,
                MetadataId = metadataId
            };
            var response = organizationService.Execute<RetrieveEntityResponse>(request);
            return response.EntityMetadata;
        }

        public static OptionSetMetadataBase GetGlobalOptionSetMetadata(this IOrganizationService organizationService,
            string logicalName, bool retrieveAsPublished = true)
        {
            var request = new RetrieveOptionSetRequest
            {
                RetrieveAsIfPublished = retrieveAsPublished,
                Name = logicalName
            };
            var response = organizationService.Execute<RetrieveOptionSetResponse>(request);
            return response.OptionSetMetadata;
        }

        public static OptionSetMetadataBase GetGlobalOptionSetMetadata(this IOrganizationService organizationService,
            Guid metadataId, bool retrieveAsPublished = true)
        {
            var request = new RetrieveOptionSetRequest
            {
                RetrieveAsIfPublished = retrieveAsPublished,
                MetadataId = metadataId
            };
            var response = organizationService.Execute<RetrieveOptionSetResponse>(request);
            return response.OptionSetMetadata;
        }

        public static TOptionSetMetadata GetGlobalOptionSetMetadata<TOptionSetMetadata>(
            this IOrganizationService organizationService, Guid metadataId, bool retrieveAsPublished = true)
            where TOptionSetMetadata : OptionSetMetadataBase
        {
            return (TOptionSetMetadata)organizationService.GetGlobalOptionSetMetadata(metadataId, retrieveAsPublished);
        }

        public static TOptionSetMetadata GetGlobalOptionSetMetadata<TOptionSetMetadata>(
            this IOrganizationService organizationService, string logicalName, bool retrieveAsPublished = true)
            where TOptionSetMetadata : OptionSetMetadataBase
        {
            return (TOptionSetMetadata)organizationService.GetGlobalOptionSetMetadata(logicalName, retrieveAsPublished);
        }

        public static AttributeMetadata GetAttributeMetadata(this IOrganizationService organizationService,
            string entityLogicalName, string logicalName, bool retrieveAsPublished)
        {
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = logicalName,
                RetrieveAsIfPublished = retrieveAsPublished,
            };
            var response = organizationService.Execute<RetrieveAttributeResponse>(request);
            return response.AttributeMetadata;
        }

        public static TAttributeMetadata GetAttributeMetadata<TAttributeMetadata>(
            this IOrganizationService organizationService, string entityLogicalName, string logicalName,
            bool retrieveAsPublished)
            where TAttributeMetadata : AttributeMetadata
        {
            return (TAttributeMetadata)organizationService.GetAttributeMetadata(entityLogicalName, logicalName,
                retrieveAsPublished);
        }

        public static AttributeMetadata GetAttributeMetadata(this IOrganizationService organizationService,
            Guid metadataId, bool retrieveAsPublished)
        {
            var request = new RetrieveAttributeRequest
            {
                MetadataId = metadataId,
                RetrieveAsIfPublished = retrieveAsPublished,
            };
            var response = organizationService.Execute<RetrieveAttributeResponse>(request);
            return response.AttributeMetadata;
        }

        public static TAttributeMetadata GetAttributeMetadata<TAttributeMetadata>(
            this IOrganizationService organizationService, Guid metadataId, bool retrieveAsPublished)
            where TAttributeMetadata : AttributeMetadata
        {
            return (TAttributeMetadata)organizationService.GetAttributeMetadata(metadataId, retrieveAsPublished);
        }

        public static WhoAmIResponse WhoAmI(this IOrganizationService organizationService)
        {
            return organizationService.Execute<WhoAmIResponse>(new WhoAmIRequest());
        }

        public static OrganizationDetail RetrieveCurrentOrganization(this IOrganizationService organizationService,
            EndpointAccessType accessType = EndpointAccessType.Default)
        {
            return organizationService.Execute<RetrieveCurrentOrganizationResponse>(
                new RetrieveCurrentOrganizationRequest
                {
                    AccessType = accessType
                }).Detail;
        }
    }
}
