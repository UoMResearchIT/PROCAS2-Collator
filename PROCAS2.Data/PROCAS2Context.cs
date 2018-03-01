using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.AspNet.Identity.EntityFramework;

using PROCAS2.Data.Identity;
using PROCAS2.Data.Entities;

namespace PROCAS2.Data
{
    public class PROCAS2Context : DbContext
    {
        public PROCAS2Context() : base("PROCAS2Connection")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<PROCAS2Context, PROCAS2.Data.Migrations.Configuration>("PROCAS2Connection"));

        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressType> AddressTypes { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<GeneticRecord> GeneticRecords { get; set; }
        public DbSet<GeneticRecordItem> GeneticRecordItems { get; set; }
        public DbSet<Histology> Histologies { get; set; }
        public DbSet<HistologyLookup> HistologyLookups { get; set; }
        public DbSet<HistologyFocus> HistologyFoci { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<ParticipantEvent> ParticipantEvents { get; set; }
       public DbSet<Question> Questions { get; set; }
       public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
       public DbSet<QuestionnaireResponseItem> QuestionnaireResponseItems { get; set; }
        public DbSet<RiskLetter> RiskLetters { get; set; }
        public DbSet<ScreeningSite> ScreeningSites { get; set; }
        public DbSet<ScreeningRecordV1_5_4> ScreeningRecordV1_5_4s { get; set; }
        public DbSet<QuestionnaireResponse> QuestionnaireResponses { get; set; }
        public DbSet<FamilyHistoryItem> FamilyHistoryItems { get; set; }
        public DbSet<AppNewsItem> AppNewsItems { get; set; }
        public DbSet<WebJobLog> WebJobLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<ParticipantEvent>()
                .HasRequired(c => c.Participant)
                .WithMany(a => a.ParticipantEvents)
                .HasForeignKey(x => x.ParticipantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ParticipantEvent>()
            .HasRequired(n => n.AppUser)
            .WithMany(a => a.ParticipantEvents)
            .HasForeignKey(n => n.AppUserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<Image>()
            .HasOptional(p => p.ScreeningRecordV1_5_4)
            .WithOptionalPrincipal(o => o.Image)
            .Map(x => x.MapKey("ImageId"));

        }
    
    }
}
