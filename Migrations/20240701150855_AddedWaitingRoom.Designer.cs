﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using pokerapi.Models;

#nullable disable

namespace pokerapi.Migrations
{
    [DbContext(typeof(PokerContext))]
    [Migration("20240701150855_AddedWaitingRoom")]
    partial class AddedWaitingRoom
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("pokerapi.Models.Bet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CurrentAm")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalVId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TotalAm")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("Bets");
                });

            modelBuilder.Entity("pokerapi.Models.CommCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalVId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Suit")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GlobalVId");

                    b.ToTable("CommCards");
                });

            modelBuilder.Entity("pokerapi.Models.DeckCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalVId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Suit")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GlobalVId");

                    b.ToTable("DeckCards");
                });

            modelBuilder.Entity("pokerapi.Models.GlobalV", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Pot")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Round")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Showdown")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Turns")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("GlobalVs");
                });

            modelBuilder.Entity("pokerapi.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Chips")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalVId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsTurn")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ready")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Score")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GlobalVId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("pokerapi.Models.PlayerCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Suit")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerCards");
                });

            modelBuilder.Entity("pokerapi.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("pokerapi.Models.WaitingRoomPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChipsRequested")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalVId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WaitingRoomPlayers");
                });

            modelBuilder.Entity("pokerapi.Models.Bet", b =>
                {
                    b.HasOne("pokerapi.Models.Player", null)
                        .WithMany("Bets")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pokerapi.Models.CommCard", b =>
                {
                    b.HasOne("pokerapi.Models.GlobalV", null)
                        .WithMany("CommCards")
                        .HasForeignKey("GlobalVId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pokerapi.Models.DeckCard", b =>
                {
                    b.HasOne("pokerapi.Models.GlobalV", null)
                        .WithMany("DeckCards")
                        .HasForeignKey("GlobalVId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pokerapi.Models.Player", b =>
                {
                    b.HasOne("pokerapi.Models.GlobalV", null)
                        .WithMany("Players")
                        .HasForeignKey("GlobalVId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pokerapi.Models.PlayerCard", b =>
                {
                    b.HasOne("pokerapi.Models.Player", null)
                        .WithMany("PlayerCards")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pokerapi.Models.GlobalV", b =>
                {
                    b.Navigation("CommCards");

                    b.Navigation("DeckCards");

                    b.Navigation("Players");
                });

            modelBuilder.Entity("pokerapi.Models.Player", b =>
                {
                    b.Navigation("Bets");

                    b.Navigation("PlayerCards");
                });
#pragma warning restore 612, 618
        }
    }
}
