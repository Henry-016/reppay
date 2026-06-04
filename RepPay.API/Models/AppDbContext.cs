using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RepPay.API.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Despesa> Despesas { get; set; }

    public virtual DbSet<Grupo> Grupos { get; set; }

    public virtual DbSet<Parcela> Parcelas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<CodigoRecuperacao> CodigosRecuperacao { get; set; }

    public virtual DbSet<Pertence> Pertences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("status_despesa", new[] { "ATIVA", "QUITADA", "CANCELADA" })
            .HasPostgresEnum("status_parcela", new[] { "PENDENTE", "PAGO", "ATRASADO" });

        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => e.IdDespesa).HasName("despesa_pkey");

            entity.ToTable("despesa");

            entity.Property(e => e.IdDespesa).HasColumnName("id_despesa");
            entity.Property(e => e.DataCadastro)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("data_cadastro");
            entity.Property(e => e.Icone)
                .HasMaxLength(500)
                .HasColumnName("icone");
            entity.Property(e => e.IdGrupo).HasColumnName("id_grupo");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");
            entity.Property(e => e.Vencimento).HasColumnName("vencimento");

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>(); ;

            entity.HasOne(d => d.IdGrupoNavigation).WithMany(p => p.Despesas)
                .HasForeignKey(d => d.IdGrupo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_despesa_grupo");
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.HasKey(e => e.IdGrupo).HasName("grupo_pkey");

            entity.ToTable("grupo");

            entity.HasIndex(e => e.CodigoAcesso, "grupo_codigo_acesso_key").IsUnique();

            entity.Property(e => e.IdGrupo).HasColumnName("id_grupo");
            entity.Property(e => e.CodigoAcesso)
                .HasMaxLength(20)
                .HasColumnName("codigo_acesso");
            entity.Property(e => e.IdAdmin).HasColumnName("id_admin");
            entity.Property(e => e.ImagemBanner)
                .HasMaxLength(500)
                .HasColumnName("imagem_banner");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.HasOne(d => d.IdAdminNavigation).WithMany(p => p.Grupos)
                .HasForeignKey(d => d.IdAdmin)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_grupo");
        });

        modelBuilder.Entity<Parcela>(entity =>
        {
            entity.HasKey(e => e.IdParcela).HasName("parcela_pkey");

            entity.ToTable("parcela");

            entity.HasIndex(e => e.IdDespesa, "idx_parcela_despesa");

            entity.HasIndex(e => new { e.IdUsuario, e.IdDespesa }, "parcela_id_usuario_id_despesa_key").IsUnique();

            entity.Property(e => e.IdParcela).HasColumnName("id_parcela");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.IdDespesa).HasColumnName("id_despesa");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Valor).HasPrecision(10, 2).HasColumnName("valor");

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>(); ;

            entity.HasOne(d => d.IdDespesaNavigation).WithMany(p => p.Parcelas)
                .HasForeignKey(d => d.IdDespesa)
                .HasConstraintName("fk_parcela_despesa");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Parcelas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_parcela_usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuario_pkey");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Email, "usuario_email_key").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .HasColumnName("senha");

 
        });

        modelBuilder.Entity<Pertence>(entity =>
        {
            entity.HasKey(e => new { e.IdUsuario, e.IdGrupo }).HasName("pertence_pkey");
            entity.ToTable("pertence");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdGrupo).HasColumnName("id_grupo");

            entity.HasOne(d => d.IdGrupoNavigation).WithMany()
                .HasForeignKey(d => d.IdGrupo)
                .HasConstraintName("fk_pertence_grupo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_pertence_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
