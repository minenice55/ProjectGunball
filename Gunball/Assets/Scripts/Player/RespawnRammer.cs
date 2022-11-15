using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Gunball.MapObject
{
    public class RespawnRammer : MonoBehaviour, ITeamObject
    {
        [SerializeField] Player owner;
        [SerializeField] Transform noPoseTarget;
        [SerializeField] Transform visTransform;
        [SerializeField] Animator anim;

        [SerializeField] Transform respawnPosition;

        [SerializeField] CinemachineVirtualCamera cutinCam;
        [SerializeField] CinemachineVirtualCamera aimingCam;

        CinemachinePOV pov;

        bool _aiming, redirected;
        Vector3 targetPosition;
        RaycastHit[] hitsBuffer = new RaycastHit[16];

        ITeamObject.Teams _team;

        NetworkedRespawnRammer _netRammer;
        public Vector3 AimingAt { get => targetPosition; }
        public Transform VisualModel { get => visTransform; }
        public bool Aiming { get => _aiming; }
        public ITeamObject.Teams ObjectTeam { get => _team; }

        // Start is called before the first frame update
        void Start()
        {
            pov = aimingCam.GetCinemachineComponent<CinemachinePOV>();
        }

        // Update is called once per frame
        void Update()
        {
            if (owner == null) return;
            if (_netRammer != null && !_netRammer.IsOwner) return;
            if (_aiming)
            {
                redirected = false;
                FindLaunchPosition(visTransform.position, owner.CameraDirection, 8);
                visTransform.LookAt(targetPosition);
                Debug.DrawLine(visTransform.position, targetPosition, Color.green);
                if (Input.GetButtonDown("Attack"))
                {
                    _aiming = false;
                    owner.FinishRespawnSequence(respawnPosition.position, targetPosition, pov.m_HorizontalAxis.Value + transform.rotation.eulerAngles.y);
                    anim.Play("RespawnFire");
                    if (_netRammer != null && _netRammer.IsOwner)
                        _netRammer.DoRammerFireServerRpc();
                }
            }
            else
            {
                pov.m_HorizontalAxis.Value = 0;
                pov.m_VerticalAxis.Value = 15;
            }
        }

        public void RemoveCameras()
        {
            Destroy(cutinCam.gameObject);
            Destroy(aimingCam.gameObject);
        }

        public void SetOwner(GameObject owner)
        {
            this.owner = owner.GetComponent<Player>();
            this.owner.SetRespawnRammer(this);
            this.owner.SetTeam(_team);
            if (_netRammer != null && _netRammer.IsOwner)
            {
                _netRammer.SetOwner(owner);
            }
        }

        public void StartRespawnSequence()
        {
            visTransform.LookAt(noPoseTarget);
            CinemachineSwitcher.SwitchTo(cutinCam);
            anim.Play("RespawnPrepare");
            owner.transform.position = respawnPosition.position;
            if (_netRammer != null && _netRammer.IsOwner)
                _netRammer.DoRammerPrepareServerRpc();
            Invoke("StartAimingSequence", 0.5f);
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

        public void PlayRammerPrepare()
        {
            anim.Play("RespawnPrepare");
        }

        public void PlayRammerFire()
        {
            anim.Play("RespawnFire");
        }

        float DistanceFromPoint(Vector3 point1, Vector3 point2)
        {
            return Mathf.Pow(point1.x - point2.x, 2) + Mathf.Pow(point1.y - point2.y, 2) + Mathf.Pow(point1.z - point2.z, 2);
        }

        void FindLaunchPosition(Vector3 origin, Vector3 direction, int tries)
        {
            tries--;
            if (tries < 0)
            {
                targetPosition = origin;
                return;
            }
            for (int i = 0; i < hitsBuffer.Length; i++)
            {
                hitsBuffer[i] = new RaycastHit();
            }
    
            int hits = Physics.RaycastNonAlloc(origin, direction.normalized, hitsBuffer, 100, LayerMask.GetMask("RespawnBlocker", "Ground", "Wall"), QueryTriggerInteraction.Collide);
            System.Array.Sort(hitsBuffer, (a, b) => DistanceFromPoint(origin, a.point).CompareTo(DistanceFromPoint(origin, b.point)));
            for (int i = 0; i < hitsBuffer.Length; i++)
            {
                if (hitsBuffer[i].collider == null) continue;
                if (hitsBuffer[i].collider.gameObject.CompareTag("RespawnBlocker"))
                {
                    RespawnBlocker rb = hitsBuffer[i].collider.GetComponent<RespawnBlocker>();
                    if (rb != null && rb.Axis != Vector3.zero)
                    {
                        if (rb.IsFromOOBRedirector && !redirected) continue;
                        if (rb.IsOOBFloor) { redirected = true; }
                        FindLaunchPosition(hitsBuffer[i].point, rb.Axis.normalized, tries);
                        return;
                    }
                    else
                    {
                        targetPosition = hitsBuffer[i].point;
                        return;
                    }
                    
                }
                else
                {
                    targetPosition = hitsBuffer[i].point;
                    return;
                }
            }
        }

        public void SetTeam(ITeamObject.Teams team)
        {
            _team = team;
        }
    }
}
