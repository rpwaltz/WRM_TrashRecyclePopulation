﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste;
using WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste.Models;

namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.SolidWaste.Configurations
{
    public partial class RecyclingRequestConfiguration : IEntityTypeConfiguration<RecyclingRequest>
    {
        public void Configure(EntityTypeBuilder<RecyclingRequest> entity)
        {
            entity.ToTable("Recycling_Request");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.Property(e => e.AcceptedTermsOfAgreement)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("ACCEPTED_TERMS_OF_AGREEMENT");

            entity.Property(e => e.AcknowledgeAdditionalDetails)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("ACKNOWLEDGE_ADDITIONAL_DETAILS");

            entity.Property(e => e.AcknowledgeJulyStartdate)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("ACKNOWLEDGE_JULY_STARTDATE");

            entity.Property(e => e.AcknowledgeMissed1stDeadline)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("ACKNOWLEDGE_MISSED_1ST_DEADLINE");

            entity.Property(e => e.BackdoorService)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("BACKDOOR_SERVICE");

            entity.Property(e => e.City)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("CITY");

            entity.Property(e => e.Comments)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("COMMENTS");

            entity.Property(e => e.Councildistrict).HasColumnName("COUNCILDISTRICT");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CREATED_BY");

            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATION_DATE");

            entity.Property(e => e.CurrentlyEnrolledInRecycling)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CURRENTLY_ENROLLED_IN_RECYCLING");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EMAIL");

            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FIRST_NAME");

            entity.Property(e => e.LastName)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("LAST_NAME");

            entity.Property(e => e.LastUpdatedBy)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("LAST_UPDATED_BY");

            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("LAST_UPDATED_DATE");

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("PHONE_NUMBER");

            entity.Property(e => e.PointX)
                .HasColumnType("numeric(18, 8)")
                .HasColumnName("POINT_X");

            entity.Property(e => e.PointY)
                .HasColumnType("numeric(18, 8)")
                .HasColumnName("POINT_Y");

            entity.Property(e => e.RolloutNote)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.SaveDuplicate)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.Property(e => e.SendEmailNewsletter)
                .HasMaxLength(1)
                .IsFixedLength();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("STATUS");

            entity.Property(e => e.StatusDate)
                .HasColumnType("datetime")
                .HasColumnName("STATUS_DATE");

            entity.Property(e => e.StillInterested)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("STILL_INTERESTED");

            entity.Property(e => e.StillInterestedUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("STILL_INTERESTED_UPDATED_DATE");

            entity.Property(e => e.StreetEid)
                .HasColumnType("numeric(38, 0)")
                .HasColumnName("STREET_EID");

            entity.Property(e => e.StreetName)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("STREET_NAME");

            entity.Property(e => e.StreetNamePrefix)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("STREET_NAME_PREFIX");

            entity.Property(e => e.StreetNameSuffix)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("STREET_NAME_SUFFIX");

            entity.Property(e => e.StreetNumber).HasColumnName("STREET_NUMBER");

            entity.Property(e => e.StreetSuffixDirection)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("STREET_SUFFIX_DIRECTION");

            entity.Property(e => e.UnitNumber)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("UNIT_NUMBER");

            entity.Property(e => e.ZipCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ZIP_CODE");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<RecyclingRequest> entity);
    }
}
