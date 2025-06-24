using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using woww.Models;

namespace woww.Context;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Animal> Animals { get; set; }

    public virtual DbSet<AnimalType> AnimalTypes { get; set; }

    public virtual DbSet<AnimalView> AnimalViews { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Voiler> Voilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_catalog", "adminpack");

        modelBuilder.Entity<Animal>(entity =>
        {
            entity.HasKey(e => e.AnimalId).HasName("Animals_pkey");

            entity.ToTable("Animals", "zoo");

            entity.Property(e => e.AnimalId).HasColumnName("Animal_ID");
            entity.Property(e => e.AnimalTypeId).HasColumnName("AnimalType_ID");
            entity.Property(e => e.AnimalViewId).HasColumnName("AnimalView_ID");
            entity.Property(e => e.GenderId).HasColumnName("Gender_ID");
            entity.Property(e => e.IsHungry).HasDefaultValueSql("true");
            entity.Property(e => e.VolierId).HasColumnName("Volier_ID");

            entity.HasOne(d => d.AnimalType).WithMany(p => p.Animals)
                .HasForeignKey(d => d.AnimalTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Animals_AnimalType_ID_fkey");

            entity.HasOne(d => d.AnimalView).WithMany(p => p.Animals)
                .HasForeignKey(d => d.AnimalViewId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Animals_AnimalView_ID_fkey");

            entity.HasOne(d => d.Gender).WithMany(p => p.Animals)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Animals_Gender_ID_fkey");

            entity.HasOne(d => d.Volier).WithMany(p => p.Animals)
                .HasForeignKey(d => d.VolierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Animals_Volier_ID_fkey");
        });

        modelBuilder.Entity<AnimalType>(entity =>
        {
            entity.HasKey(e => e.AnimalTypeId).HasName("AnimalType_pkey");

            entity.ToTable("AnimalType", "zoo");

            entity.Property(e => e.AnimalTypeId).HasColumnName("AnimalType_ID");
        });

        modelBuilder.Entity<AnimalView>(entity =>
        {
            entity.HasKey(e => e.AnimalViewId).HasName("AnimalView_pkey");

            entity.ToTable("AnimalView", "zoo");

            entity.Property(e => e.AnimalViewId).HasColumnName("AnimalView_ID");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderId).HasName("Gender_pkey");

            entity.ToTable("Gender", "zoo");

            entity.Property(e => e.GenderId).HasColumnName("Gender_ID");
            entity.Property(e => e.Gender1).HasColumnName("Gender");
        });

        modelBuilder.Entity<Voiler>(entity =>
        {
            entity.HasKey(e => e.VoilerId).HasName("Voiler_pkey");

            entity.ToTable("Voiler", "zoo");

            entity.Property(e => e.VoilerId).HasColumnName("Voiler_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
