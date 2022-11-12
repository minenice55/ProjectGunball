using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class RespawnPoint : MonoBehaviour, ITeamObject
    {
        [SerializeField] bool IsForRammer = true;
        [SerializeField] ITeamObject.Teams Team;

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