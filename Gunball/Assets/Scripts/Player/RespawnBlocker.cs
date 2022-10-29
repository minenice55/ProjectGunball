using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class RespawnBlocker : MonoBehaviour
    {
        [SerializeField] Vector3 Axis;
        [SerializeField] bool IsOOBFloor;
        [SerializeField] bool IsFromOOBRedirector;
    }
}