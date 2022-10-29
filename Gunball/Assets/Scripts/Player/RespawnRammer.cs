using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Gunball.MapObject
{
    public class RespawnRammer : MonoBehaviour
    {
        [SerializeField] Player owner;
        [SerializeField] Transform visTransform;
        [SerializeField] Animator anim;

        [SerializeField] Transform respawnPosition;

        [SerializeField] CinemachineVirtualCamera cutinCam;
        [SerializeField] CinemachineVirtualCamera aimingCam;

        CinemachinePOV pov;

        bool _aiming;
        Vector3 targetPosition;

        // Start is called before the first frame update
        void Start()
        {
            pov = aimingCam.GetCinemachineComponent<CinemachinePOV>();
        }

        // Update is called once per frame
        void Update()
        {
            if (owner == null) return;
            if (_aiming)
            {

            }
            else
            {
                pov.m_HorizontalAxis.Value = 0;
                pov.m_VerticalAxis.Value = 15;
            }
        }

        public void StartRespawnSequence()
        {
            CinemachineSwitcher.SwitchTo(cutinCam);
            anim.Play("RespawnPrepare");
            owner.transform.position = respawnPosition.position;
            Invoke("StartAimingSequence", 0.25f);
        }

        public void StartAimingSequence()
        {
            CinemachineSwitcher.SwitchTo(aimingCam);
            Invoke("EnableAiming", 0.25f);
        }

        public void EnableAiming()
        {
            _aiming = true;
        }
    }
}
