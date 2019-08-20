﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Senko.Bot.Data;

namespace Senko.Bot.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20190820160441_AddUserExperience")]
    partial class AddUserExperience
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Senko.Bot.Data.Entities.Setting", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("GuildId", "Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Senko.Commands.Entities.ChannelPermission", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("ChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("Granted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("GuildId", "ChannelId");

                    b.HasAlternateKey("GuildId", "ChannelId", "Name");

                    b.ToTable("ChannelPermission");
                });

            modelBuilder.Entity("Senko.Commands.Entities.GuildModule", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("Enabled");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("GuildId");

                    b.HasAlternateKey("GuildId", "Name");

                    b.ToTable("GuildModule");
                });

            modelBuilder.Entity("Senko.Commands.Entities.RolePermission", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("RoleId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("Granted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("GuildId", "RoleId");

                    b.HasAlternateKey("GuildId", "RoleId", "Name");

                    b.ToTable("RolePermission");
                });

            modelBuilder.Entity("Senko.Commands.Entities.UserPermission", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("Granted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("GuildId", "UserId");

                    b.HasAlternateKey("GuildId", "UserId", "Name");

                    b.ToTable("UserPermission");
                });

            modelBuilder.Entity("Senko.Modules.Levels.Data.Entities.UserExperience", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<long>("Experience");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("UserExperience");
                });

            modelBuilder.Entity("Senko.Modules.Moderation.Data.Entities.UserWarning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal?>("ConsoleChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal?>("ConsoleMessageId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<DateTime>("Created");

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("ModeratorId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Reason");

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.ToTable("UserWarning");
                });
#pragma warning restore 612, 618
        }
    }
}
