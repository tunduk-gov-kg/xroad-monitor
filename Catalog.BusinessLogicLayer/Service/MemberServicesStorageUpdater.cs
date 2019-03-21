﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Catalog.BusinessLogicLayer.Service.Interfaces;
using Catalog.DataAccessLayer;
using Catalog.DataAccessLayer.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using XRoad.Domain;

namespace Catalog.BusinessLogicLayer.Service {
    public class MemberServicesStorageUpdater : IXRoadStorageUpdater<ServiceIdentifier> {
        private readonly AppDbContext _dbContext;

        public MemberServicesStorageUpdater(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task UpdateLocalDatabaseAsync(IImmutableList<ServiceIdentifier> updatedList) {
            ValidateList(updatedList);

            var databaseServicesList = _dbContext.MemberServices
                .Include(service => service.Member)
                .ToImmutableList();

            CreateCompletelyNewServices(updatedList, databaseServicesList);
            RemoveNonExistingServices(updatedList, databaseServicesList);
            RestorePreviouslyRemovedServices(updatedList, databaseServicesList);

            await _dbContext.SaveChangesAsync();
        }

        public void Dispose() {
            _dbContext.Dispose();
        }

        private void ValidateList(IImmutableList<ServiceIdentifier> updatedList) {
            var allSubSystemsAreNull = updatedList.All(identifier => string.IsNullOrEmpty(identifier.SubSystemCode));
            if (!allSubSystemsAreNull) throw new ArgumentException(nameof(updatedList));
        }

        private void RestorePreviouslyRemovedServices(IImmutableList<ServiceIdentifier> updatedList,
            ImmutableList<MemberService> databaseServicesList) {
            foreach (var databaseService in databaseServicesList) {
                if (!databaseService.IsDeleted) continue;
                var storedInUpdatedList = updatedList.Any(identifier => Equals(databaseService, identifier));
                if (!storedInUpdatedList) continue;
                databaseService.IsDeleted = false;
                databaseService.ModificationDateTime = DateTime.Now;
                _dbContext.MemberServices.Update(databaseService);
            }
        }

        private void RemoveNonExistingServices(IImmutableList<ServiceIdentifier> updatedList,
            ImmutableList<MemberService> databaseServicesList) {
            foreach (var databaseService in databaseServicesList) {
                var storedInUpdatedList = updatedList.Any(service => Equals(databaseService, service));
                if (storedInUpdatedList) continue;
                databaseService.IsDeleted = true;
                databaseService.ModificationDateTime = DateTime.Now;
                _dbContext.MemberServices.Update(databaseService);
            }
        }

        private void CreateCompletelyNewServices(IImmutableList<ServiceIdentifier> updatedList,
            ImmutableList<MemberService> databaseServicesList) {
            foreach (var incomingService in updatedList) {
                var storedInDatabase = databaseServicesList.Any(service => Equals(service, incomingService));

                if (storedInDatabase) continue;

                var isContextMember = new Predicate<Member>(member =>
                    member.Instance.Equals(incomingService.Instance)
                    && member.MemberClass.Equals(incomingService.MemberClass)
                    && member.MemberCode.Equals(incomingService.MemberCode));

                var contextMember = _dbContext.Members.FirstOrDefault(member => isContextMember(member));

                if (contextMember == null) continue;

                var completelyNewService = new MemberService {
                    Member = contextMember,
                    ServiceCode = incomingService.ServiceCode,
                    ServiceVersion = incomingService.ServiceVersion
                };
                _dbContext.MemberServices.Add(completelyNewService);
            }
        }

        private bool Equals(MemberService memberService, ServiceIdentifier serviceIdentifier) {
            var expression = memberService.ServiceCode.Equals(serviceIdentifier.ServiceCode)
                && memberService.Member.Instance.Equals(serviceIdentifier.Instance)
                && memberService.Member.MemberClass.Equals(serviceIdentifier.MemberClass)
                && memberService.Member.MemberCode.Equals(serviceIdentifier.MemberCode);

            if (memberService.ServiceVersion == null) return expression && serviceIdentifier.ServiceVersion == null;

            return expression && memberService.ServiceVersion.Equals(serviceIdentifier.ServiceVersion);
        }
    }
}