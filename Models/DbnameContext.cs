using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace demo2;

public partial class DbnameContext : DbContext
{
    public DbnameContext()
    {
    }

    public DbnameContext(DbContextOptions<DbnameContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<SampleTable> SampleTables { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.FirstName)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<SampleTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SampleTable");

            entity.Property(e => e.Data)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("data");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83F022C2FE1");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("phone_number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
