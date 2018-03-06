using System.ComponentModel.DataAnnotations;

namespace UnicornHack.Models
{
    public class CharacterModel
    {
        [Required]
        [RegularExpression("[w]+")]
        [Display(Name = "Character Name", Prompt = "Character Name")]
        public string Name { get; set; }
    }
}