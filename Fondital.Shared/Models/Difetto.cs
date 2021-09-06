﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fondital.Shared.Models
{
    [Table("Difetti")]
    public class Difetto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Il campo è obbligatorio.")]
        public string NomeItaliano { get; set; }
        [Required(ErrorMessage = "Il campo è obbligatorio.")]
        public string NomeRusso { get; set; }
        public bool IsAbilitato { get; set; }
    }
}
