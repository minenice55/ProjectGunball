using System;
using UnityEngine;
namespace Gunball.MapObject
{
    public interface ITeamObject
    {
        enum Teams
        {
            Alpha,
            Bravo,
            Neutral,
            Other
        }
        GameObject gameObject { get; }
        public Teams ObjectTeam { get; }

        public void SetTeam(Teams team);

    }
}