﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data;

#nullable disable

namespace Migrations
{
    [DbContext(typeof(SnitzDbContext))]
    [Migration("20240314134512_SnitzEvents")]
    partial class SnitzEvents
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SnitzEvents.Models.CalendarEventItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("C_ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Author")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("End")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("EVENT_ENDDATE");

                    b.Property<DateTime?>("EndDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsAllDayEvent")
                        .HasColumnType("bit")
                        .HasColumnName("EVENT_ALLDAY");

                    b.Property<string>("RecurDays")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("EVENT_DAYS");

                    b.Property<int>("Recurs")
                        .HasColumnType("int")
                        .HasColumnName("EVENT_RECURS");

                    b.Property<string>("Start")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("EVENT_DATE");

                    b.Property<DateTime?>("StartDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TopicId")
                        .HasColumnType("int")
                        .HasColumnName("TOPIC_ID");

                    b.HasKey("Id");

                    b.ToTable("CAL_EVENTS");
                });

            modelBuilder.Entity("SnitzEvents.Models.ClubCalendarCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("CAT_ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CAT_NAME");

                    b.Property<int>("Order")
                        .HasColumnType("int")
                        .HasColumnName("CAT_ORDER");

                    b.HasKey("Id");

                    b.ToTable("EVENT_CAT");
                });

            modelBuilder.Entity("SnitzEvents.Models.ClubCalendarClub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("CLUB_ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CLUB_ABBR");

                    b.Property<int>("DefLocId")
                        .HasColumnType("int")
                        .HasColumnName("CLUB_DEF_LOC");

                    b.Property<string>("LongName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CLUB_L_NAME");

                    b.Property<int>("Order")
                        .HasColumnType("int")
                        .HasColumnName("CLUB_ORDER");

                    b.Property<string>("ShortName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CLUB_S_NAME");

                    b.HasKey("Id");

                    b.ToTable("EVENT_CLUB");
                });

            modelBuilder.Entity("SnitzEvents.Models.ClubCalendarLocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("LOC_ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("LOC_NAME");

                    b.Property<int>("Order")
                        .HasColumnType("int")
                        .HasColumnName("LOC_ORDER");

                    b.HasKey("Id");

                    b.ToTable("EVENT_LOCATION");
                });

            modelBuilder.Entity("SnitzEvents.Models.ClubCalendarSubscriptions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("SUB_ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClubId")
                        .HasColumnType("int")
                        .HasColumnName("CLUB_ID");

                    b.Property<int>("MemberId")
                        .HasColumnType("int")
                        .HasColumnName("MEMBER_ID");

                    b.HasKey("Id");

                    b.ToTable("EVENT_SUBSCRIPTIONS");
                });


#pragma warning restore 612, 618
        }
    }
}