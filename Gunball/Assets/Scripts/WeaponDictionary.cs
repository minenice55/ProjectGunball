using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.WeaponSystem
{
    
    public class WeaponDictionary : MonoBehaviour
    {
        public static WeaponDictionary instance;
        [Serializable]
        public enum WeaponClass
        {
            Primary,
            Secondary,
            Special,
            Other
        }

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        [Serializable]
        public struct WeaponEntry
        {
            public string name;
            public WeaponClass weaponClass;
            public GameObject weaponManager;
        }
    }
}