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
        public static readonly PropertyDescription HealthPointMaximum = new PropertyDescription { Name = "health point maximum", PropertyType = typeof(int), MinValue = 0 };
    }
}