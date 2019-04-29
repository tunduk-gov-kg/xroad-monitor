using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Catalog.BusinessLogicLayer.Service.Interfaces;
using Microsoft.Extensions.Logging;
using SimpleSOAPClient.Exceptions;
using XRoad.Domain;

namespace Catalog.BusinessLogicLayer.Service
{
    public class XRoadMetadataCollector
    {
        private readonly IXRoadGlobalConfigurationClient _configurationClient;
        private readonly ILogger<XRoadMetadataCollector> _logger;
        private readonly IXRoadStorageUpdater<MemberData> _membersStorage;
        private readonly IXRoadStorageUpdater<SecurityServerData> _serversStorageUpdater;
        private readonly ServicesStorageUpdater _servicesStorage;
        private readonly IXRoadStorageUpdater<SubSystemIdentifier> _subSystemsStorage;

        public XRoadMetadataCollector(IXRoadGlobalConfigurationClient configurationClient
            , IXRoadStorageUpdater<MemberData> membersStorage
            , IXRoadStorageUpdater<SecurityServerData> serversStorageUpdater
            , IXRoadStorageUpdater<SubSystemIdentifier> subSystemsStorage
            , ServicesStorageUpdater servicesStorage
            , ILogger<XRoadMetadataCollector> logger)
        {
            _configurationClient = configurationClient;
            _membersStorage = membersStorage;
            _serversStorageUpdater = serversStorageUpdater;
            _subSystemsStorage = subSystemsStorage;
            _servicesStorage = servicesStorage;
            _logger = logger;
        }


        public async Task RunBatchUpdateTask()
        {
            var memberDataRecords = await _configurationClient.GetMembersListAsync();
            await _membersStorage.UpdateLocalDatabaseAsync(memberDataRecords);

            var securityServerDataRecords = await _configurationClient.GetSecurityServersListAsync();
            await _serversStorageUpdater.UpdateLocalDatabaseAsync(securityServerDataRecords);

            var subSystemIdentifiers = await _configurationClient.GetSubSystemsListAsync();
            await _subSystemsStorage.UpdateLocalDatabaseAsync(subSystemIdentifiers);

            var servicesList = await _configurationClient.GetServicesListAsync();

            var containsSubSystemCode =
                new Predicate<ServiceIdentifier>(identifier => !string.IsNullOrEmpty(identifier.SubSystemCode));

            var subSystemServicesList = servicesList.Where(service => containsSubSystemCode(service)).ToImmutableList();
            await _servicesStorage.UpdateLocalDatabaseAsync(subSystemServicesList);

            await UpdateServicesWsdl(servicesList);
        }

        public async Task RunWsdlUpdateTask(ServiceIdentifier targetService)
        {
            try
            {
                var wsdl = await _configurationClient.GetWsdlAsync(targetService);
                await _servicesStorage.UpdateWsdlAsync(targetService, wsdl);
            }
            catch (FaultException exception)
            {
                _logger.LogError(LoggingEvents.UpdateWsdlTask, "" +
                                                               "Error occurred during wsdl update task for service: {service}; " +
                                                               "Server responded with message: {message}",
                    targetService.ToString(),
                    exception.String
                );
            }
        }

        private async Task UpdateServicesWsdl(IList<ServiceIdentifier> subSystemServicesList)
        {
            _logger.LogInformation("Update wsdl task started.");
            foreach (var serviceIdentifier in subSystemServicesList.AsParallel())
                await RunWsdlUpdateTask(serviceIdentifier);
            _logger.LogInformation("Update wsdl task finished.");
        }
    }
}