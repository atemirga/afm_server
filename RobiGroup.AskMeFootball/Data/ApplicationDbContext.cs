using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RobiGroup.AskMeFootball.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Card> Cards { get; set; }

        public DbSet<CardType> CardTypes { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }

        public DbSet<GamerCard> GamerCards { get; set; }

        public DbSet<GamerRank> GamerRanks { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<MatchGamer> MatchGamers { get; set; }

        public DbSet<MatchAnswer> MatchAnswers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(e =>
            {
                e.Property(p => p.FullName)
                    .HasComputedColumnSql("[LastName] + ' ' + [FirstName]");
                e.HasIndex(u => u.PhoneNumber).IsUnique();
            });
                

            builder.Entity<GamerCard>().HasAlternateKey(gc => new {gc.CardId, gc.GamerId});

            builder.Entity<CardType>().HasData(
                new CardType {Id = 10, Name = "Ежедневный", Code = Data.CardTypes.Daily.ToString() },
                new CardType {Id = 20, Name = "Еженедельный", Code = Data.CardTypes.Weekly.ToString() },
                new CardType {Id = 30, Name = "Ежемесячный", Code = Data.CardTypes.Monthly.ToString() });

        }
    }
}
