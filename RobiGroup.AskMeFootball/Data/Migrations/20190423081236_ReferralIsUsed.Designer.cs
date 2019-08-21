﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190423081236_ReferralIsUsed")]
    partial class ReferralIsUsed
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(128);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<int>("Bot");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("FullName")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql("[LastName] + ' ' + [FirstName]");

                    b.Property<string>("Lang");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NickName");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("OneSignalId");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber")
                        .IsRequired();

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PhotoUrl");

                    b.Property<int>("PointsToPlay");

                    b.Property<int?>("RankId");

                    b.Property<string>("Referral");

                    b.Property<bool>("ReferralUsed");

                    b.Property<DateTime>("RegisteredDate");

                    b.Property<int>("Score");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("Sync");

                    b.Property<int>("TotalScore");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.HasIndex("RankId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ImageUrlCard");

                    b.Property<string>("ImageUrlDetail");

                    b.Property<string>("ImageUrlSelect");

                    b.Property<bool>("IsHalfH");

                    b.Property<bool>("IsTwoH");

                    b.Property<int>("MatchQuestions");

                    b.Property<string>("Name");

                    b.Property<string>("Prize");

                    b.Property<int>("ResetPeriod");

                    b.Property<DateTime>("ResetTime");

                    b.Property<int>("TypeId");

                    b.HasKey("Id");

                    b.HasIndex("TypeId");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.CardType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("CardTypes");

                    b.HasData(
                        new { Id = 10, Code = "Daily", Name = "Ежедневный" },
                        new { Id = 20, Code = "Weekly", Name = "Еженедельный" },
                        new { Id = 30, Code = "Monthly", Name = "Ежемесячный" }
                    );
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.CardWinner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CardEndTime");

                    b.Property<int>("CardId");

                    b.Property<DateTime>("CardStartTime");

                    b.Property<int>("GamerCardScore");

                    b.Property<string>("GamerId");

                    b.Property<string>("Prize");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.HasIndex("GamerId");

                    b.ToTable("CardWinners");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GamerCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<DateTime?>("EndTime");

                    b.Property<string>("GamerId");

                    b.Property<bool>("IsActive");

                    b.Property<int>("Score");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.HasIndex("GamerId");

                    b.ToTable("GamerCards");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GamerRank", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("GamerRanks");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Questions");

                    b.Property<DateTime?>("StartTime");

                    b.Property<byte>("Status");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.MatchAnswer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AnswerId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("IsCorrectAnswer");

                    b.Property<int>("MatchGamerId");

                    b.Property<int>("QuestionId");

                    b.HasKey("Id");

                    b.HasAlternateKey("MatchGamerId", "QuestionId");

                    b.HasIndex("AnswerId");

                    b.HasIndex("QuestionId");

                    b.ToTable("MatchAnswers");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.MatchGamer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Bonus");

                    b.Property<bool>("Cancelled");

                    b.Property<bool>("Confirmed");

                    b.Property<bool>("Delayed");

                    b.Property<int>("GamerCardId");

                    b.Property<string>("GamerId");

                    b.Property<bool>("IsPlay");

                    b.Property<bool>("IsWinner");

                    b.Property<DateTime?>("JoinTime");

                    b.Property<int>("MatchId");

                    b.Property<bool>("Ready");

                    b.Property<int>("Score");

                    b.HasKey("Id");

                    b.HasIndex("GamerCardId");

                    b.HasIndex("GamerId");

                    b.HasIndex("MatchId");

                    b.ToTable("MatchGamers");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.PointHistories", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GamerId");

                    b.Property<int>("Point");

                    b.Property<DateTime>("TimeAdded");

                    b.HasKey("Id");

                    b.ToTable("PointHistories");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Prize", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<string>("Facebook");

                    b.Property<string>("FirstPhoneNumber");

                    b.Property<string>("FirstPhotoUrl");

                    b.Property<int>("InStock");

                    b.Property<string>("Instagram");

                    b.Property<string>("Name");

                    b.Property<int>("Price");

                    b.Property<string>("SecondPhotoUrl");

                    b.Property<string>("ThirdPhotoUrl");

                    b.Property<string>("Twitter");

                    b.Property<string>("Vkontakte");

                    b.HasKey("Id");

                    b.ToTable("Prizes");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.PrizeBuyHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("BuyDate");

                    b.Property<string>("GamerId");

                    b.Property<int>("Price");

                    b.Property<int>("PrizeId");

                    b.HasKey("Id");

                    b.ToTable("PrizeBuyHistories");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<int>("CorrectAnswerId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.QuestionAnswer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Order");

                    b.Property<int>("QuestionId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("QuestionAnswers");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.UserCoins", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Coins");

                    b.Property<string>("GamerId");

                    b.Property<DateTime>("LastUpdate");

                    b.HasKey("Id");

                    b.HasIndex("GamerId");

                    b.ToTable("UserCoins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.ApplicationUser", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.GamerRank", "Rank")
                        .WithMany()
                        .HasForeignKey("RankId");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Card", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.CardType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.CardWinner", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany()
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GamerCard", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany("GamerCards")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Match", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany("Matches")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.MatchAnswer", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.QuestionAnswer", "Answer")
                        .WithMany()
                        .HasForeignKey("AnswerId");

                    b.HasOne("RobiGroup.AskMeFootball.Data.MatchGamer", "MatchGamer")
                        .WithMany("Answers")
                        .HasForeignKey("MatchGamerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.MatchGamer", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.GamerCard", "GamerCard")
                        .WithMany()
                        .HasForeignKey("GamerCardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId");

                    b.HasOne("RobiGroup.AskMeFootball.Data.Match", "Match")
                        .WithMany("Gamers")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Question", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany("Questions")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.QuestionAnswer", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Question", "Question")
                        .WithMany("Answers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.UserCoins", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId");
                });
#pragma warning restore 612, 618
        }
    }
}
