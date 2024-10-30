using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.Movement;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    public class RagdollController : MonoBehaviour
    {
        const float c_defaultFriction = 0.6f;

        public static RagdollController Instance { get; private set; } = null;

        Transform m_avatarTransform = null;
        Transform m_hips = null;
        VRIK m_vrIK = null;
        bool m_applyHipsPosition = false;
        bool m_applyHipsRotation = false;

        bool m_avatarReady = false;
        bool m_ragdolled = false;
        bool m_forcedSwitch = false;
        Coroutine m_initTask = null;

        Transform m_puppet = null;
        Transform m_puppetRoot = null;
        BipedRagdollReferences m_puppetReferences;
        readonly List<RagdollBodypartHandler> m_ragdollBodyHandlers = null;
        readonly List<System.Tuple<Transform, Transform>> m_boneLinks = null;
        readonly List<System.Tuple<CharacterJoint, Vector3>> m_jointAnchors = null;

        RagdollToggle m_avatarRagdollToggle = null; // Custom component available for editor
        AvatarBoolParameter m_ragdolledParameter = null;
        PhysicMaterial m_physicsMaterial = null;

        bool m_inAir = false;
        bool m_reachedGround = true;
        float m_groundedTime = 0f;
        float m_downTime = float.MinValue;

        Vector3 m_lastRagdollPosition;
        Vector3 m_lastSeatPositon;
        Vector3 m_seatVelocity;
        Plane m_playerPlane;

        internal RagdollController()
        {
            m_ragdollBodyHandlers = new List<RagdollBodypartHandler>();
            m_boneLinks = new List<System.Tuple<Transform, Transform>>();
            m_jointAnchors = new List<System.Tuple<CharacterJoint, Vector3>>();
            m_playerPlane = new Plane();
        }

        // Unity events
        void Awake()
        {
            if(Instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            this.gameObject.layer = LayerMask.NameToLayer("PlayerLocal");

            m_physicsMaterial = new PhysicMaterial("Ragdoll");
            m_physicsMaterial.dynamicFriction = c_defaultFriction;
            m_physicsMaterial.staticFriction = c_defaultFriction;
            m_physicsMaterial.frictionCombine = PhysicMaterialCombine.Average;
            m_physicsMaterial.bounciness = 0f;
            m_physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;

            m_puppet = new GameObject("[Puppet]").transform;
            m_puppet.parent = this.transform;
            m_puppet.localPosition = Vector3.zero;
            m_puppet.localRotation = Quaternion.identity;

            Settings.OnMovementDragChanged.AddListener(this.OnMovementDragChanged);
            Settings.OnAngularDragChanged.AddListener(this.OnAngularDragChanged);
            Settings.OnGravityChanged.AddListener(this.OnGravityChanged);
            Settings.OnSlipperinessChanged.AddListener(this.OnPhysicsMaterialChanged);
            Settings.OnBouncinessChanged.AddListener(this.OnPhysicsMaterialChanged);
            Settings.OnBuoyancyChanged.AddListener(this.OnBuoyancyChanged);
            Settings.OnFallDamageChanged.AddListener(this.OnFallDamageChanged);
            Settings.OnGestureGrabChanged.AddListener(this.OnGestureGrabChanged);

            GameEvents.OnAvatarClear.AddListener(this.OnAvatarClear);
            GameEvents.OnAvatarSetup.AddListener(this.OnAvatarSetup);
            GameEvents.OnAvatarPreReuse.AddListener(this.OnAvatarPreReuse);
            GameEvents.OnAvatarPostReuse.AddListener(this.OnAvatarPostReuse);
            GameEvents.OnIKScaling.AddListener(this.OnAvatarScaling);
            GameEvents.OnSeatPreSit.AddListener(this.OnSeatPreSit);
            GameEvents.OnCalibrationStart.AddListener(this.OnCalibrationStart);
            GameEvents.OnWorldPreSpawn.AddListener(this.OnWorldPreSpawn);
            GameEvents.OnCombatPreDown.AddListener(this.OnCombatPreDown);
            GameEvents.OnFlightChange.AddListener(this.OnFlightChange);
            GameEvents.OnIKOffsetUpdate.AddListener(this.OnIKOffsetUpdate);
            BetterBetterCharacterController.OnTeleport.AddListener(this.OnPlayerTeleport);

            ModUi.OnSwitchChanged.AddListener(this.SwitchRagdoll);
            RemoteGesturesManager.OnGestureState.AddListener(this.OnRemoteGestureStateChanged);
        }

        void OnDestroy()
        {
            if(Instance == this)
                Instance = null;

            if(m_initTask != null)
                StopCoroutine(m_initTask);
            m_initTask = null;

            if(m_puppet != null)
                Object.Destroy(m_puppet);
            m_puppet = null;

            m_puppetRoot = null;
            m_ragdollBodyHandlers.Clear();
            m_boneLinks.Clear();
            m_jointAnchors.Clear();
            m_avatarRagdollToggle = null;

            if(m_physicsMaterial != null)
                Object.Destroy(m_physicsMaterial);
            m_physicsMaterial = null;

            Settings.OnMovementDragChanged.RemoveListener(this.OnMovementDragChanged);
            Settings.OnAngularDragChanged.RemoveListener(this.OnAngularDragChanged);
            Settings.OnGravityChanged.RemoveListener(this.OnGravityChanged);
            Settings.OnSlipperinessChanged.RemoveListener(this.OnPhysicsMaterialChanged);
            Settings.OnBouncinessChanged.RemoveListener(this.OnPhysicsMaterialChanged);
            Settings.OnBuoyancyChanged.RemoveListener(this.OnBuoyancyChanged);
            Settings.OnFallDamageChanged.RemoveListener(this.OnFallDamageChanged);
            Settings.OnGestureGrabChanged.RemoveListener(this.OnGestureGrabChanged);

            GameEvents.OnAvatarClear.RemoveListener(this.OnAvatarClear);
            GameEvents.OnAvatarSetup.RemoveListener(this.OnAvatarSetup);
            GameEvents.OnAvatarPreReuse.RemoveListener(this.OnAvatarPreReuse);
            GameEvents.OnAvatarPostReuse.RemoveListener(this.OnAvatarPostReuse);
            GameEvents.OnIKScaling.RemoveListener(this.OnAvatarScaling);
            GameEvents.OnSeatPreSit.RemoveListener(this.OnSeatPreSit);
            GameEvents.OnCalibrationStart.RemoveListener(this.OnCalibrationStart);
            GameEvents.OnWorldPreSpawn.RemoveListener(this.OnWorldPreSpawn);
            GameEvents.OnCombatPreDown.RemoveListener(this.OnCombatPreDown);
            GameEvents.OnFlightChange.RemoveListener(this.OnFlightChange);
            GameEvents.OnIKOffsetUpdate.RemoveListener(this.OnIKOffsetUpdate);
            BetterBetterCharacterController.OnTeleport.RemoveListener(this.OnPlayerTeleport);

            ModUi.OnSwitchChanged.RemoveListener(this.SwitchRagdoll);
            RemoteGesturesManager.OnGestureState.RemoveListener(this.OnRemoteGestureStateChanged);
        }

        void Update()
        {
            if(m_avatarReady)
            {
                if(!m_ragdolled && Settings.FallDamage && !BetterBetterCharacterController.Instance.IsFlying() && !BetterBetterCharacterController.Instance.IsSitting())
                {
                    bool l_grounded = BetterBetterCharacterController.Instance.IsGrounded();
                    bool l_inWater = BetterBetterCharacterController.Instance.IsInWater();
                    if(m_inAir && l_grounded && !l_inWater && (BetterBetterCharacterController.Instance.characterMovement.landedVelocity.magnitude >= Settings.FallLimit))
                        Ragdoll();

                    m_inAir = !(l_grounded || l_inWater);
                }

                if(!m_ragdolled && BetterBetterCharacterController.Instance.IsSitting()) // Those seats without velocity, smh
                {
                    CVRSeat l_seat = BetterBetterCharacterController.Instance.GetCurrentSeat();
                    if(l_seat != null)
                    {
                        Vector3 l_pos = l_seat.transform.position;
                        m_seatVelocity = (l_pos - m_lastSeatPositon) / Time.deltaTime;
                        m_lastSeatPositon = l_pos;
                    }
                }

                if(m_ragdolled)
                {
                    BodySystem.TrackingPositionWeight = 0f;
                    BetterBetterCharacterController.Instance.PauseGroundConstraint();
                    BetterBetterCharacterController.Instance.ResetAllForces();
                    PlayerSetup.Instance.animatorManager.CancelEmote = true;
                }

                if(!m_ragdolled && !m_reachedGround && (BetterBetterCharacterController.Instance.IsOnGround() || BetterBetterCharacterController.Instance.IsInWater() || BetterBetterCharacterController.Instance.IsSitting()))
                {
                    m_groundedTime += Time.unscaledDeltaTime;
                    if(m_groundedTime >= 0.25f)
                        m_reachedGround = true;
                }

                if(m_ragdolled && Settings.AutoRecover)
                {
                    m_downTime += Time.unscaledDeltaTime;
                    if(m_downTime >= Settings.RecoverDelay)
                    {
                        Unragdoll();
                        m_downTime = float.MinValue; // One attempt to recover
                    }
                }

                if((m_avatarRagdollToggle != null) && m_avatarRagdollToggle.isActiveAndEnabled && m_avatarRagdollToggle.shouldOverride && (m_ragdolled != m_avatarRagdollToggle.isOn))
                    SwitchRagdoll();

                if(Settings.Hotkey && Input.GetKeyDown(Settings.HotkeyKey) && !ViewManager.Instance.IsAnyMenuOpen)
                    SwitchRagdoll();

                if(m_ragdolled && CVRInputManager.Instance.jump && Settings.JumpRecover)
                    Unragdoll();
            }
        }

        void LateUpdate()
        {
            if(m_avatarReady)
            {
                if(m_ragdolled)
                {
                    MovePlayer();

                    foreach(var l_link in m_boneLinks)
                        l_link.Item1.CopyGlobal(l_link.Item2);
                }
                else
                {
                    if(m_vrIK == null)
                    {
                        m_puppetRoot.position = m_avatarTransform.position;
                        m_puppetRoot.rotation = m_avatarTransform.rotation;

                        foreach(var l_link in m_boneLinks)
                            l_link.Item2.CopyGlobal(l_link.Item1);
                    }
                }
            }
        }

        // Game events
        void OnAvatarClear()
        {
            if(m_initTask != null)
            {
                StopCoroutine(m_initTask);
                m_initTask = null;
            }

            if(m_ragdolled)
            {
                TryRestoreMovement();
                BodySystem.TrackingPositionWeight = 1f;
            }

            if(m_puppetRoot != null)
                Object.Destroy(m_puppetRoot.gameObject);
            m_puppetRoot = null;

            m_avatarTransform = null;
            m_hips = null;
            m_vrIK = null;
            m_applyHipsPosition = false;
            m_ragdolled = false;
            m_avatarReady = false;
            m_avatarRagdollToggle = null;
            m_ragdolledParameter = null;
            m_puppetReferences = new BipedRagdollReferences();
            m_ragdollBodyHandlers.Clear();
            m_boneLinks.Clear();
            m_jointAnchors.Clear();
            m_reachedGround = true;
            m_groundedTime = 0f;
            m_downTime = float.MinValue;
            m_puppet.localScale = Vector3.one;
            m_inAir = false;
        }

        void OnAvatarSetup()
        {
            if(PlayerSetup.Instance._animator.isHuman)
            {
                m_avatarTransform = PlayerSetup.Instance._avatar.transform;
                m_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
                Utils.SetAvatarTPose();

                BipedRagdollReferences l_avatarReferences = BipedRagdollReferences.FromAvatar(PlayerSetup.Instance._animator);

                m_puppetRoot = new GameObject("Root").transform;
                m_puppetRoot.parent = m_puppet;
                m_puppetRoot.position = m_avatarTransform.position;
                m_puppetRoot.rotation = m_avatarTransform.rotation;

                m_puppetReferences.root = m_puppetRoot;
                m_puppetReferences.hips = CloneTransform(l_avatarReferences.hips, m_puppetReferences.root, "Hips");
                m_puppetReferences.spine = CloneTransform(l_avatarReferences.spine, m_puppetReferences.hips, "Spine");

                if(l_avatarReferences.chest != null)
                    m_puppetReferences.chest = CloneTransform(l_avatarReferences.chest, m_puppetReferences.spine, "Chest");

                m_puppetReferences.head = CloneTransform(l_avatarReferences.head, (m_puppetReferences.chest != null) ? m_puppetReferences.chest : m_puppetReferences.spine, "Head");

                m_puppetReferences.leftUpperArm = CloneTransform(l_avatarReferences.leftUpperArm, (m_puppetReferences.chest != null) ? m_puppetReferences.chest : m_puppetReferences.spine, "LeftUpperArm");
                m_puppetReferences.leftLowerArm = CloneTransform(l_avatarReferences.leftLowerArm, m_puppetReferences.leftUpperArm, "LeftLowerArm");
                m_puppetReferences.leftHand = CloneTransform(l_avatarReferences.leftHand, m_puppetReferences.leftLowerArm, "LeftHand");

                m_puppetReferences.rightUpperArm = CloneTransform(l_avatarReferences.rightUpperArm, (m_puppetReferences.chest != null) ? m_puppetReferences.chest : m_puppetReferences.spine, "RightUpperArm");
                m_puppetReferences.rightLowerArm = CloneTransform(l_avatarReferences.rightLowerArm, m_puppetReferences.rightUpperArm, "RightLowerArm");
                m_puppetReferences.rightHand = CloneTransform(l_avatarReferences.rightHand, m_puppetReferences.rightLowerArm, "RightHand");

                m_puppetReferences.leftUpperLeg = CloneTransform(l_avatarReferences.leftUpperLeg, m_puppetReferences.hips, "LeftUpperLeg");
                m_puppetReferences.leftLowerLeg = CloneTransform(l_avatarReferences.leftLowerLeg, m_puppetReferences.leftUpperLeg, "LeftLowerLeg");
                m_puppetReferences.leftFoot = CloneTransform(l_avatarReferences.leftFoot, m_puppetReferences.leftLowerLeg, "LeftFoot");

                m_puppetReferences.rightUpperLeg = CloneTransform(l_avatarReferences.rightUpperLeg, m_puppetReferences.hips, "RightUpperLeg");
                m_puppetReferences.rightLowerLeg = CloneTransform(l_avatarReferences.rightLowerLeg, m_puppetReferences.rightUpperLeg, "RightLowerLeg");
                m_puppetReferences.rightFoot = CloneTransform(l_avatarReferences.rightFoot, m_puppetReferences.rightLowerLeg, "RightFoot");

                // Move to world origin to overcome possible issues
                m_puppetRoot.position = Vector3.zero;
                m_puppetRoot.rotation = Quaternion.identity;

                BipedRagdollCreator.Options l_options = BipedRagdollCreator.AutodetectOptions(m_puppetReferences);
                l_options.joints = RagdollCreator.JointType.Character;
                BipedRagdollCreator.Create(m_puppetReferences, l_options);

                Transform[] l_puppetTransforms = m_puppetReferences.GetRagdollTransforms();
                Transform[] l_avatarTransforms = l_avatarReferences.GetRagdollTransforms();
                Transform[] l_influencedTransforms = new Transform[] { m_puppetReferences.hips, m_puppetReferences.spine, m_puppetReferences.chest };
                for(int i = 0; i < l_puppetTransforms.Length; i++)
                {
                    if(l_puppetTransforms[i] != null)
                    {
                        CharacterJoint l_joint = l_puppetTransforms[i].GetComponent<CharacterJoint>();
                        if(l_joint != null)
                        {
                            l_joint.enablePreprocessing = false;
                            l_joint.enableProjection = true;
                            m_jointAnchors.Add(System.Tuple.Create(l_joint, l_joint.connectedAnchor));
                        }

                        Rigidbody l_body = l_puppetTransforms[i].GetComponent<Rigidbody>();
                        Collider l_collider = l_puppetTransforms[i].GetComponent<Collider>();
                        if((l_body != null) && (l_collider != null))
                        {
                            RagdollBodypartHandler l_handler = l_puppetTransforms[i].gameObject.AddComponent<RagdollBodypartHandler>();
                            l_handler.SetInfuencerUsage(Utils.IsInEnumeration(l_puppetTransforms[i], l_influencedTransforms));
                            m_ragdollBodyHandlers.Add(l_handler);
                        }

                        if(l_avatarTransforms[i] != null)
                            m_boneLinks.Add(System.Tuple.Create(l_puppetTransforms[i], l_avatarTransforms[i]));
                    }
                }

                // And return back
                m_puppetRoot.position = m_avatarTransform.position;
                m_puppetRoot.rotation = m_avatarTransform.rotation;

                m_vrIK = PlayerSetup.Instance._avatar.GetComponent<VRIK>();
                if(m_vrIK != null)
                    m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostSolverUpdate);

                m_avatarRagdollToggle = PlayerSetup.Instance._avatar.GetComponentInChildren<RagdollToggle>(true);
                m_ragdolledParameter = new AvatarBoolParameter("Ragdolled", PlayerSetup.Instance.animatorManager);

                m_initTask = StartCoroutine(WaitForBodyHandlers());
            }
        }

        IEnumerator WaitForBodyHandlers()
        {
            while(!m_ragdollBodyHandlers.TrueForAll(p => p.IsReady()))
                yield return null;

            foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
            {
                l_handler.SetAsKinematic(true);
                l_handler.SetColliderMaterial(m_physicsMaterial);
            }

            m_avatarReady = true;
            m_initTask = null;

            OnMovementDragChanged(Settings.MovementDrag);
            OnAngularDragChanged(Settings.AngularDrag);
            OnGravityChanged(Settings.Gravity);
            OnBuoyancyChanged(Settings.Buoyancy);
        }

        void OnAvatarPreReuse()
        {
            m_forcedSwitch = true;
            Unragdoll();
            m_forcedSwitch = false;
        }
        void OnAvatarPostReuse()
        {
            m_vrIK = PlayerSetup.Instance._avatar.GetComponent<VRIK>();

            if(m_vrIK != null)
                m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostSolverUpdate);
        }

        void OnAvatarScaling(float p_scaleDifference)
        {
            if(m_puppetRoot != null)
                m_puppetRoot.localScale = Vector3.one * p_scaleDifference;

            foreach(var l_pair in m_jointAnchors)
                l_pair.Item1.connectedAnchor = l_pair.Item2 * p_scaleDifference;
        }

        void OnSeatPreSit(CVRSeat p_seat)
        {
            m_lastSeatPositon = p_seat.transform.position;

            if(!p_seat.occupied)
            {
                m_forcedSwitch = true;
                Unragdoll();
                m_forcedSwitch = false;
                m_inAir = false;
            }
        }

        void OnCalibrationStart()
        {
            m_forcedSwitch = true;
            Unragdoll();
            m_forcedSwitch = false;
            m_inAir = false;
        }

        void OnWorldPreSpawn()
        {
            Unragdoll();
            m_inAir = false;

            OnGravityChanged(Settings.Gravity);
            OnPhysicsMaterialChanged(true);
            OnMovementDragChanged(Settings.MovementDrag);
            OnBuoyancyChanged(Settings.Buoyancy);
        }

        void OnCombatPreDown()
        {
            if(CombatSystem.Instance.isDown && Settings.CombatReaction)
            {
                m_reachedGround = true;
                m_forcedSwitch = true;
                Ragdoll();
                m_forcedSwitch = false;
                m_inAir = false;
            }
        }

        void OnFlightChange()
        {
            if(BetterBetterCharacterController.Instance.IsFlying())
            {
                m_forcedSwitch = true;
                Unragdoll();
                m_forcedSwitch = false;
                m_inAir = false;
            }
        }

        void OnPlayerTeleport(BetterBetterCharacterController.PlayerMoveOffset p_offset)
        {
            try
            {
                m_inAir = false;

                if(m_avatarReady && m_ragdolled)
                {
                    m_puppetReferences.hips.position = m_hips.position;
                    m_lastRagdollPosition = m_puppetReferences.hips.position;
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnIKOffsetUpdate(GameEvents.EventResult p_result)
        {
            p_result.m_result |= (m_ragdolled && (m_vrIK != null));
        }

        // Custom game events
        void OnRemoteGestureStateChanged(ABI_RC.Core.Player.PuppetMaster p_master, RemoteGesturesManager.GestureHand p_hand, bool p_state)
        {
            if(m_avatarReady && m_ragdolled && Settings.GestureGrab && (p_master.animatorManager.Animator != null))
            {
                Transform l_hand = p_master.animatorManager.Animator.GetBoneTransform((p_hand == RemoteGesturesManager.GestureHand.Left) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                Transform l_finger = p_master.animatorManager.Animator.GetBoneTransform((p_hand == RemoteGesturesManager.GestureHand.Left) ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);

                if(l_hand != null)
                {
                    Vector3 l_pos = (l_finger != null) ? ((l_hand.position + l_finger.position) * 0.5f) : l_hand.position;
                    foreach(var l_bodyHandler in m_ragdollBodyHandlers)
                    {
                        if(p_state)
                        {
                            if(l_bodyHandler.Attach(l_hand, l_pos))
                                break;
                        }
                        else
                            l_bodyHandler.Detach(l_hand);
                    }
                }
            }
        }

        // VRIK updates
        void OnIKPostSolverUpdate()
        {
            if(!m_ragdolled)
            {
                m_puppetRoot.position = m_avatarTransform.position;
                m_puppetRoot.rotation = m_avatarTransform.rotation;

                foreach(var l_link in m_boneLinks)
                    l_link.Item2.CopyGlobal(l_link.Item1);
            }
        }

        // Settings
        void OnMovementDragChanged(float p_value)
        {
            if(m_avatarReady)
            {
                float l_drag = (WorldManager.IsSafeWorld() ? p_value : 1f);
                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                    l_handler.SetDrag(l_drag);
            }
        }

        void OnAngularDragChanged(float p_value)
        {
            if(m_avatarReady)
            {
                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                    l_handler.SetAngularDrag(p_value);
            }
        }

        void OnGravityChanged(bool p_state)
        {
            if(m_avatarReady)
            {
                bool l_gravity = (!WorldManager.IsSafeWorld() || p_state);
                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                    l_handler.SetActiveGravity(l_gravity);

                if(!l_gravity)
                {
                    OnMovementDragChanged(Settings.MovementDrag);
                    OnAngularDragChanged(Settings.AngularDrag);
                }
            }
        }

        void OnPhysicsMaterialChanged(bool p_state)
        {
            if(m_physicsMaterial != null)
            {
                bool l_slipperiness = (Settings.Slipperiness && WorldManager.IsSafeWorld());
                bool l_bounciness = (Settings.Bounciness && WorldManager.IsSafeWorld());
                m_physicsMaterial.dynamicFriction = (l_slipperiness ? 0f : c_defaultFriction);
                m_physicsMaterial.staticFriction = (l_slipperiness ? 0f : c_defaultFriction);
                m_physicsMaterial.frictionCombine = (l_slipperiness ? PhysicMaterialCombine.Minimum : PhysicMaterialCombine.Average);
                m_physicsMaterial.bounciness = (l_bounciness ? 1f : 0f);
                m_physicsMaterial.bounceCombine = (l_bounciness ? PhysicMaterialCombine.Maximum : PhysicMaterialCombine.Average);
            }
        }

        void OnBuoyancyChanged(bool p_state)
        {
            if(m_avatarReady)
            {
                bool l_buoyancy = (!WorldManager.IsSafeWorld() || p_state);
                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                    l_handler.SetBuoyancy(l_buoyancy);

                if(!l_buoyancy)
                {
                    OnMovementDragChanged(Settings.MovementDrag);
                    OnAngularDragChanged(Settings.AngularDrag);
                }
            }
        }

        void OnFallDamageChanged(bool p_state)
        {
            m_inAir = false;
        }

        void OnGestureGrabChanged(bool p_state)
        {
            if(m_avatarReady && m_ragdolled && !p_state)
            {
                foreach(var l_hanlder in m_ragdollBodyHandlers)
                    l_hanlder.Detach();
            }
        }

        // Arbitrary
        public bool IsRagdolled() => (m_avatarReady && m_ragdolled);

        public void SwitchRagdoll()
        {
            if(m_ragdolled)
                Unragdoll();
            else
                Ragdoll();
        }

        public void Ragdoll()
        {
            if(m_avatarReady && !m_ragdolled && CanRagdoll())
            {
                Vector3 l_velocity = (BetterBetterCharacterController.Instance.IsSitting() ? m_seatVelocity : BetterBetterCharacterController.Instance.velocity);
                l_velocity *= (WorldManager.IsSafeWorld() ? Settings.VelocityMultiplier : 1f);
                l_velocity = Vector3.ClampMagnitude(l_velocity, WorldManager.GetMovementLimit());
                if(Settings.ViewVelocity && WorldManager.IsSafeWorld())
                {
                    float l_mag = l_velocity.magnitude;
                    l_velocity = PlayerSetup.Instance.GetActiveCamera().transform.forward * l_mag;
                }

                Vector3 l_playerPos = PlayerSetup.Instance.transform.position;
                Quaternion l_playerRot = PlayerSetup.Instance.transform.rotation;
                bool l_wasSitting = BetterBetterCharacterController.Instance.IsSitting();
                if(BetterBetterCharacterController.Instance.IsSitting())
                {
                    BetterBetterCharacterController.Instance.SetSitting(false);
                    l_wasSitting = true;
                }

                if(BetterBetterCharacterController.Instance.IsFlying())
                    BetterBetterCharacterController.Instance.ChangeFlight(false, true);
                BetterBetterCharacterController.Instance.SetImmobilized(true);
                BetterBetterCharacterController.Instance.ClearFluidVolumes();
                BetterBetterCharacterController.Instance.ResetAllForces();
                BetterBetterCharacterController.Instance.PauseGroundConstraint();
                BodySystem.TrackingPositionWeight = 0f;
                m_applyHipsPosition = IKSystem.Instance.applyOriginalHipPosition;
                IKSystem.Instance.applyOriginalHipPosition = true;
                m_applyHipsRotation = IKSystem.Instance.applyOriginalHipRotation;
                IKSystem.Instance.applyOriginalHipRotation = true;

                PlayerSetup.Instance.animatorManager.CancelEmote = true;
                m_ragdolledParameter.SetValue(true);

                if(!WorldManager.IsSafeWorld())
                {
                    m_reachedGround = false; // Force player to unragdoll and reach ground first
                    m_groundedTime = 0f;
                }

                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                    l_handler.SetAsKinematic(false);

                m_puppet.gameObject.SetActive(false); // Resets rigidbodies and joints inner physics states
                m_puppet.gameObject.SetActive(true);

                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                {
                    l_handler.SetVelocity(l_velocity);
                    l_handler.SetAngularVelocity(Vector3.zero);
                }

                if(l_wasSitting)
                {
                    PlayerSetup.Instance.transform.position = l_playerPos;
                    PlayerSetup.Instance.transform.rotation = l_playerRot;
                }
                m_lastRagdollPosition = m_puppetReferences.hips.position;
                m_downTime = 0f;

                m_ragdolled = true;
            }
        }

        public void Unragdoll()
        {
            if(m_avatarReady && m_ragdolled && CanUnragdoll())
            {
                BetterBetterCharacterController.Instance.TeleportPlayerTo(m_puppetReferences.hips.position, PlayerSetup.Instance.GetPlayerRotation().eulerAngles, false, false);
                TryRestoreMovement();
                BodySystem.TrackingPositionWeight = 1f;
                IKSystem.Instance.applyOriginalHipPosition = m_applyHipsPosition;
                IKSystem.Instance.applyOriginalHipRotation = m_applyHipsRotation;

                if(m_vrIK != null)
                    m_vrIK.solver.Reset();

                m_ragdolledParameter.SetValue(false);

                m_puppet.localPosition = Vector3.zero;
                m_puppet.localRotation = Quaternion.identity;

                foreach(RagdollBodypartHandler l_handler in m_ragdollBodyHandlers)
                {
                    l_handler.Detach();
                    l_handler.ClearFluidVolumes();
                    l_handler.SetAsKinematic(true);
                }

                m_downTime = float.MinValue;

                // Restore rigidbody properties that could be affected by buoyancy
                OnMovementDragChanged(Settings.MovementDrag);
                OnAngularDragChanged(Settings.AngularDrag);

                m_ragdolled = false;
            }
        }

        bool CanRagdoll()
        {
            if(WorldManager.IsRestrictedWorld())
                return false;

            bool l_result = m_reachedGround;
            l_result &= !BodySystem.isCalibrating;
            l_result &= ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown);
            return (l_result || m_forcedSwitch);
        }

        bool CanUnragdoll()
        {
            bool l_result = true;
            l_result &= ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown);
            return (l_result || m_forcedSwitch);
        }

        void MovePlayer()
        {
            // Pain
            Vector3 l_up = PlayerSetup.Instance.transform.rotation * Vector3.up;
            Vector3 l_diff = m_puppetReferences.hips.position - m_lastRagdollPosition;
            m_playerPlane.SetNormalAndPosition(l_up, PlayerSetup.Instance.transform.position);

            PlayerSetup.Instance.transform.position += l_diff;
            m_lastRagdollPosition = m_puppetReferences.hips.position;

            // Try to tether player position closer to hips rigid body position
            if(m_playerPlane.GetDistanceToPoint(m_lastRagdollPosition) < 0f)
                m_playerPlane.SetNormalAndPosition(l_up, m_lastRagdollPosition);
            if(m_playerPlane.GetDistanceToPoint(PlayerSetup.Instance.transform.position) < 0f)
                PlayerSetup.Instance.transform.position = m_playerPlane.ClosestPointOnPlane(PlayerSetup.Instance.transform.position);
        }

        static void TryRestoreMovement()
        {
            bool l_state = true;
            l_state &= ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown);
            l_state &= !BetterBetterCharacterController.Instance.IsSitting();

            if(l_state)
                BetterBetterCharacterController.Instance.SetImmobilized(false);
        }


        static Transform CloneTransform(Transform p_source, Transform p_parent, string p_name)
        {
            Transform l_target = new GameObject(p_name).transform;
            l_target.parent = p_parent;
            p_source.CopyGlobal(l_target);
            return l_target;
        }
    }
}
