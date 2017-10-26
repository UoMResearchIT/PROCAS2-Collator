using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using PROCAS2.Data.Entities;

namespace PROCAS2.Data
{
    public class PROCAS2Context : DbContext
    {
        public PROCAS2Context() : base("PROCAS2Connection")
        {
           
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Histology> Histologies { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<ParticipantEvent> ParticipantEvents { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<ScreeningSite> ScreeningSites { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
