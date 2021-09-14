﻿using System.ComponentModel.DataAnnotations;

namespace Fondital.Shared.Models
{
    public class Difetto
    {
        public int Id { get; set; }

        [Display(Name = "NomeItaliano", ResourceType = typeof(Resources.Display))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        public string NomeItaliano { get; set; } = "";

        [Display(Name = "NomeRusso", ResourceType = typeof(Resources.Display))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        public string NomeRusso { get; set; } = "";
        public bool IsAbilitato { get; set; } = true;
    }
}
