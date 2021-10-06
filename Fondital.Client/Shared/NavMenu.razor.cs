﻿using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fondital.Client.Shared
{
    public partial class NavMenu
    {
        List<MenuItem> MenuItems { get; set; }
        static bool IsInternalPage(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            var protocols = new string[]
            {
                "//",
                "http://",
                "https://"
            };
            return !protocols.Any(p => url.StartsWith(p.ToLower()));
        }

        protected async Task OnClickHandler(MenuItem item)
        {
            await JSRuntime.InvokeVoidAsync("OnClickHandler", item);
        }

        protected override void OnInitialized()
        {
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    ModifyOnClick = true,
                    Text = localizer["Anagrafiche"],
                    Items = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            Text = localizer["AnagraficheSP"],
                            Url = "servicePartners"
                        },
                        new MenuItem()
                        {
                            Text = localizer["AnagraficheDifetti"],
                            Url = "defects"
                        },
                        new MenuItem()
                        {
                            Text = localizer["AnagraficheLavorazioni"],
                            Url = "processings"
                        },
                        new MenuItem()
                        {
                            Text = localizer["AnagraficheVociDiCosto"],
                            Url = "costItems"
                        },
                        new MenuItem()
                        {
                            Text = localizer["Listino"],
                            Url = "priceLists"
                        },
                         new MenuItem()
                        {
                            Text = localizer["UtenzeDirezione"],
                            Url = "utenzedirezione"
                        }
                    }
                },
                new MenuItem()
                {
                    ModifyOnClick = false,
                    Text = localizer["GestioneRapporti"],
                    Url = "reports"
                },
                new MenuItem()
                {
                    ModifyOnClick = false,
                    Text = localizer["ConfigurazioniGenerali"],
                    Url = "generalConfigurations"
                }
            };
        }
    }

    public partial class MenuItem
    {
        public bool ModifyOnClick { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<MenuItem>? Items { get; set; }
    }
}