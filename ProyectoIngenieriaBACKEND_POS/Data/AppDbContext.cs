using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;

namespace ProyectoIngenieriaBACKEND_POS.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<DuplecateReference> DuplecateReferences { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=SinpeBridge.mssql.somee.com;Database=SinpeBridge;User Id=sinpe_login;Password=Xb5@nT1$wJ8&pYc;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DuplecateReference>(entity =>
        {
            entity.Property(e => e.Cellphone).HasColumnName("cellphone");
            entity.Property(e => e.IdClient).HasColumnName("idClient");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.DuplecateReferences)
                .HasForeignKey(d => d.IdClient)
                .HasConstraintName("FK_DuplecateReferences_Clients");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.ClientId, "IX_Payments_ClientId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.Payments).HasForeignKey(d => d.ClientId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
