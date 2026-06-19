namespace CasLib
{
    public static class FlagsDecayInfo
    {
        /// <summary>
        /// Indicates that the item will only decay when there is
        /// atleast 1 item in it's inventory.
        /// </summary>
        public const byte DECAY_WHEN_FILLED = 1;
        /// <summary>
        /// Indicates that the item will only decay when it is held,
        /// and is not contained within another container.
        /// </summary>
        public const byte DECAY_WHEN_MAIN_SLOTS = 2;
        /// <summary>
        /// Indicates that the item will only decay when it is held,
        /// and the player is moving faster than 0.5u.
        /// </summary>
        public const byte DECAY_WHEN_MOVING = 4;
    }
    public static class CraftingQualitys
    {
        // tools
        public const string CUTTING = "cutting";
        public const string HAMMERING = "hammering";
        public const string NAILS = "nails";
        public const string DRESSING = "dressing";
        // material
        public const string FOLIAGE = "foliage";
        public const string FLOUR = "flour";
        public const string PRODUCE = "produce";
        // qualities
        public const string RIPPABLE = "rippable";
        // liquids
        public const string WATER = "water";
        public const string DISINFECTANT = "disinfectant";
        public const string BLOOD = "blood";
        public const string FAT = "fat";
        public const string OPIATE = "opiate";
        public const string CONDIMENT = "condiment";
        // fire
        public const string HEATSOURCE = "heatsource";
        public const string FIRESTARTER = "firestarter";
        public const string FLAMMABLE = "flammable";

    }
}