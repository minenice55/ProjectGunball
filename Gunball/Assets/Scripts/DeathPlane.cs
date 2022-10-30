using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class DeathPlane : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            IShootableObject shootable = other.GetComponent<IShootableObject>();
            if (shootable != null)
            {
                shootable.DoDamage(shootable.Health);
                shootable.DoDeath();
            }
        }
    }
}