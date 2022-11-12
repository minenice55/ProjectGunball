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
            Charlie,
            Delta,
            Neutral

        }

        public Teams ObjectTeam { get; }

    }
}