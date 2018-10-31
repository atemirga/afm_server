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
    [Migration("20181031150125_AddGameTables")]
    partial class AddGameTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
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
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

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

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PhotoUrl");

                    b.Property<int?>("RankId");

                    b.Property<int>("Score");

                    b.Property<string>("SecurityStamp");

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

                    b.HasIndex("RankId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ImageUrl");

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

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GameParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("GameId");

                    b.Property<int>("GamerId");

                    b.Property<string>("GamerId1");

                    b.Property<int>("Score");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("GamerId1");

                    b.ToTable("GameParticipant");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GamerCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<string>("GamerId")
                        .IsRequired();

                    b.Property<int>("Points");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("CardId", "GamerId");

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

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<int>("CorrectAnswerId");

                    b.Property<int>("Duration");

                    b.Property<int>("Order");

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

                    b.Property<string>("Order");

                    b.Property<int>("QuestionId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("QuestionAnswers");
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

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.Game", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany("Games")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GameParticipant", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Game", "Game")
                        .WithMany("Participants")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId1");
                });

            modelBuilder.Entity("RobiGroup.AskMeFootball.Data.GamerCard", b =>
                {
                    b.HasOne("RobiGroup.AskMeFootball.Data.Card", "Card")
                        .WithMany()
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobiGroup.AskMeFootball.Data.ApplicationUser", "Gamer")
                        .WithMany()
                        .HasForeignKey("GamerId")
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
#pragma warning restore 612, 618
        }
    }
}
