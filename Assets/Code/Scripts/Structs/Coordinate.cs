using System;
using UnityEngine;

namespace Code.Scripts.Structs
{
    public readonly struct Coordinate : IComparable<Coordinate>
    {
        public int X { get; }
        public int Z { get; }

        public Coordinate(float x, float z)
        {
            X = Mathf.FloorToInt(x);
            Z = Mathf.FloorToInt(z);
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