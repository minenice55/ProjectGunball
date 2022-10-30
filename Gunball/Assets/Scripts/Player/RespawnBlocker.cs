using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class RespawnBlocker : MonoBehaviour
    {
        [SerializeField] public Vector3 Axis;
        [SerializeField] public bool IsOOBFloor;
        [SerializeField] public bool IsFromOOBRedirector;
    }
}