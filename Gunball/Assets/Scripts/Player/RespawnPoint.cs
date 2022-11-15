using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class RespawnPoint : MonoBehaviour, ITeamObject
    {
        [SerializeField] bool IsForRammer = true;
        [SerializeField] ITeamObject.Teams Team;

        public Vector3 Position {get => transform.position;}
        public Quaternion Facing {get => transform.rotation;}

    Player _player;

        public void AssignPlayer(Player player)
        {
            _player = player;
        }

        public ITeamObject.Teams ObjectTeam
        {
            get
            {
                return Team;
            }
        }
    }
}