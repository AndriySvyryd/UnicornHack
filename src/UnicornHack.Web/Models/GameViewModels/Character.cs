using System.ComponentModel.DataAnnotations;

namespace UnicornHack.Models.GameViewModels
{
    public class Character
    {
        [Required]
        [RegularExpression("[w]+")]
        [Display(Name = "Character Name", Prompt = "Character Name")]
        public string Name { get; set; }
    }
}