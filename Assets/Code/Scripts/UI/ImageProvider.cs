using Code.Scripts.Enums;
using UnityEngine;

namespace Code.Scripts.UI
{
    public static class ImageProvider
    {
        public static Texture2D GetImageFromBiome(Biome biome)
        {
            switch (biome)
            {
                case Biome.Meadow:
                    return Resources.Load<Texture2D>("meadow_preview");
                case Biome.Farmland:
                    return Resources.Load<Texture2D>("farmland_preview");
                case Biome.ForestPine:
                    return Resources.Load<Texture2D>("forest_pine_preview");
                case Biome.ForestDeciduous:
                    return Resources.Load<Texture2D>("forest_deciduous_preview");
                case Biome.ForestMixed:
                    return Resources.Load<Texture2D>("forest_mixed_preview");
                case Biome.River:
                    return Resources.Load<Texture2D>("river_straight_preview");
                case Biome.Sealed:
                default:
                    // should be never executed, sealed surface spreading should be handled as an event
                    return Resources.Load<Texture2D>("nabu_mascot_2");
            }
        }
        
        public static Texture2D GetImageFromEvent(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case GameEvent.SealedSurfaceSpreading:
                    return Resources.Load<Texture2D>("nabu_mascot_2"); // todo: add preview of sealed surface spreading
                case GameEvent.BufoBufoAppeared:
                    // return Resources.Load<Texture2D>("bufo_bufo_preview");
                    return Resources.Load<Texture2D>("nabu_mascot_2"); // todo: change!
                default:
                    // this should be never executed
                    return Resources.Load<Texture2D>("nabu_mascot_2");
            }
        }
    }
}