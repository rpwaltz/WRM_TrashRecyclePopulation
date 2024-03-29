﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models;

namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Configurations
{
    public partial class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> entity)
        {
            entity.Property(e => e.CartSerialNumber).HasDefaultValueSql("('UNKNOWN')");

            entity.Property(e => e.CartSize).HasDefaultValueSql("('96 GALLON')");

            entity.Property(e => e.CartType).HasDefaultValueSql("('TRASH')");

            entity.Property(e => e.CompositeCartKey).HasComputedColumnSql("(concat([CartSerialNumber],[AddressID],[CartType]))", false);

            entity.HasOne(d => d.Address)
                .WithMany(p => p.Cart)
                .HasForeignKey(d => d.AddressID)
                .HasConstraintName("FK__Cart__AddressID__4CA06362");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Cart> entity);
    }
}
