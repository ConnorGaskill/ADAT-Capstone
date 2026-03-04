using System;
using System.Collections.Generic;
using ADAT_Project.DataAccess.EfCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Context;

public partial class ProjectDbContext : DbContext
{
    public ProjectDbContext()
    {
    }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Card> Cards { get; set; }

    public virtual DbSet<CardPrinting> CardPrintings { get; set; }

    public virtual DbSet<Cardtype> Cardtypes { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=mtg_database;Trusted_Connection=True;");
        //.EnableSensitiveDataLogging().LogTo(Console.WriteLine);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__audit_lo__5AF33E33520C6B02");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__cards__BDF201DD93F2652B");

            entity.Property(e => e.IsLegendary).HasDefaultValue(false);

            entity.HasMany(d => d.Cardtypes).WithMany(p => p.Cards)
                .UsingEntity<Dictionary<string, object>>(
                    "CardCardtype",
                    r => r.HasOne<Cardtype>().WithMany()
                        .HasForeignKey("CardtypeId")
                        .HasConstraintName("FK__card_type__type___5AEE82B9"),
                    l => l.HasOne<Card>().WithMany()
                        .HasForeignKey("CardId")
                        .HasConstraintName("FK__card_type__card___59FA5E80"),
                    j =>
                    {
                        j.HasKey("CardId", "CardtypeId").HasName("PK__card_typ__2F320184F90159ED");
                        j.ToTable("card_cardtypes");
                        j.IndexerProperty<int>("CardId").HasColumnName("card_id");
                        j.IndexerProperty<int>("CardtypeId").HasColumnName("cardtype_id");
                    });

            entity.HasMany(d => d.Colors).WithMany(p => p.Cards)
                .UsingEntity<Dictionary<string, object>>(
                    "CardColor",
                    r => r.HasOne<Color>().WithMany()
                        .HasForeignKey("ColorId")
                        .HasConstraintName("FK__card_colo__color__571DF1D5"),
                    l => l.HasOne<Card>().WithMany()
                        .HasForeignKey("CardId")
                        .HasConstraintName("FK__card_colo__card___5629CD9C"),
                    j =>
                    {
                        j.HasKey("CardId", "ColorId").HasName("PK__card_col__1CE63D317B235600");
                        j.ToTable("card_colors");
                        j.IndexerProperty<int>("CardId").HasColumnName("card_id");
                        j.IndexerProperty<int>("ColorId").HasColumnName("color_id");
                    });
        });

        modelBuilder.Entity<CardPrinting>(entity =>
        {
            entity.HasKey(e => e.PrintingId).HasName("PK__card_pri__15FFEFA27DBF2B4F");

            entity.HasOne(d => d.Card).WithMany(p => p.CardPrintings).HasConstraintName("FK__card_prin__card___5DCAEF64");

            entity.HasOne(d => d.Set).WithMany(p => p.CardPrintings).HasConstraintName("FK__card_prin__set_i__5EBF139D");
        });

        modelBuilder.Entity<Cardtype>(entity =>
        {
            entity.HasKey(e => e.CardtypeId).HasName("PK__types__2C0005983128B7D4");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__colors__1143CECB01BE8A19");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => e.SetId).HasName("PK__sets__14B092A38E8A0E17");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
