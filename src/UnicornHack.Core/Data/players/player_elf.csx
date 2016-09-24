new PlayerVariant
{
    BaseCreatureVariantName = "elf",
    StrengthBonus = 1,
    DexterityBonus = 3,
    IntelligenceBonus = 2,
    WillpowerBonus = 2,
    SpeedBonus = 2,
    SkillAptitudes = new Dictionary<Skill, int> { { Skill.Bows, 1 } },
    Name = "player elf"
}
