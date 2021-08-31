﻿using Fondital.Shared.Enums;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Fondital.Client.Shared
{
    public partial class CultureSelector
    {
        public static IEnumerable<string> SupportedLanguages { get => EnumExtensions.GetEnumNames<Lingua>(); }
        private string _currentLang { get; set; }

        public string CurrentLang
        {
            get => _currentLang;
            set
            {
                _currentLang = value;

                var curLang = EnumExtensions.GetEnumValues<Lingua>().FirstOrDefault(l => l.ToString() == value);

                if (CultureInfo.CurrentCulture.Name != curLang.Description())
                {
                    var js = (IJSInProcessRuntime)JSRuntime;
                    js.InvokeVoid("blazorCulture.set", curLang.Description());

                    Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
                }
            }
        }

        protected override void OnInitialized()
        {
            var region = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            _currentLang = region.TwoLetterISORegionName;
        }
    }
}
