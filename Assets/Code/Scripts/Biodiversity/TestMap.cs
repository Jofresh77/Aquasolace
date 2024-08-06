using System;
using UnityEngine;

namespace Code.Scripts.Biodiversity
{
    public class TestMap : MonoBehaviour
    {
        [SerializeField] private Species[] species;

        private void Start()
        {
            Vector3[] positions =
            {
                new (5, 0, 5),
                new (5, 0, 10),
                new (5, 0, 15),
                new (5, 0, 20)
            };

            for (int i = 0; i < 4; i++)
            {
                //Instantiate(species[i]).InitTest(positions[i]);
            }
        }
    }
}