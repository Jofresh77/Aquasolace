using System;

namespace Code.Scripts.Structs
{
    public readonly struct Coordinate : IComparable<Coordinate>
    {
        public int X { get; }
        public int Z { get; }

        public Coordinate(float x, float z)
        {
            X = (int)(x - 0.5f);
            Z = (int)(z - 0.5f);
        }

        public Coordinate(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int CompareTo(Coordinate other)
        {
            if (Z < other.Z)
            {
                return -1;
            }

            return Z > other.Z ? 1 : X.CompareTo(other.X);
        }

        public override string ToString()
        {
            return X + " " + Z;
        }
    }
}