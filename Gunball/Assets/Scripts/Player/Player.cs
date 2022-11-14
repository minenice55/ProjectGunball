﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Gunball.WeaponSystem;

namespace Gunball.MapObject
{
    public class Player : MonoBehaviour, IShootableObject, IDamageSource
    {
        [System.Flags]
        public enum FiringType
        {
            Primary = 1 << 0,
            Secondary = 1 << 1,
            Super = 1 << 2
        }

        public enum PlayerState
        {
            Active,
            Dead,
            Respawn,
            GameIntro,
            GameResult
        }

        #region Serialized Components
        [SerializeField] Camera playerCamera;
        [SerializeField] Transform playerCameraTarget;
        [SerializeField] Transform vsBallPos;
        [SerializeField] Transform vsWpnBallPos;
        [SerializeField] CinemachineFreeLook playerCineCamera;
        [SerializeField] GameObject visualModel;
        [SerializeField] PlayerParameters playPrm;
        [SerializeField] LayerMask gndCollisionFlags;
        [SerializeField] GuideManager guideMgr;
        [SerializeField] Vector3 aimOffset;
        [SerializeField] CapsuleCollider _playCollider;
        [SerializeField] RespawnRammer respawnRammer;
        #endregion

        #region Public Variables
        [SerializeField] public Transform weaponPos;
        [SerializeField] public string[] WeaponNames;
        [NonSerialized] public WeaponBase Weapon;
        [NonSerialized] public PlayerState State;
        [NonSerialized] public bool IsOnGround;
        [NonSerialized] public bool IsJumping;
        [NonSerialized] public bool IsPlayer = true;

        [NonSerialized] public IEnumerator FireCoroutine;
        [NonSerialized] public bool InFireCoroutine = false;

        [NonSerialized] public GunBall VsBall;
        #endregion

        #region Private Variables
        float _hp;
        bool _isOnSlope, _isOnSlopeSteep, _onWpSwitchBlock;
        float _dt;
        float _firingTime;
        float _fireRelaxTime = Single.MaxValue;
        float requestMoveTimer;
        float ignoreMoveTimer;
        Rigidbody _playController;
        Vector3 _input;
        Vector3 _aimingInput;
        Vector3 _gndNormal;
        RaycastHit _gndHit;
        Quaternion _onJmpRotation;
        FiringType _fireKeys;

        Vector3 _startPos;
        float respawnRamTime = 0f;
        Vector3 respawnStart, respawnEnd;

        Dictionary<string, GameObject> kitWeapons;

        NetworkedPlayer _netPlayer;
        #endregion

        #region Public Properties
        public Vector3 CameraDirection { get { return playerCamera.transform.forward; } }
        public CinemachineVirtualCamera CineCamera { get { return playerCineCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>(); } }
        public Vector3 Velocity { get { return _playController.velocity; } set { _playController.velocity = value; } }
        public float MaxHealth { get { return playPrm.Max_Health; } }
        public float Health { get { return _hp; } set { _hp = Mathf.Clamp(value, 0f, MaxHealth); } }
        public bool IsDead { get { return _hp <= 0; } }
        public Transform Transform { get { return transform; } }
        public IShootableObject.ShootableType Type { get { return IShootableObject.ShootableType.Player; } }
        public Transform BallPickupPos { get => vsBallPos; }
        public Transform BallSpawnPos { get => vsWpnBallPos; }
        public FiringType CurrentFireKeys { get => _fireKeys; }
        public bool InAction { get => Weapon.GetPlayerInAction() || InFireCoroutine || (int)CurrentFireKeys != 0; }
        public GameObject VisualModel { get => visualModel; }
        public Vector3 AimingAngle { get => new Vector3(playerCamera.transform.rotation.eulerAngles.x, playerCamera.transform.rotation.eulerAngles.y, 0); set => _aimingInput = value; }
        public float SpeedStat { get => IsOnGround ? playPrm.Move_RunSpeed : playPrm.Move_AirSpeed; }
        public float AccelStat { get => IsOnGround ? playPrm.Move_RunAccel : playPrm.Move_AirAccel; }
        #endregion

