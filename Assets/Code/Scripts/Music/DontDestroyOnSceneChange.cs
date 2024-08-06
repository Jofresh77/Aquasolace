using System;
using UnityEngine;

namespace Code.Scripts.Music
{
    public class DontDestroyOnSceneChange : MonoBehaviour
    {
        private void Start()
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag("MenuMusic");

            if (objs.Length > 1)
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
        }
    }
}
