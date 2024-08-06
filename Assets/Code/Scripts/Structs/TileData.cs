using System;
using Code.Scripts.Enums;

namespace Code.Scripts.Structs
{
    public struct TileData : ICloneable
    {
        public Biome Biome { get; set; }
        public Direction Direction { get; set; }
        
        public RiverConfiguration RiverConfiguration { get; set; }

        public TileData(Biome biome, Direction direction, RiverConfiguration riverConfiguration)
        {
            Biome = biome;
            Direction = direction;
            RiverConfiguration = riverConfiguration;
        }

        public Object Clone() => new TileData(Biome, Direction, RiverConfiguration);
        
        public override string ToString() => "Biome: " + Biome + " | Direction: " + Direction + " | River config: " + RiverConfiguration;
    }
}