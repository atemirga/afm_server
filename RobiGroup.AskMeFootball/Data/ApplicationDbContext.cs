﻿using System;
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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GamerCard>().HasAlternateKey(gc => new {gc.CardId, gc.GamerId});

            builder.Entity<CardType>().HasData(
                new CardType {Id = 10, Name = "Ежедневный", Code = "Daily"},
                new CardType {Id = 20, Name = "Еженедельный", Code = "Weekly"},
                new CardType {Id = 30, Name = "Ежемесячный", Code = "Monthly"});

        }
    }
}
