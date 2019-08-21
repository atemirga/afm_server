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

        public DbSet<CardWinner> CardWinners { get; set; }

        public DbSet<UserCoins> UserCoins { get; set; }

        public DbSet<PointHistories> PointHistories { get; set; }

        public DbSet<Prize> Prizes { get; set; }

        public DbSet<PrizeBuyHistory> PrizeBuyHistories { get; set; }

        public DbSet<GetContact> GetContacts { get; set; }

        public DbSet<ReferralUser> ReferralUsers { get; set; }

        public DbSet<Friend> Friends { get; set; } 

        public DbSet<MatchBid> MatchBids { get; set; }

        public DbSet<CardInfo> CardInfos { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<Version> Versions { get; set; }

        public DbSet<InfoCard> InfoCards { get; set; }

        public DbSet<InfoCardImage> InfoCardImages { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketCategory> TicketCategories { get; set; }

        public DbSet<TicketAttachment> TicketAttachments { get; set; }

        public DbSet<CardTeams> CardTeams { get; set; }

        public DbSet<QuestionBox> QuestionBoxes { get; set; }

        public DbSet<UserBalance> UserBalances { get; set; }

        public DbSet<CashOutHistory> CashOutHistories { get; set; }

        public DbSet<CardLimits> CardLimits { get; set; }

        public DbSet<PackPrice> PackPrices { get; set; }

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
                e.Property(u => u.PhoneNumber).IsRequired();
            });
                
            builder.Entity<CardType>().HasData(
                new CardType {Id = 10, Name = "Ежедневный", Code = Data.CardTypes.Daily.ToString() },
                new CardType {Id = 20, Name = "Еженедельный", Code = Data.CardTypes.Weekly.ToString() },
                new CardType {Id = 30, Name = "Ежемесячный", Code = Data.CardTypes.Monthly.ToString() },
                new CardType { Id = 40, Name = "Live", Code = "Live" },
                new CardType { Id = 50, Name = "HalfTime", Code = "HalfTime"});

            builder.Entity<MatchAnswer>().HasAlternateKey(a => new { a.MatchGamerId, a.QuestionId });

            builder.Entity<Match>().Property(m => m.Status).HasConversion<byte>();

        }
    }
}
