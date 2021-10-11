﻿using Fondital.Shared.Enums;
using Fondital.Shared.Models;
using Fondital.Shared.Models.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fondital.Shared.Repositories
{
    public interface IRapportoRepository : IRepository<Rapporto>
    {
        Task<List<Rapporto>> GetAllRapporti();
        Task<Rapporto> GetRapportoByIdAsync(int Id);
        Task AddRapporto(Rapporto rapporto, Utente utente);
        Task AddAudit(Rapporto rapporto, Utente utente, StatoRapporto? stato = null, string note = null);
    }
}