using System.ComponentModel.DataAnnotations;

namespace UnicornHack.Models;

public class CharacterModel
{
    [Required]
    [RegularExpression("[w]+")]
    [StringLength(32)]
    [Display(Name = "Character Name", Prompt = "Type in your character name")]
    public string Name
    {
        get;
        set;
    }
}