        #region Built-Ins
        void Start()
        {
            _playController = GetComponent<Rigidbody>();
            _netPlayer = GetComponent<NetworkedPlayer>();
            CreateKitWeapons();
            if (_netPlayer != null && !_netPlayer.IsOwner) return;
            
            _startPos = transform.position - Vector3.up * 1.5f;

            guideMgr.SetCamera(playerCamera);
            Cursor.lockState = CursorLockMode.Locked;

            _hp = 0;
            ChangeWeapon(null);
            visualModel.SetActive(false);
            SetNoClip(true);
            _playController.velocity = Vector3.zero;
            StartRespawnSequence();
        }

        void Update()
        {
            if (_netPlayer != null && !_netPlayer.IsOwner)
            {
                GetGrounded();
                GetSlopeNormal();
            }
            else
            {
                //TEMPORARY
                if (Input.GetKey(KeyCode.Escape))
                    Cursor.lockState = CursorLockMode.None;

                PollInput();
                TickTimers();
                if ((!IsDead) && respawnRamTime <= 0f)
                {
                    DoPlayerMovement();
                    DoWeaponLogic();
                }
            }
        }

        public void CreateKitWeapons()
        {
            kitWeapons = new Dictionary<string, GameObject>();
            if (_netPlayer != null)
            {
                if (_netPlayer.IsOwner)
                    _netPlayer.SetupKitWeaponsServerRpc();
            }
            else
            {
                for (int i = 0; i < WeaponNames.Length; i++)
                {
                    GameObject WpGO = GameCoordinator.instance.CreatePlayerWeapon(WeaponNames[i]);
                    RegisterKitWeapon(WeaponNames[i], WpGO);
                }
            }
        }

        public void RegisterKitWeapon(string name, GameObject wpn)
        {
            kitWeapons.Add(name, wpn);
        }

        public void ChangeWeapon(string weaponName)
        {
            if (Input.GetButton("Attack"))
            {
                _onWpSwitchBlock = true;
            }
            if (InFireCoroutine)
            {
                StopCoroutine(FireCoroutine);
                FireCoroutine = null;
                InFireCoroutine = false;
            }
            if (weaponName == null)
            {
                SetNullWeapon();
                return;
            }

            GameObject WpGO;
            if (kitWeapons.ContainsKey(weaponName))
            {
                //check our kit weapons
                WpGO = kitWeapons[weaponName];
            }
            else
            {
                //use global weapon pool
                WpGO = GameCoordinator.instance.CreateGlobalWeapon(weaponName);
            }
            if (WpGO == null)
            {
                SetNullWeapon();
                throw new Exception("ChangeWeapon: coordinator couldn't get weapon " + weaponName);
            }
            WeaponBase newWeapon = WpGO.GetComponent<WeaponBase>();
            if (newWeapon == null)
            {
                SetNullWeapon();
                throw new Exception("ChangeWeapon: prefab is not a weapon base!");
            }
            newWeapon.SetOwner(gameObject);
            Weapon = newWeapon;
            guideMgr.SetWeapon(Weapon);
            guideMgr.UpdateGuide();
            if (_netPlayer != null)
            {
                WpGO.GetComponent<NetworkedWeapon>().SetOwnerServerRpc();
            }
        }

        void SetNullWeapon()
        {
            if (Weapon != null && Weapon.gameObject != null && !Weapon.IsGlobalWeapon)
            {
                if (_netPlayer != null) Weapon.gameObject.GetComponent<NetworkedWeapon>().RemoveOwnerServerRpc();
            }
            Weapon = null;
            guideMgr.SetWeapon(Weapon);
            guideMgr.UpdateGuide();
        }

        public void ResetWeapon()
        {
            if (WeaponNames.Length > 0)
            {
                ChangeWeapon(WeaponNames[0]);
            }
            else
            {
                ChangeWeapon(null);
            }
        }
        #endregion

