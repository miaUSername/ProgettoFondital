﻿using Fondital.Shared.Enums;
using Fondital.Shared.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Fondital.Client
{
    public class FonditalAuthenticationState : RemoteAuthenticationState
    {
        public Utente UtenteCorrente { get; set; }
    }
}
