﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using PROCAS2.Data.Entities;

namespace PROCAS2.Data
{
    /// <summary>
    /// Recreate and re-seed the database if the model changes.
    /// </summary>
    public class PROCAS2Initialiser: System.Data.Entity.CreateDatabaseIfNotExists<PROCAS2Context>
    {
        protected override void Seed(PROCAS2Context context)
        {

            var appUsers = new List<AppUser>
            {
                new AppUser { Id = 1, UserCode = "andrew.jerrison@manchester.ac.uk", Active=true, SuperUser = true}
            };

            appUsers.ForEach(a => context.AppUsers.Add(a));
            context.SaveChanges();

            var screeningSites = new List<ScreeningSite>
            {
                new ScreeningSite { Id = 1, Code="MACC", Name="Macclesfield" },
                new ScreeningSite { Id = 2, Code="WYTH", Name="Wythenshawe" }
            };

            screeningSites.ForEach(s => context.ScreeningSites.Add(s));
            context.SaveChanges();

            var addressTypes = new List<AddressType>
            {
                new AddressType { Id = 1, Name = "Home" },
                new AddressType { Id = 2, Name = "GP"}
            };

            addressTypes.ForEach(s => context.AddressTypes.Add(s));
            context.SaveChanges();

            var eventTypes = new List<EventType>
            {
                new EventType { Id =1, Name="Participant Created", Code="001" },
                new EventType { Id =2, Name="Consent Received", Code="002" },
                new EventType { Id =3, Name="Full Detail Added", Code="003" },
                new EventType { Id =4, Name="Field Updated", Code="004" },
                new EventType { Id =5, Name="Deleted", Code="005" },
                new EventType { Id = 6, Name="Risk Letter Asked For", Code="006" }
            };

            eventTypes.ForEach(s => context.EventTypes.Add(s));
            context.SaveChanges();
        }
    }
}
