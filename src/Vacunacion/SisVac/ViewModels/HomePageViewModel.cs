﻿using System;
using Prism.Navigation;
using Prism.Services;
using SisVac.Framework.Domain;
using SisVac.Framework.Services;

namespace SisVac.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        ICacheService _cacheService;
        public HomePageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService)
        {
            _cacheService = cacheService;
            Init();
        }

       
        public new ApplicationUser User { get; set; }
        public new ApplicationUser Vaccionator { get; set; }

        private async void Init()
        {
            User = await _cacheService.GetLocalObject<ApplicationUser>(CacheKeyDictionary.UserInfo);
            Vaccionator = await _cacheService.GetLocalObject<ApplicationUser>(CacheKeyDictionary.VaccinatorInfo);
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            //if (parameters.ContainsKey("user"))
            //    User = parameters.GetValue<ApplicationUser>("user");
        }
    }
}
