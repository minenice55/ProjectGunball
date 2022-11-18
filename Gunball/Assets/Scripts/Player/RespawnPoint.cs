using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class RespawnPoint : MonoBehaviour, ITeamObject
    {
        [SerializeField] public bool IsForRammer = true;
        [SerializeField] ITeamObject.Teams Team;

        public Vector3 Position {get => transform.position;}
        public Quaternion Facing {get => transform.rotation;}

        public ITeamObject.Teams ObjectTeam
        {
            get
            {
                return Team;
            }
        }

        public void SetTeam(ITeamObject.Teams team){}
    }
}