        #region Collision
        void GetGrounded()
        {
            if (IsJumping)
            {
                IsOnGround = false;
                return;
            }
            Vector3 direction;
            if (_isOnSlopeSteep)
                direction = Vector3.zero;
            else
                direction = (Vector3.Scale(Velocity, new Vector3(1, 0, 1)) * _dt);
            Vector3 p1 = transform.position + _playCollider.center + Vector3.down * (_playCollider.height * 0.5f - _playCollider.radius - 0.05f) + direction;
            Vector3 p2 = p1 + Vector3.up * (_playCollider.height - _playCollider.radius * 2f - 0.05f);

            IsOnGround = Physics.CapsuleCast(p1, p2,
                _playCollider.radius * 0.99f,
                Vector3.down + direction, out _gndHit,
                playPrm.Gnd_RayLength + 0.05f,
                gndCollisionFlags, QueryTriggerInteraction.Ignore);
        }
        void GetSlopeNormal()
        {
            if (IsJumping || !IsOnGround)
            {
                _gndNormal = Vector3.up;
                _isOnSlope = false;
                _isOnSlopeSteep = false;
                return;
            }
            _gndNormal = _gndHit.normal;
            float ang = Vector3.Angle(Vector3.up, _gndNormal);
            _isOnSlope = ang <= playPrm.Gnd_SlopeLimit && ang != 0f;
            _isOnSlopeSteep = ang > playPrm.Gnd_SlopeLimit && ang < 90f;
        }
        #endregion

        #region Methods
        void TickTimers()
        {
            _dt = Time.deltaTime;

            if (respawnRamTime > 0f)
            {
                Health = playPrm.Max_Health;
                respawnRamTime -= _dt;
                transform.position = Vector3.Lerp(respawnStart, respawnEnd, 1f - Mathf.Clamp01(respawnRamTime));
                if (respawnRamTime <= 0f)
                {
                    FinishRespawnRam();
                    respawnRamTime = 0f;
                    Vector3 xzDir = respawnEnd - respawnStart;
                    xzDir.y = 0f;
                    _playController.velocity = xzDir.normalized * 10f + Vector3.up * 2f;
                }
            }

            if (Input.GetButton("Attack"))
            {
                //TEMPORARY
                Cursor.lockState = CursorLockMode.Locked;
                if (!_onWpSwitchBlock)
                {
                    _fireKeys |= FiringType.Primary;
                    _firingTime += _dt;
                }
                else
                {
                    _firingTime = 0f;
                    _fireRelaxTime += _dt;
                }
            }
            else
            {
                if (_onWpSwitchBlock) _onWpSwitchBlock = false;
                _firingTime = 0f;
                _fireRelaxTime += _dt;
            }

            if (_input.magnitude > 0.05f && Math.Abs(playerCineCamera.m_XAxis.m_InputAxisValue) <= 0.05f && _firingTime <= 0f)
                requestMoveTimer += _dt;
            else
                requestMoveTimer = 0f;
            
            ignoreMoveTimer -= _dt;
            if (ignoreMoveTimer < 0f) ignoreMoveTimer = 0f;
        }

        void PollInput()
        {
            _fireKeys = 0;
            _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }

