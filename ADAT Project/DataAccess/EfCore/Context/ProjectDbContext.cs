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
        => optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=mtg_database;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AUDIT LOG
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("sysutcdatetime()");
        });

        // CARD
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.Property(e => e.IsLegendary)
                  .HasDefaultValue(false);

            // Index (search by name)
            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("IX_cards_name");

            // Many-to-many: Card <-> Cardtype
            entity.HasMany(d => d.Cardtypes)
                .WithMany(p => p.Cards)
                .UsingEntity<Dictionary<string, object>>(
                    "CardCardtype",
                    r => r.HasOne<Cardtype>()
                          .WithMany()
                          .HasForeignKey("CardtypeId")
                          .OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Card>()
                          .WithMany()
                          .HasForeignKey("CardId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.ToTable("card_cardtypes");

                        j.HasKey("CardId", "CardtypeId");

                        j.IndexerProperty<int>("CardId")
                         .HasColumnName("card_id");

                        j.IndexerProperty<int>("CardtypeId")
                         .HasColumnName("cardtype_id");

                        // Reverse index
                        j.HasIndex("CardtypeId")
                         .HasDatabaseName("IX_card_cardtypes_cardtype_id");
                    });

            // Many-to-many: Card <-> Color
            entity.HasMany(d => d.Colors)
                .WithMany(p => p.Cards)
                .UsingEntity<Dictionary<string, object>>(
                    "CardColor",
                    r => r.HasOne<Color>()
                          .WithMany()
                          .HasForeignKey("ColorId")
                          .OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Card>()
                          .WithMany()
                          .HasForeignKey("CardId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.ToTable("card_colors");

                        j.HasKey("CardId", "ColorId");

                        j.IndexerProperty<int>("CardId")
                         .HasColumnName("card_id");

                        j.IndexerProperty<int>("ColorId")
                         .HasColumnName("color_id");

                        // Reverse index
                        j.HasIndex("ColorId")
                         .HasDatabaseName("IX_card_colors_color_id");
                    });
        });

        // CARD PRINTINGS
        modelBuilder.Entity<CardPrinting>(entity =>
        {
            entity.HasKey(e => e.PrintingId);

            entity.HasOne(d => d.Card)
                  .WithMany(p => p.CardPrintings)
                  .HasForeignKey(d => d.CardId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Set)
                  .WithMany(p => p.CardPrintings)
                  .HasForeignKey(d => d.SetId)
                  .OnDelete(DeleteBehavior.Cascade);

            // FK indexes
            entity.HasIndex(e => e.CardId)
                  .HasDatabaseName("IX_card_printings_card_id");

            entity.HasIndex(e => e.SetId)
                  .HasDatabaseName("IX_card_printings_set_id");
        });

        // CARDTYPE
        modelBuilder.Entity<Cardtype>(entity =>
        {
            entity.HasKey(e => e.CardtypeId);

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("UQ_types_name");
        });

        // COLOR
        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId);

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("UQ_colors_name");
        });

        // SET
        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => e.SetId);

            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("UQ_sets_code");
        });
    }
}