namespace UnicornHack.Models.GameDefinitions
{
    public class Attack
    {
        public Attack(
            AttackType type,
            AttackEffect effect,
            byte diceCount = 0,
            byte diceSides = 0,
            Frequency frequency = Frequency.Always)
        {
            Type = type;
            Effect = effect;
            DiceCount = diceCount;
            DiceSides = diceSides;
            Frequency = frequency;
        }

        public AttackType Type { get; }
        public AttackEffect Effect { get; }
        public byte DiceCount { get; }
        public byte DiceSides { get; }
        public Frequency Frequency { get; }
    }
}