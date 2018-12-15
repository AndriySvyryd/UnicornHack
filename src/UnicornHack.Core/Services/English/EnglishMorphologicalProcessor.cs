using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Services.English
{
    public class EnglishMorphologicalProcessor
    {
        private static readonly string[] UninflectiveSuffixes =
            {"fish", "ese", "ois", "sheep", "deer", "pos", "itis", "ism", "ness"};

        private static readonly string[] UninflectiveWords =
        {
            "advice", "aircraft", "asparagus", "barracks", "beef", "bison", "binoculars", "bream", "britches",
            "breeches", "broccoli", "cabbage", "carp", "cattle", "clippers", "chassis", "cod", "contretemps", "corps",
            "corn", "cotton", "chaos", "diabetes", "debris", "earth", "elk", "eland", "flounder", "gallows", "gold",
            "graffiti", "high-jinks", "headquarters", "herpes", "homework", "hay", "hair", "hemp", "jeans",
            "jackanapes", "information", "ice", "lettuce", "mackerel", "moose", "mumps", "mews", "milk", "millet",
            "mutton", "molasses", "measles", "news", "okra", "offspring", "pants", "proceedings", "pliers", "pincers",
            "police", "pork", "rabies", "rice", "salmon", "scissors", "sea-bass", "series", "shears", "shorts",
            "species", "swine", "scabies", "shambles", "shingles", "trout", "tuna", "traffic", "tobacco", "wildebeest",
            "whiting", "venison", "apparatus", "impetus", "prospectus", "cantus", "nexus", "sinus", "coitus", "plexus",
            "status", "hiatus"
        };

        private static readonly Dictionary<string, string> IrregularPlurals = new Dictionary<string, string>
        {
            {"trilby", "trilbys"},
            {"cheese", "cheeses"},
            {"child", "children"},
            {"person", "people"},
            {"ox", "oxen"},
            {"chilli", "chillies"},
            {"cloth", "clothes"},
            {"staff", "staves"},
            {"thief", "thieves"},
            {"die", "dice"},
            {"mouse", "mice"},
            {"louse", "lice"},
            {"goose", "geese"},
            {"albino", "albinos"},
            {"generalissimo", "generalissimos"},
            {"manifesto", "manifestos"},
            {"archipelago", "archipelagos"},
            {"ghetto", "ghettos"},
            {"medico", "medicos"},
            {"armadillo", "armadillos"},
            {"guano", "guanos"},
            {"commando", "commandos"},
            {"inferno", "infernos"},
            {"photo", "photos"},
            {"ditto", "dittos"},
            {"jumbo", "jumbos"},
            {"pro", "pros"},
            {"dynamo", "dynamos"},
            {"lingo", "lingos"},
            {"solo", "solos"},
            {"quarto", "quartos"},
            {"octavo", "octavos"},
            {"embryo", "embryos"},
            {"lumbago", "lumbagos"},
            {"rhino", "rhinos"},
            {"fiasco", "fiascos"},
            {"magneto", "magnetos"},
            {"stylo", "stylos"},
            {"auto", "autos"},
            {"memo", "memos"},
            {"casino", "casinos"},
            {"silo", "silos"},
            {"stereo", "stereos"},
            {"halo", "halos"},
            {"kilo", "kilos"},
            {"piano", "pianos"},
            {"alumna", "alumnae"},
            {"alga", "algae"},
            {"vertebra", "vertebrae"},
            {"larva", "larvae"},
            {"abscissa", "abscissae"},
            {"formula", "formulae"},
            {"medusa", "medusae"},
            {"amoeba", "amoebae"},
            {"hydra", "hydrae"},
            {"nebula", "nebulae"},
            {"antenna", "antennae"},
            {"hyperbola", "hyperbolae"},
            {"nova", "novae"},
            {"aurora", "aurorae"},
            {"lacuna", "lacunae"},
            {"parabola", "parabolae"},
            {"codex", "codices"},
            {"murex", "murices"},
            {"silex", "silices"},
            {"apex", "apices"},
            {"latex", "latices"},
            {"vertex", "vertices"},
            {"cortex", "cortices"},
            {"pontifex", "pontifices"},
            {"vortex", "vortices"},
            {"index", "indices"},
            {"simplex", "simplices"},
            {"aphelion", "aphelia"},
            {"perihelion", "perihelia"},
            {"hyperbaton", "hyperbata"},
            {"asyndeton", "asyndeta"},
            {"noumenon", "noumena"},
            {"phenomenon", "phenomena"},
            {"criterion", "criteria"},
            {"organon", "organa"},
            {"prolegomenon", "prolegomena"},
            {"automaton", "automata"},
            {"polyhedron", "polyhedra"},
            {"agendum", "agenda"},
            {"datum", "data"},
            {"extremum", "extrema"},
            {"bacterium", "bacteria"},
            {"desideratum", "desiderata"},
            {"stratum", "strata"},
            {"candelabrum", "candelabra"},
            {"erratum", "errata"},
            {"ovum", "ova"},
            {"forum", "fora"},
            {"addendum", "addenda"},
            {"stadium", "stadia"},
            {"aquarium", "aquaria"},
            {"interregnum", "interregna"},
            {"quantum", "quanta"},
            {"compendium", "compendia"},
            {"lustrum", "lustra"},
            {"rostrum", "rostra"},
            {"consortium", "consortia"},
            {"maximum", "maxima"},
            {"spectrum", "spectra"},
            {"cranium", "crania"},
            {"medium", "media"},
            {"speculum", "specula"},
            {"curriculum", "curricula"},
            {"memorandum", "memoranda"},
            {"dictum", "dicta"},
            {"millenium", "millenia"},
            {"trapezium", "trapezia"},
            {"emporium", "emporia"},
            {"minimum", "minima"},
            {"ultimatum", "ultimata"},
            {"enconium", "enconia"},
            {"momentum", "momenta"},
            {"vacuum", "vacua"},
            {"gymnasium", "gymnasia"},
            {"optimum", "optima"},
            {"velum", "vela"},
            {"honorarium", "honoraria"},
            {"phylum", "phyla"},
            {"stamen", "stamina"},
            {"foramen", "foramina"},
            {"lumen", "lumina"},
            {"anathema", "anathemata"},
            {"enema", "enemata"},
            {"oedema", "oedemata"},
            {"bema", "bemata"},
            {"enigma", "enigmata"},
            {"sarcoma", "sarcomata"},
            {"carcinoma", "carcinomata"},
            {"gumma", "gummata"},
            {"schema", "schemata"},
            {"charisma", "charismata"},
            {"lemma", "lemmata"},
            {"soma", "somata"},
            {"diploma", "diplomata"},
            {"lymphoma", "lymphomata"},
            {"stigma", "stigmata"},
            {"dogma", "dogmata"},
            {"magma", "magmata"},
            {"stoma", "stomata"},
            {"drama", "dramata"},
            {"melisma", "melismata"},
            {"trauma", "traumata"},
            {"edema", "edemata"},
            {"miasma", "miasmata"},
            {"corpus", "corpora"},
            {"viscus", "viscera"},
            {"ephemeris", "ephemerides"},
            {"iris", "irides"},
            {"clitoris", "clitorides"},
            {"alumnus", "alumni"},
            {"cactus", "cacti"},
            {"focus", "foci"},
            {"genius", "genii"},
            {"hippopotamus", "hippopotami"},
            {"incubus", "incubi"},
            {"succubus", "succubi"},
            {"fungus", "fungi"},
            {"mythos", "mythoi"},
            {"nimbus", "nimbi"},
            {"nucleolus", "nucleoli"},
            {"nucleus", "nuclei"},
            {"radius", "radii"},
            {"stimulus", "stimuli"},
            {"stylus", "styli"},
            {"torus", "tori"},
            {"umbilicus", "umbilici"},
            {"uterus", "uteri"},
            {"efreet", "efreeti"},
            {"djinni", "djinnis"},
            {"cherub", "cherubim"},
            {"goy", "goyim"},
            {"seraph", "seraphim"}
        };

        private static readonly string[,,] Pronouns = new string[2, 5, 5]
        {
            {
                {"I", "you", "it", "he", "she"}, {"me", "you", "it", "him", "her"},
                {"myself", "yourself", "itself", "himself", "herself"}, {"mine", "yours", "its", "his", "hers"},
                {"my", "your", "its", "his", "her"}
            },
            {
                {"we", "you", "they", "they", "they"}, {"us", "you", "them", "them", "them"},
                {"ourselves", "yourselves", "themselves", "themselves", "themselves"},
                {"ours", "yours", "theirs", "theirs", "theirs"}, {"our", "your", "their", "their", "their"}
            }
        };

        private readonly SuffixReplacementStateMachine _nounPluralizationMachine = new SuffixReplacementStateMachine();
        private readonly SuffixReplacementStateMachine _verbSFormMachine = new SuffixReplacementStateMachine();

        public EnglishMorphologicalProcessor()
        {
            foreach (var suffix in UninflectiveSuffixes)
            {
                _nounPluralizationMachine.AddRule(suffix, replacement: "", charactersToReplace: 0);
            }

            foreach (var word in UninflectiveWords)
            {
                _nounPluralizationMachine.AddRule("^" + word, replacement: "", charactersToReplace: 0);
            }

            foreach (var pair in IrregularPlurals)
            {
                _nounPluralizationMachine.AddRule("^" + pair.Key, pair.Value, pair.Key.Length);
            }

            _nounPluralizationMachine.AddRule(suffix: "man", replacement: "en", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "tooth", replacement: "eeth", charactersToReplace: 4);
            _nounPluralizationMachine.AddRule(suffix: "foot", replacement: "eet", charactersToReplace: 3);
            _nounPluralizationMachine.AddRule(suffix: "zoon", replacement: "a", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "cis", replacement: "es", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "sis", replacement: "es", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "xis", replacement: "es", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "trix", replacement: "ces", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "eau", replacement: "x", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "ieu", replacement: "x", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "alf", replacement: "ves", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "elf", replacement: "ves", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "olf", replacement: "ves", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "eaf", replacement: "ves", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "arf", replacement: "ves", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "ife", replacement: "ves", charactersToReplace: 2);
            _nounPluralizationMachine.AddRule(suffix: "ao", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "eo", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "io", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "oo", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "uo", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "ch", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "sh", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "ss", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "s", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "zz", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "x", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "o", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "i", replacement: "es", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "z", replacement: "zes", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "ay", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "ey", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "iy", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "oy", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "uy", replacement: "s", charactersToReplace: 0);
            _nounPluralizationMachine.AddRule(suffix: "y", replacement: "ies", charactersToReplace: 1);
            _nounPluralizationMachine.AddRule(suffix: "", replacement: "s", charactersToReplace: 0);

            _verbSFormMachine.AddRule(suffix: "^be", replacement: "is", charactersToReplace: 2);
            _verbSFormMachine.AddRule(suffix: "^have", replacement: "s", charactersToReplace: 2);
            _verbSFormMachine.AddRule(suffix: "ao", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "eo", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "io", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "oo", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "uo", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "ch", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "sh", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "ss", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "s", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "zz", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "x", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "o", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "i", replacement: "es", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "z", replacement: "zes", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "ay", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "ey", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "iy", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "oy", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "uy", replacement: "s", charactersToReplace: 0);
            _verbSFormMachine.AddRule(suffix: "y", replacement: "ies", charactersToReplace: 1);
            _verbSFormMachine.AddRule(suffix: "", replacement: "s", charactersToReplace: 0);
        }

        public string ProcessVerbSimplePresent(string verbPhrase, EnglishPerson person) => ProcessVerb(
            verbPhrase,
            person == EnglishPerson.Third
                ? EnglishVerbForm.ThirdPersonSingularPresent
                : EnglishVerbForm.BareInfinitive);

        public string ProcessVerb(string verbPhrase, EnglishVerbForm form)
        {
            switch (form)
            {
                case EnglishVerbForm.Infinitive:
                    return "to " + verbPhrase;
                case EnglishVerbForm.ThirdPersonSingularPresent:
                    var verb = GetFirstWord(verbPhrase, out var lastPart);
                    return _verbSFormMachine.Process(verb) + lastPart;
                default:
                    return verbPhrase;
            }
        }

        public string ProcessNoun(string nounPhrase, EnglishNounForm form)
        {
            switch (form)
            {
                case EnglishNounForm.Plural:
                    var words = nounPhrase.Split(' ');
                    var indexToPluralize = words.Length - 1;
                    if (words.Length > 2 && string.Equals("of", words[1], StringComparison.OrdinalIgnoreCase))
                    {
                        indexToPluralize = 0;
                    }

                    words[indexToPluralize] = _nounPluralizationMachine.Process(words[indexToPluralize]);
                    return string.Join(" ", words);
                default:
                    return nounPhrase;
            }
        }

        public bool IsPlural(string noun)
            => noun.Length > 2 && noun.Substring(noun.Length - 2, 2) == "es";

        public bool IsVocal(char c)
        {
            switch (char.ToLowerInvariant(c))
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    return true;
                default:
                    return false;
            }
        }

        public string GetPronoun(
            EnglishPronounForm form, EnglishNumber number, EnglishPerson person, EnglishGender? gender)
        {
            var numberIndex = number == EnglishNumber.Plural ? 1 : 0;
            var formIndex = (int)form;
            var personIndex = person == EnglishPerson.Third ? 2 + (int)gender.Value : (int)person;
            return Pronouns[numberIndex, formIndex, personIndex];
        }

        private static string GetFirstWord(string phrase, out string lastPart)
        {
            var firstWordLength = phrase.IndexOf(value: ' ');
            if (firstWordLength == -1)
            {
                firstWordLength = phrase.Length;
            }

            lastPart = phrase.Substring(firstWordLength);
            return phrase.Substring(startIndex: 0, length: firstWordLength);
        }

        private static string GetLastWord(string phrase, out string firstPart)
        {
            var firstPartLength = phrase.LastIndexOf(value: ' ') + 1;
            firstPart = phrase.Substring(startIndex: 0, length: firstPartLength);
            return phrase.Substring(firstPartLength);
        }

        private class SuffixReplacementStateMachine
        {
            private readonly State _initialState = new State();

            public void AddRule(string suffix, string replacement, int charactersToReplace)
            {
                Debug.Assert(suffix.Length >= charactersToReplace);

                var state = _initialState;
                for (var i = suffix.Length - 1; i >= 0; i--)
                {
                    var nextCharacter = suffix[i];
                    var nextState = state.Next(nextCharacter);
                    if (nextState == null)
                    {
                        nextState = new State();
                        state.AddNext(nextCharacter, nextState);
                    }

                    state = nextState;
                }

                Debug.Assert(state.Replacement == null, "Repeated suffix " + suffix);
                state.Replacement = new ReplacementSuffix
                {
                    ReplacementString = replacement,
                    CharactersToReplace = charactersToReplace
                };
            }

            public string Process(string word)
            {
                var state = _initialState;
                Debug.Assert(state.Replacement.HasValue);
                var replacement = state.Replacement.Value;
                var reachedEnd = true;
                for (var i = word.Length - 1; i >= 0; i--)
                {
                    var nextState = state.Next(word[i]);
                    if (nextState == null)
                    {
                        reachedEnd = false;
                        break;
                    }

                    state = nextState;
                    if (state.Replacement.HasValue)
                    {
                        replacement = state.Replacement.Value;
                    }
                }

                if (reachedEnd)
                {
                    var nextState = state.Next(character: '^');
                    if (nextState != null)
                    {
                        Debug.Assert(nextState.Replacement.HasValue);
                        replacement = nextState.Replacement.Value;
                    }
                }

                return word.Remove(word.Length - replacement.CharactersToReplace, replacement.CharactersToReplace) +
                       replacement.ReplacementString;
            }

            private class State
            {
                private readonly Dictionary<char, State> _nextStates = new Dictionary<char, State>();

                public ReplacementSuffix? Replacement { get; set; }

                public void AddNext(char character, State state) => _nextStates.Add(character, state);

                public State Next(char character) =>
                    _nextStates.TryGetValue(character, out var nextState) ? nextState : null;
            }

            private struct ReplacementSuffix
            {
                public string ReplacementString { get; set; }
                public int CharactersToReplace { get; set; }
            }
        }
    }
}
