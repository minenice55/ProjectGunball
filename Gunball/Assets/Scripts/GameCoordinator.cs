using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;

namespace Gunball
{
    using static WeaponDictionary;
    public class GameCoordinator : MonoBehaviour
    {
        public static GameCoordinator instance;
        public List<WeaponEntry> weapons;
        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}