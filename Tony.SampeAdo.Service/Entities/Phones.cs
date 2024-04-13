using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tony.SampeAdo.Service.Entities
{
    public class Phones : BaseEntity
    {
        [Key]
        [Phone]
        public string Phone { get; set; }
        public DateTime? ChangedOn { get; set; }

        public virtual ICollection<Codes> Codes { get; set; }
    }
}
