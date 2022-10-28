using System;
using UnityEngine;
namespace Gunball.MapObject
{
    public interface IPickup
    {
        public Player Owner { get; }
        public void Pickup(Player player);
        public void DoEffect();
        public void EndEffect();
    }
}