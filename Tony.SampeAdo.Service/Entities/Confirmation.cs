﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Tony.SampeAdo.Service.Entities
{
    public class Confirmation : BaseEntity
    {
        [Key]
        public Guid InstallationId { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string ConfirmationCode { get; set; }
        public DateTime? ConfirmedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
    }
}
