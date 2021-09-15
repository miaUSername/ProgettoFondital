﻿using Microsoft.AspNetCore.Identity;
using System;

namespace Fondital.Shared.Models.Auth
{
    public class Utente : IdentityUser<int>
    {
        public ServicePartner ServicePartner { get; set; }
        public bool IsAbilitato { get; set; } = true;
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public DateTime Pw_LastChanged { get; set; } = DateTime.Now;
        public bool Pw_MustChange { get; set; } = true;
    }
}
