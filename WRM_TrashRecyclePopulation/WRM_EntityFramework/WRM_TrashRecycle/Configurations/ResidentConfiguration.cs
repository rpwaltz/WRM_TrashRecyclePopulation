﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;


namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Configurations
{
    public partial class ResidentConfiguration : IEntityTypeConfiguration<Resident>
    {
        public void Configure(EntityTypeBuilder<Resident> entity)
        {
            entity.Property(e => e.ResidentId).HasColumnName("ResidentID");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");

            entity.Property(e => e.CreateDate).HasColumnType("datetime");

            entity.Property(e => e.CreateUser)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Note).HasColumnType("text");

            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.Property(e => e.UpdateUser)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Address)
                .WithMany(p => p.Resident)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__Resident__Addres__2136E270");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Resident> entity);
    }
}