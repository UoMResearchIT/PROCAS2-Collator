using System;
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
            var screeningSites = new List<ScreeningSite>
            {
                new ScreeningSite { Id = 1, Code="MACC", Name="Macclesfield" },
                new ScreeningSite { Id = 2, Code="WYTH", Name="Wythenshawe" }
            };

            screeningSites.ForEach(s => context.ScreeningSites.Add(s));
            context.SaveChanges();
        }
    }
}