        void DoPlayerMovement()
        {
            GetGrounded();
            GetSlopeNormal();

            if (IsOnGround)
            {
                if (_onJmpRotation != Quaternion.identity)
                {
                    visualModel.transform.rotation = Quaternion.Euler(0, _onJmpRotation.eulerAngles.y, 0);
                    _onJmpRotation = Quaternion.identity;
                }

                if (IsJumping)
                    IsJumping = false;
            }
            else if (_onJmpRotation == Quaternion.identity)
            {
                _onJmpRotation = Quaternion.Euler(0, visualModel.transform.rotation.eulerAngles.y, 0);
            }

            if (Input.GetButtonDown("Jump") && !IsJumping && IsOnGround)
            {
                DoJump();
            }
            if (Input.GetButtonUp("Jump") && IsJumping && !IsOnGround && Velocity.y > playPrm.Jump_Shortening)
            {
                DoJumpShortening();
            }


            bool attacking = (Weapon != null) ? Weapon.GetPlayerInAction() : Input.GetButton("Attack");
            if (_input.magnitude > 0f)
            {
                if (!attacking)
                    DoCameraAutoTurn();
                if (!_isOnSlopeSteep)
                    DoMove(SpeedStat, AccelStat);
            }
            DoPlayerModelRotation(SpeedStat, attacking);
            if (_isOnSlopeSteep)
            {
                //slide away from the slope
                DoSlopeSliding(AccelStat);
            }

            if (Velocity.y < 0f)
            {
                if (IsJumping)
                    IsJumping = false;
            }
            if (!IsOnGround)
            {
                if (Velocity.y < -playPrm.Move_TerminalGravity)
                    _playController.velocity = new Vector3(Velocity.x, -playPrm.Move_TerminalGravity, Velocity.z);
            }
        }

        void DoWeaponLogic()
        {
            // from camera orientation set weapon orientation
            //set vertical aim to the aim offset
            playerCameraTarget.rotation = playerCamera.transform.rotation;
            if (Weapon != null) 
                Weapon.SetFacingDirection((playerCameraTarget.forward + (playerCameraTarget.rotation * aimOffset)).normalized);

            guideMgr.UpdateGuide();

            if (Weapon != null && Weapon.RefireCheck(_firingTime, _fireRelaxTime, Weapon.WpPrm))
            {
                _fireRelaxTime = 0f;
                Weapon.StartFireSequence(this);
            }
            else if (_firingTime > 0)
            {
                _fireRelaxTime += _dt;
            }
        }

        void DoCameraAutoTurn()
        {
            if (requestMoveTimer > playPrm.Camera_AutoTurnTime)
            {
                float intensity = Mathf.Clamp01(requestMoveTimer - playPrm.Camera_AutoTurnTime);
                playerCineCamera.m_XAxis.Value += _input.x * intensity * playPrm.Camera_AutoTurnSpeed * _dt;
                // playerCameraTarget.Rotate(0, _input.x * playPrm.Camera_AutoTurnSpeed * _dt * intensity, 0);
            }
        }

        void DoSlopeSliding(float accel)
        {
            Vector3 slopeDown = Vector3.up - _gndNormal * Vector3.Dot(Vector3.up, _gndNormal);
            Vector3 fInp = Vector3.Scale(playerCamera.transform.TransformDirection(_input), new Vector3(1, 0, 1));
            fInp = Vector3.ProjectOnPlane(fInp.normalized, _gndNormal);

            _playController.AddForce(slopeDown * -playPrm.Move_SlideSpeed + fInp * accel, ForceMode.Acceleration);
        }

        void DoPlayerModelRotation(float speed, bool faceCam = false)
        {
            if (Weapon == null) faceCam = false;
            if (faceCam)
            {
                Quaternion targetRot = Quaternion.Euler(0, AimingAngle.y, 0);
                if (IsOnGround)
                {
                    visualModel.transform.rotation = Quaternion.Slerp(visualModel.transform.rotation, targetRot, 90f * _dt);
                }
                else
                {
                    _onJmpRotation = targetRot;
                    targetRot = TiltRotationTowardsVelocity(_onJmpRotation, Vector3.up, Velocity, speed * 32f);
                    visualModel.transform.rotation = Quaternion.Slerp(visualModel.transform.rotation, targetRot, 90f * _dt);
                }
            }
            else
            {
                if (_input.magnitude <= 0f) return;
                Vector3 fInp = Vector3.Scale(playerCamera.transform.TransformDirection(_input), new Vector3(1, 0, 1));
                if (IsOnGround)
                {
                    // ease to new direction
                    visualModel.transform.forward = Vector3.Slerp(visualModel.transform.forward, fInp, 15f * _dt);
                }
                else
                {
                    Quaternion targetRot = TiltRotationTowardsVelocity(_onJmpRotation, Vector3.up, Velocity, speed * 32f);
                    visualModel.transform.rotation = Quaternion.Slerp(visualModel.transform.rotation, targetRot, 8f * _dt);
                }
            }
        }

