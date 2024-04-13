using System;
using System.ComponentModel.DataAnnotations;

namespace Tony.SampeAdo.Service.Entities
{
    public class Codes : BaseEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }

        [Phone]
        public string Phone { get; set; }
        public DateTime SentOn { get; set; }

        public Phones Phones { get; set; }
    }
}
