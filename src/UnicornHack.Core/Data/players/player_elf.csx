new Player
{
    Name = "player elf",
    BaseName = "elf",
    MovementRate = 14,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Strength = 11,
    Dexterity = 13,
    Intelligence = 12,
    Willpower = 12,
    Speed = 12,
    SkillAptitudes = new Dictionary<Skill, int> { { Skill.Bows, 1 } }
}