        void DoMove(float speed, float accel)
        {
            // if (_netPlayer.IsOwner)
            //     Debug.Log("DoMove: " + _input + " kine: " + _playController.isKinematic);
            Vector3 vel = new Vector3(Velocity.x, 0, Velocity.z);
            Vector3 moveDir = playerCamera.transform.TransformDirection(_input);
            moveDir.y = 0f;

            if (_isOnSlope && !IsJumping)
            {
                vel = Velocity;
                moveDir = Vector3.ProjectOnPlane(moveDir.normalized, _gndNormal);
            }

            //get angle between velocity and input
            float a = Vector3.Angle(vel, moveDir.normalized * accel);
            float mag = vel.magnitude * Mathf.Cos(a * Mathf.Deg2Rad);
            if (mag < speed - (accel * _dt))
            {
                _playController.AddForce(moveDir.normalized * accel, ForceMode.Force);
            }
            else if (speed - (accel * _dt) <= mag && mag < speed)
            {
                _playController.AddForce(moveDir.normalized * (speed - mag), ForceMode.Force);
            }
            else
            {
                _playController.AddForce(moveDir.normalized * 0, ForceMode.Force);
            }

            if (IsOnGround && ignoreMoveTimer <= 0)
                DoSpeedCap(speed);
        }

        void DoSpeedCap(float speed)
        {
            if (_isOnSlope && !IsJumping)
            {
                if (Velocity.magnitude > speed)
                    _playController.velocity = Velocity.normalized * speed;
            }
            else
            {
                Vector3 fVel = new Vector3(Velocity.x, 0, Velocity.z);
                if (fVel.magnitude > speed)
                {
                    fVel = fVel.normalized * speed;
                    _playController.velocity = new Vector3(fVel.x, Velocity.y, fVel.z);
                }
            }
        }
        void DoJump()
        {
            IsJumping = true;
            _playController.velocity = new Vector3(Velocity.x, 0, Velocity.z);
            _playController.AddForce(_gndNormal * playPrm.Jump_Velocity, ForceMode.Impulse);
            IsOnGround = false;
            _onJmpRotation = Quaternion.Euler(0, visualModel.transform.rotation.eulerAngles.y, 0);
            GetSlopeNormal();
        }

        void DoJumpShortening()
        {
            IsJumping = false;
            _playController.velocity = new Vector3(Velocity.x, playPrm.Jump_Shortening, Velocity.z);
        }

        void SetNoClip(bool noClip = false)
        {
            _playController.detectCollisions = !noClip;
            _playController.useGravity = !noClip;
            _playController.isKinematic = noClip;

            _playCollider.enabled = !noClip;
            //disable colliders
            foreach (Collider c in visualModel.GetComponentsInChildren<Collider>())
            {
                c.enabled = !noClip;
            }
        }

        void StartRespawnSequence()
        {
            if (respawnRammer == null)
            {
                Health = playPrm.Max_Health;
                visualModel.SetActive(true);
                transform.position = _startPos;
                visualModel.transform.LookAt(_startPos + Vector3.up);
                respawnRamTime = 0.66f;
                respawnStart = _startPos;
                respawnEnd = _startPos + Vector3.up;
                CinemachineSwitcher.SwitchTo(playerCineCamera);
            }
            else
            {
                respawnRammer.StartRespawnSequence();
            }
        }

        public void FinishRespawnSequence(Vector3 startPos, Vector3 targetPos, float xAxis)
        {
            Health = playPrm.Max_Health;
            visualModel.SetActive(true);
            transform.position = startPos;
            visualModel.transform.LookAt(targetPos);
            playerCineCamera.m_XAxis.Value = xAxis;
            respawnRamTime = 0.66f;
            respawnStart = startPos;
            respawnEnd = targetPos + Vector3.up * 1f;
            CinemachineSwitcher.SwitchTo(playerCineCamera);
        }

