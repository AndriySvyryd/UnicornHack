using System;
namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class ChangeSimpleProperty : AbilityEffect
    {
        public string PropertyName { get; set; }
        public bool IsAdded { get; set; }
    }
}
