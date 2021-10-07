﻿using Fondital.Data;
using Fondital.Shared.Models.Auth;
using Fondital.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fondital.Repository
{
    public class UtenteRepository : Repository<Utente>, IUtenteRepository
    {
        public UtenteRepository(FonditalDbContext context)
            : base(context)
        { }

        private FonditalDbContext Db
        {
            get { return Context as FonditalDbContext; }
        }

        public async Task<Utente> GetByUsernameAsync(string username)
        {
            return await Db.Utenti.SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task CreateUtente(Utente utente)
        {
            await Db.Utenti.AddAsync(utente);
            Db.Entry(utente.ServicePartner).State = EntityState.Unchanged;
        }

        public async Task<IEnumerable<Utente>> GetAllUtenti(bool? isDirezione)
        {
            if (isDirezione == true)
                return await Db.Utenti.Where(x => x.ServicePartner == null).ToListAsync(); 
            else if (isDirezione == false)
                return await Db.Utenti.Where(x => x.ServicePartner != null).ToListAsync();
            else
                return await Db.Utenti.ToListAsync();
        }
    }
}