﻿using Fondital.Shared.Dto;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Fondital.Client.Dialogs
{
    public partial class AddDifettoDialog
    {
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        protected DifettoDto NuovoDifetto { get; set; } = new DifettoDto();
        protected bool isSubmitting = false;
        protected string ErrorMessage = "";

        protected async Task SalvaDifetto()
        {
            isSubmitting = true;
            ErrorMessage = "";

            try
            {
                await httpClient.CreateDifetto(NuovoDifetto);
                isSubmitting = false;
                await OnSave.InvokeAsync();
            }
            catch (Exception ex)
            {
                isSubmitting = false;
                ErrorMessage = localizer[ex.Message];
            }
        }
    }
}
