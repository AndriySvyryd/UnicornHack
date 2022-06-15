using Xunit;

namespace UnicornHack.Services.English;

public class EnglishMorphologicalProcessorTest : IClassFixture<EnglishMorphologicalProcessorTest.TestFixture>
{
    [Theory]
    [InlineData("boat", "boats")]
    [InlineData("house", "houses")]
    [InlineData("cat", "cats")]
    [InlineData("bus", "buses")]
    [InlineData("wish", "wishes")]
    [InlineData("pitch", "pitches")]
    [InlineData("box", "boxes")]
    [InlineData("quiz", "quizzes")]
    [InlineData("buzz", "buzzes")]
    [InlineData("fox", "foxes")]
    [InlineData("penny", "pennies")]
    [InlineData("spy", "spies")]
    [InlineData("daisy", "daisies")]
    [InlineData("jelly", "jellies")]
    [InlineData("boy", "boys")]
    [InlineData("way", "ways")]
    [InlineData("key", "keys")]
    [InlineData("echo", "echoes")]
    [InlineData("hero", "heroes")]
    [InlineData("auto", "autos")]
    [InlineData("kangaroo", "kangaroos")]
    [InlineData("tattoo", "tattoos")]
    [InlineData("zoo", "zoos")]
    [InlineData("kilo", "kilos")]
    [InlineData("memo", "memos")]
    [InlineData("photo", "photos")]
    [InlineData("piano", "pianos")]
    [InlineData("solo", "solos")]
    [InlineData("duo", "duos")]
    [InlineData("studio", "studios")]
    [InlineData("video", "videos")]
    [InlineData("halo", "halos")]
    [InlineData("mosquito", "mosquitoes")]
    [InlineData("tornado", "tornadoes")]
    [InlineData("volcano", "volcanoes")]
    [InlineData("calf", "calves")]
    [InlineData("half", "halves")]
    [InlineData("wolf", "wolves")]
    [InlineData("thief", "thieves")]
    [InlineData("elf", "elves")]
    [InlineData("dwarf", "dwarves")]
    [InlineData("leaf", "leaves")]
    [InlineData("staff", "staves")]
    [InlineData("belief", "beliefs")]
    [InlineData("chef", "chefs")]
    [InlineData("roof", "roofs")]
    [InlineData("knife", "knives")]
    [InlineData("safe", "safes")]
    [InlineData("man", "men")]
    [InlineData("woman", "women")]
    [InlineData("person", "people")]
    [InlineData("child", "children")]
    [InlineData("foot", "feet")]
    [InlineData("goose", "geese")]
    [InlineData("cheese", "cheeses")]
    [InlineData("archdiocese", "archdiocese")]
    [InlineData("tooth", "teeth")]
    [InlineData("cloth", "clothes")]
    [InlineData("mouse", "mice")]
    [InlineData("louse", "lice")]
    [InlineData("ox", "oxen")]
    [InlineData("djinni", "djinnis")]
    [InlineData("die", "dice")]
    [InlineData("sheep", "sheep")]
    [InlineData("deer", "deer")]
    [InlineData("moose", "moose")]
    [InlineData("fish", "fish")]
    [InlineData("police", "police")]
    [InlineData("gold", "gold")]
    [InlineData("earth", "earth")]
    [InlineData("news", "news")]
    [InlineData("advice", "advice")]
    [InlineData("information", "information")]
    [InlineData("cattle", "cattle")]
    [InlineData("pants", "pants")]
    [InlineData("jeans", "jeans")]
    [InlineData("shorts", "shorts")]
    [InlineData("scissors", "scissors")]
    [InlineData("clippers", "clippers")]
    [InlineData("binoculars", "binoculars")]
    [InlineData("barracks", "barracks")]
    [InlineData("species", "species")]
    [InlineData("toughness", "toughness")]
    [InlineData("strength", "strengths")]
    [InlineData("protozoon", "protozoa")]
    [InlineData("plateau", "plateaux")]
    [InlineData("chassis", "chassis")]
    [InlineData("axis", "axes")]
    [InlineData("crisis", "crises")]
    [InlineData("thesis", "theses")]
    [InlineData("hypothesis", "hypotheses")]
    [InlineData("oasis", "oases")]
    [InlineData("matrix", "matrices")]
    [InlineData("vortex", "vortices")]
    [InlineData("sphinx", "sphinxes")]
    [InlineData("lynx", "lynxes")]
    [InlineData("fungus", "fungi")]
    [InlineData("cactus", "cacti")]
    [InlineData("nucleus", "nuclei")]
    [InlineData("corpus", "corpora")]
    [InlineData("bacterium", "bacteria")]
    [InlineData("datum", "data")]
    [InlineData("criterion", "criteria")]
    [InlineData("antenna", "antennae")]
    [InlineData("alga", "algae")]
    [InlineData("larva", "larvae")]
    [InlineData("focus", "foci")]
    public void Nouns_are_pluralized(string noun, string expectedPlural) => Assert.Equal(expectedPlural,
        Fixture.EnglishMorphologicalProcessor.ProcessNoun(noun, EnglishNounForm.Plural));

    [Theory]
    [InlineData("be", "is")]
    [InlineData("bite", "bites")]
    [InlineData("dig", "digs")]
    [InlineData("see", "sees")]
    [InlineData("die", "dies")]
    [InlineData("solo", "soloes")]
    [InlineData("kiss", "kisses")]
    [InlineData("wish", "wishes")]
    [InlineData("touch", "touches")]
    [InlineData("fizz", "fizzes")]
    [InlineData("fix", "fixes")]
    [InlineData("fly", "flies")]
    [InlineData("slay", "slays")]
    [InlineData("spit at", "spits at")]
    [InlineData("run to", "runs to")]
    public void Verbs_3SPresent(string verb, string expectedPlural) => Assert.Equal(expectedPlural,
        Fixture.EnglishMorphologicalProcessor.ProcessVerb(verb, EnglishVerbForm.ThirdPersonSingularPresent));

    protected TestFixture Fixture
    {
        get;
    }

    public EnglishMorphologicalProcessorTest(TestFixture fixture)
    {
        Fixture = fixture;
    }

    public class TestFixture
    {
        public EnglishMorphologicalProcessor EnglishMorphologicalProcessor
        {
            get;
        } =
            new EnglishMorphologicalProcessor();
    }
}
