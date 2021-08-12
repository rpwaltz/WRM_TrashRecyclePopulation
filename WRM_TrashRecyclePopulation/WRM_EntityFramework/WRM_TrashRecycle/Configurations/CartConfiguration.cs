﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;


namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Configurations
{
    public partial class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> entity)
        {
            entity.Property(e => e.CartId).HasColumnName("CartID");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");

            entity.Property(e => e.CartSerialNumber)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.Property(e => e.CartSize)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.CartStatus)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.CreateDate).HasColumnType("datetime");

            entity.Property(e => e.CreateUser)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Note)
                .HasMaxLength(512)
                .IsUnicode(false);

            entity.Property(e => e.SerialNumberReceivedDate).HasColumnType("datetime");

            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.Property(e => e.UpdateUser)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Address)
                .WithMany(p => p.Cart)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__Cart__AddressID__29CC2871");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Cart> entity);
    }
}