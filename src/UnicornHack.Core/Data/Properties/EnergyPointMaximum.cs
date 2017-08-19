using System;
using System.Collections.Generic;
using UnicornHack;
using UnicornHack.Effects;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription EnergyPointMaximum = new PropertyDescription { Name = "energy point maximum", PropertyType = typeof(int), MinValue = 0 };
    }
}