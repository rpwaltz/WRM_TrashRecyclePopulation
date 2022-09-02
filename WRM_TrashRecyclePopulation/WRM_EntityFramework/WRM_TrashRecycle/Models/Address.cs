﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models
{
    public partial class Address
    {
        public Address()
        {
            BackDoorPickup = new HashSet<BackDoorPickup>();
            Cart = new HashSet<Cart>();
            CommercialAccount = new HashSet<CommercialAccount>();
            Resident = new HashSet<Resident>();
        }

        [Key]
        public int AddressID { get; set; }
        [StringLength(11)]
        [Unicode(false)]
        public string GISParcelID { get; set; }
        public int StreetNumber { get; set; }
        [Required]
        [StringLength(100)]
        [Unicode(false)]
        public string StreetName { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string UnitNumber { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string ZipCode { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string NumberUnits { get; set; }
        [Required]
        [StringLength(50)]
        [Unicode(false)]
        public string AddressType { get; set; }
        public bool? AlleyPickup { get; set; }
        [StringLength(30)]
        [Unicode(false)]
        public string GISAddressUseType { get; set; }
        [StringLength(25)]
        [Unicode(false)]
        public string RecyclingStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? RecyclingStatusDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? RecyclingRequestedDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? RecyclingApprovalDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? RecyclingWithdrawalDate { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string RecycleDayOfWeek { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string RecycleFrequency { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string TrashDayOfWeek { get; set; }
        [StringLength(25)]
        [Unicode(false)]
        public string TrashStatus { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? GISPointX { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? GISPointY { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? GISLatitude { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? GISLongitude { get; set; }
        [StringLength(512)]
        [Unicode(false)]
        public string Comment { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string CreateUser { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CreateDate { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string UpdateUser { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? UpdateDate { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string AlternateSchedule { get; set; }
        [StringLength(32)]
        [Unicode(false)]
        public string BatchLoadGUID { get; set; }

        [InverseProperty("Address")]
        public virtual ICollection<BackDoorPickup> BackDoorPickup { get; set; }
        [InverseProperty("Address")]
        public virtual ICollection<Cart> Cart { get; set; }
        [InverseProperty("Address")]
        public virtual ICollection<CommercialAccount> CommercialAccount { get; set; }
        [InverseProperty("Address")]
        public virtual ICollection<Resident> Resident { get; set; }
    }
}