        void FinishRespawnRam()
        {
            Health = playPrm.Max_Health;
            SetNoClip(false);
            ResetWeapon();
        }
        #endregion

        #region Helpers
        // http://answers.unity.com/answers/1498260/view.html
        public static Quaternion TiltRotationTowardsVelocity(Quaternion cleanRotation, Vector3 referenceUp, Vector3 vel, float velMagFor45Degree)
        {
            Vector3 rotAxis = Vector3.Cross(referenceUp, vel);
            float tiltAngle = Mathf.Atan(vel.magnitude / velMagFor45Degree) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(tiltAngle, rotAxis) * cleanRotation;    //order matters
        }
        #endregion

        #region Inherited Methods
        public void Knockback(Vector3 force, Vector3 pos)
        {
            if (IsDead) return;
            IsJumping = false;
            _playController.AddForceAtPosition(force, pos, ForceMode.Impulse);
        }

        public void SetKnockbackTimer(float time)
        {
            ignoreMoveTimer = time;
        }

        public void DoDamage(float damage, IDamageSource source = null)
        {
            if (IsDead) return;
            if (respawnRamTime > 0f) return;
            // log damage
            Debug.Log("Took " + damage + " damage. Health remaining: " + (Health - damage));
            Health -= damage;
            if (IsDead)
            {
                // log death
                Debug.Log("Player died");
                // respawn
                DoDeath(source);
            }
        }

        public void RecoverDamage(float healing, IDamageSource source = null)
        {
            if (IsDead) return;
            if (respawnRamTime > 0f) return;
            // log recovery
            Debug.Log("Recovered " + healing + " damage");
            Health += healing;
        }

        public void DoDeath(IDamageSource cause = null)
        {
            Health = 0;
            if (VsBall != null)
            {
                VsBall.DeathDrop();
            }
            ChangeWeapon(null);
            visualModel.SetActive(false);
            SetNoClip(true);
            _playController.velocity = Vector3.zero;
            Invoke("StartRespawnSequence", 1f);
            return;
        }

        public void InflictKnockback(Vector3 force, Vector3 pos, float knockbackTimer, IShootableObject target)
        {
            if (_netPlayer != null)
            {
                if (_netPlayer.IsOwner)
                    _netPlayer.NetInflictKnockback(force, pos, knockbackTimer, target);
            }
            target.Knockback(force, pos);
            target.SetKnockbackTimer(knockbackTimer);
        }

        public void InflictDamage(float damage, IShootableObject target)
        {
            if (_netPlayer != null)
            {
                if (_netPlayer.IsOwner)
                    _netPlayer.NetInflictDamage(damage, target);
            }
            target.DoDamage(damage, this);
        }

        public void InflictHealing(float healing, IShootableObject target)
        {
            if (_netPlayer != null)
            {
                if (_netPlayer.IsOwner)
                    _netPlayer.NetInflictHealing(healing, target);
            }
            target.RecoverDamage(healing, this);
        }
        #endregion

        #region Parameter Classes
        [Serializable]
        public class PlayerParameters
        {
            [Header("Health")]
            public float Max_Health = 100f;
            [Header("Movement")]
            public float Move_RunSpeed = 5.0f;
            public float Move_RunAccel = 100.0f;
            public float Move_AirSpeed = 5.0f;
            public float Move_AirAccel = 100.0f;
            public float Move_SlideSpeed = 1f;
            public float Move_TerminalGravity = 20f;

            [Header("Jumping")]
            public float Jump_Velocity = 5.0f;
            public float Jump_Shortening = 1f;


            [Header("Misc")]
            public float Camera_AutoTurnTime = 2f;
            public float Camera_AutoTurnSpeed = 45f;
            public float Gnd_RayLength = 0.2f;
            public float Gnd_SlopeLimit = 40f;
        }
        #endregion
    }
}