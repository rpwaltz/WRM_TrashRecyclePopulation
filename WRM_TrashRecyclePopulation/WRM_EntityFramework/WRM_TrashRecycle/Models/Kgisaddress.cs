﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WRM_TrashRecyclePopulation.WRM_EntityFramework.WRM_TrashRecycle.Models
{
    [Keyless]
    public partial class KGISAddress
    {
        [Column(TypeName = "numeric(38, 0)")]
        public decimal OBJECTID { get; set; }
        [Column(TypeName = "numeric(38, 0)")]
        public decimal? ADDRESS_STATUS { get; set; }
        [Column(TypeName = "numeric(38, 0)")]
        public decimal? ADDRESS_NUM { get; set; }
        [StringLength(2)]
        public string PREFIX { get; set; }
        [StringLength(32)]
        public string STREET_BASENAME { get; set; }
        [StringLength(50)]
        public string STREET_NAME { get; set; }
        [StringLength(4)]
        public string STREET_TYPE { get; set; }
        [StringLength(10)]
        public string UNIT { get; set; }
        [StringLength(30)]
        public string UNIT_TYPE { get; set; }
        [Column(TypeName = "numeric(38, 0)")]
        public decimal? ZIP_CODE { get; set; }
        [StringLength(2)]
        public string STATE_CODE { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? POINT_X { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? POINT_Y { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? LATITUDE { get; set; }
        [Column(TypeName = "numeric(38, 8)")]
        public decimal? LONGITUDE { get; set; }
        [StringLength(30)]
        public string ADDRESS_USE_TYPE { get; set; }
        [StringLength(50)]
        public string PARCELID { get; set; }
        [Column(TypeName = "numeric(38, 0)")]
        public decimal? JURISDICTION { get; set; }
        [Column(TypeName = "date")]
        public DateTime? DATE_OFFICIAL_CHANGE { get; set; }
    }
}