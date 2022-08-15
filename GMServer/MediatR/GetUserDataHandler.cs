﻿using GMServer.Context;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using SRC.DataFiles.Cache;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class GetUserDataRequest : IRequest<GetUserDataResponse>
    {
        public string UserID;
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;
    }

    public class GetUserDataResponse
    {
        public List<UserArtefact> Artefacts;
        public List<UserArmouryItem> ArmouryItems;
        public UserCurrencies Currencies;
        public List<UserMerc> UnlockedMercs;
        public UserBounties Bounties;
        public UserQuests Quests;
        public LifetimeUserAccountStats LifetimeStats;
    }

    public class GetUserDataHandler : IRequestHandler<GetUserDataRequest, GetUserDataResponse>
    {
        private readonly ArtefactsService _artefacts;
        private readonly ArmouryService _armoury;
        private readonly QuestsService _quests;
        private readonly CurrenciesService _currencies;
        private readonly MercsService _mercs;
        private readonly AccountStatsService _stats;
        private readonly IBountiesService _bounties;
        private readonly IDataFileCache _dataFiles;

        public GetUserDataHandler(
            ArtefactsService artefacts,
            ArmouryService armoury,
            CurrenciesService currencies,
            MercsService mercs,
            IBountiesService bounties,
            QuestsService quests,
            AccountStatsService stats,
            IDataFileCache dataFiles)
        {
            _artefacts = artefacts;
            _armoury = armoury;
            _quests = quests;
            _currencies = currencies;
            _mercs = mercs;
            _stats = stats;
            _bounties = bounties;
            _dataFiles = dataFiles;
        }

        public async Task<GetUserDataResponse> Handle(GetUserDataRequest request, CancellationToken cancellationToken)
        {
            GetUserDataResponse response = new()
            {
                Artefacts = await _artefacts.GetUserArtefactsAsync(request.UserID),
                ArmouryItems = await _armoury.GetUserArmouryItemsAsync(request.UserID),
                Currencies = await _currencies.GetUserCurrenciesAsync(request.UserID),
                UnlockedMercs = await _mercs.GetUserMercsAsync(request.UserID),
                Bounties = await _bounties.GetUserBountiesAsync(request.UserID),
                Quests = new()
                {
                    DateTime = DateTime.UtcNow,
                    CompletedDailyQuests = await _quests.GetCompletedDailyQuestsAsync(request.UserID, request.DailyRefresh),
                    CompletedMercQuests = await _quests.GetCompletedMercQuestsAsync(request.UserID),
                    Quests = _dataFiles.Quests
                },
                LifetimeStats = await _stats.GetUserLifetimeStatsAsync(request.UserID)
            };

            return response;
        }
    }
}
