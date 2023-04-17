using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.MovementSystem;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    public class RagdollController : MonoBehaviour
    {
        public static RagdollController Instance { get; private set; } = null;

        VRIK m_vrIK = null;
        float m_vrIkWeight = 1f;
        bool m_inVr = false;

        bool m_enabled = false;

        readonly List<Rigidbody> m_rigidBodies = null;
        readonly List<Collider> m_colliders = null;
        Transform m_puppetRoot = null;
        Transform m_puppet = null;
        BipedRagdollReferences m_puppetReferences;
        readonly List<System.Tuple<Transform, Transform>> m_boneLinks = null;

        bool m_avatarReady = false;
        Vector3 m_lastPosition = Vector3.zero;
        Vector3 m_velocity = Vector3.zero;
        Vector3 m_ragdollLastPos = Vector3.zero;

        RagdollToggle m_avatarRagdollToggle = null;
        RagdollTrigger m_customTrigger = null;
        AvatarBoolParameter m_ragdolledParameter = null;
        readonly PhysicMaterial m_physicsMaterial = null;

        bool m_reachedGround = true;
        float m_downTime = float.MinValue;

        internal RagdollController()
        {
            if(Instance == null)
                Instance = this;

            m_rigidBodies = new List<Rigidbody>();
            m_colliders = new List<Collider>();
            m_boneLinks = new List<System.Tuple<Transform, Transform>>();

            m_physicsMaterial = new PhysicMaterial("Ragdoll");
            m_physicsMaterial.dynamicFriction = 0.5f;
            m_physicsMaterial.staticFriction = 0.5f;
            m_physicsMaterial.frictionCombine = PhysicMaterialCombine.Average;
            m_physicsMaterial.bounciness = 0f;
            m_physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
        }
        ~RagdollController()
        {
            if(Instance == this)
                Instance = null;
        }

        // Unity events
        void Start()
        {
            m_inVr = Utils.IsInVR();

            m_puppetRoot = new GameObject("[PlayerAvatarPuppet]").transform;
            m_puppetRoot.parent = PlayerSetup.Instance.transform;
            m_puppetRoot.localPosition = Vector3.zero;
            m_puppetRoot.localRotation = Quaternion.identity;

            m_customTrigger = MovementSystem.Instance.proxyCollider.gameObject.AddComponent<RagdollTrigger>();

            Settings.SwitchChange += this.SwitchRagdoll;
            Settings.MovementDragChange += this.OnMovementDragChange;
            Settings.AngularDragChange += this.OnAngularDragChange;
            Settings.GravityChange += this.OnGravityChange;
            Settings.SlipperinessChange += this.OnPhysicsMaterialChange;
            Settings.BouncinessChange += this.OnPhysicsMaterialChange;
        }

        void OnDestroy()
        {
            if(m_customTrigger != null)
            {
                Object.Destroy(m_customTrigger);
                m_customTrigger = null;
            }

            Settings.SwitchChange -= this.SwitchRagdoll;
            Settings.MovementDragChange -= this.OnMovementDragChange;
            Settings.AngularDragChange -= this.OnAngularDragChange;
            Settings.GravityChange -= this.OnGravityChange;
            Settings.SlipperinessChange -= this.OnPhysicsMaterialChange;
            Settings.BouncinessChange -= this.OnPhysicsMaterialChange;
        }

        void Update()
        {
            if(m_enabled && m_avatarReady)
            {
                Vector3 l_dif = m_puppetReferences.hips.position - m_ragdollLastPos;
                PlayerSetup.Instance.transform.position += l_dif;
                m_puppetReferences.hips.position -= l_dif;
                m_ragdollLastPos = m_puppetReferences.hips.position;
            }

            if(!m_enabled && m_avatarReady)
            {
                Vector3 l_pos = PlayerSetup.Instance.transform.position;
                m_velocity = (m_velocity + (l_pos - m_lastPosition) / Time.deltaTime) * 0.5f;
                m_lastPosition = l_pos;
            }

            if(m_avatarReady && !m_reachedGround && MovementSystem.Instance.IsGrounded())
                m_reachedGround = true;

            if(m_enabled && m_avatarReady && BodySystem.isCalibratedAsFullBody)
                BodySystem.TrackingPositionWeight = 0f;

            if(m_avatarReady && m_enabled && Settings.AutoRecover)
            {
                m_downTime += Time.deltaTime;
                if(m_downTime >= Settings.RecoverDelay)
                {
                    SwitchRagdoll();
                    m_downTime = float.MinValue; // One attepmt to recover
                }
            }

            if(Settings.Hotkey && Input.GetKeyDown(KeyCode.R) && !ViewManager.Instance.isGameMenuOpen())
                SwitchRagdoll();

            if((m_avatarRagdollToggle != null) && m_avatarRagdollToggle.isActiveAndEnabled && m_avatarRagdollToggle.shouldOverride && (m_enabled != m_avatarRagdollToggle.isOn))
                SwitchRagdoll();

            if((m_customTrigger != null) && m_customTrigger.GetStateWithReset() && !m_enabled && m_avatarReady && Settings.PointersReaction)
                SwitchRagdoll();
        }

        void LateUpdate()
        {
            if(m_enabled && m_avatarReady)
            {
                if(BodySystem.isCalibratedAsFullBody)
                    BodySystem.TrackingPositionWeight = 0f;

                foreach(var l_link in m_boneLinks)
                    l_link.Item1.CopyGlobal(l_link.Item2);
            }
        }

        // Game events
        internal void OnAvatarClear()
        {
            if(m_enabled)
                MovementSystem.Instance.SetImmobilized(false);

            if(m_puppet != null)
                Object.Destroy(m_puppet.gameObject);
            m_puppet = null;

            m_vrIK = null;
            m_enabled = false;
            m_avatarReady = false;
            m_avatarRagdollToggle = null;
            m_ragdolledParameter = null;
            m_rigidBodies.Clear();
            m_colliders.Clear();
            m_puppetReferences = new BipedRagdollReferences();
            m_boneLinks.Clear();
            m_reachedGround = true;
            m_downTime = float.MinValue;
        }

        internal void OnAvatarSetup()
        {
            m_inVr = Utils.IsInVR();

            if(PlayerSetup.Instance._animator.isHuman)
            {
                BipedRagdollReferences l_avatarReferences = BipedRagdollReferences.FromAvatar(PlayerSetup.Instance._animator);

                m_puppet = new GameObject("Root").transform;
                m_puppet.parent = m_puppetRoot;
                m_puppet.localPosition = Vector3.zero;
                m_puppet.localRotation = Quaternion.identity;

                m_puppetReferences.root = m_puppet;
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

                // Move to world origin to overcome possible issues, maybe?
                m_puppetRoot.position = Vector3.zero;
                m_puppetRoot.rotation = Quaternion.identity;

                BipedRagdollCreator.Options l_options = BipedRagdollCreator.AutodetectOptions(m_puppetReferences);
                l_options.joints = RagdollCreator.JointType.Character;
                BipedRagdollCreator.Create(m_puppetReferences, l_options);

                // And return back
                m_puppetRoot.localPosition = Vector3.zero;
                m_puppetRoot.localRotation = Quaternion.identity;

                Transform[] l_puppetTransforms = m_puppetReferences.GetRagdollTransforms();
                Transform[] l_avatarTransforms = l_avatarReferences.GetRagdollTransforms();
                for(int i = 0; i < l_puppetTransforms.Length; i++)
                {
                    if(l_puppetTransforms[i] != null)
                    {
                        Rigidbody l_body = l_puppetTransforms[i].GetComponent<Rigidbody>();
                        if(l_body != null)
                        {
                            m_rigidBodies.Add(l_body);
                            l_body.isKinematic = true;
                            l_body.angularDrag = Settings.AngularDrag;
                            l_body.drag = Settings.MovementDrag;
                            l_body.useGravity = (!Utils.IsWorldSafe() || Settings.Gravity);
                            l_body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        }

                        CharacterJoint l_joint = l_puppetTransforms[i].GetComponent<CharacterJoint>();
                        if(l_joint != null)
                        {
                            l_joint.enablePreprocessing = false;
                            l_joint.enableProjection = true;
                        }

                        Collider l_collider = l_puppetTransforms[i].GetComponent<Collider>();
                        if(l_collider != null)
                        {
                            Physics.IgnoreCollision(l_collider, MovementSystem.Instance.proxyCollider, true);
                            Physics.IgnoreCollision(l_collider, MovementSystem.Instance.controller, true);
                            Physics.IgnoreCollision(l_collider, MovementSystem.Instance.forceCollider, true);
                            l_collider.enabled = false;
                            l_collider.sharedMaterial = m_physicsMaterial;
                            l_collider.material = m_physicsMaterial;
                            m_colliders.Add(l_collider);
                        }

                        if(l_avatarTransforms[i] != null)
                            m_boneLinks.Add(System.Tuple.Create(l_puppetTransforms[i], l_avatarTransforms[i]));
                    }
                }

                m_vrIK = PlayerSetup.Instance._avatar.GetComponent<VRIK>();
                if(m_vrIK != null)
                {
                    m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                    m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
                }

                m_avatarRagdollToggle = PlayerSetup.Instance._avatar.GetComponentInChildren<RagdollToggle>(true);
                m_ragdolledParameter = new AvatarBoolParameter("Ragdolled", PlayerSetup.Instance.animatorManager);

                m_avatarReady = true;
            }
        }

        internal void OnSeatSitDown(CVRSeat p_seat)
        {
            if(m_enabled && m_avatarReady && !p_seat.occupied)
                SwitchRagdoll();
        }

        internal void OnStartCalibration()
        {
            if(m_enabled && m_avatarReady)
                SwitchRagdoll();
        }

        internal void OnWorldSpawn()
        {
            if(m_enabled && m_avatarReady)
                SwitchRagdoll();

            OnGravityChange(Settings.Gravity);
            OnPhysicsMaterialChange(true);
        }

        internal void OnCombatDown()
        {
            if(!m_enabled && m_avatarReady && Settings.CombatReaction)
                SwitchRagdoll();
        }

        // IK updates
        void OnIKPreUpdate()
        {
            if(m_enabled)
            {
                m_vrIkWeight = m_vrIK.solver.IKPositionWeight;
                m_vrIK.solver.IKPositionWeight = 0f;
            }
        }
        void OnIKPostUpdate()
        {
            if(m_enabled)
                m_vrIK.solver.IKPositionWeight = m_vrIkWeight;
        }

        // Settings
        void OnMovementDragChange(float p_value)
        {
            if(m_avatarReady)
            {
                foreach(Rigidbody l_body in m_rigidBodies)
                {
                    l_body.drag = p_value;
                    if(m_enabled)
                        l_body.WakeUp();
                }
            }
        }
        void OnAngularDragChange(float p_value)
        {
            if(m_avatarReady)
            {
                foreach(Rigidbody l_body in m_rigidBodies)
                {
                    l_body.angularDrag = p_value;
                    if(m_enabled)
                        l_body.WakeUp();
                }
            }
        }
        void OnGravityChange(bool p_state)
        {
            if(m_avatarReady)
            {
                foreach(Rigidbody l_body in m_rigidBodies)
                    l_body.useGravity = (!Utils.IsWorldSafe() || p_state);
            }
        }
        void OnPhysicsMaterialChange(bool p_state)
        {
            if(m_physicsMaterial != null)
            {
                bool l_slipperiness = (Settings.Slipperiness && Utils.IsWorldSafe());
                bool l_bounciness = (Settings.Bounciness && Utils.IsWorldSafe());
                m_physicsMaterial.dynamicFriction = (l_slipperiness ? 0f : 0.5f);
                m_physicsMaterial.staticFriction = (l_slipperiness ? 0f : 0.5f);
                m_physicsMaterial.frictionCombine = (l_slipperiness ? PhysicMaterialCombine.Minimum : PhysicMaterialCombine.Average);
                m_physicsMaterial.bounciness = (l_bounciness ? 1f : 0f);
                m_physicsMaterial.bounceCombine = (l_bounciness ? PhysicMaterialCombine.Maximum : PhysicMaterialCombine.Average);
            }
        }

        // Arbitrary
        public void SwitchRagdoll()
        {
            if(m_avatarReady)
            {
                if(!m_enabled)
                {
                    if(IsSafeToRagdoll() && m_reachedGround)
                    {
                        // Eject player from seat
                        if(MovementSystem.Instance.lastSeat != null)
                        {
                            Vector3 l_pos = PlayerSetup.Instance.transform.position;
                            Quaternion l_rot = PlayerSetup.Instance.transform.rotation;

                            if(MetaPort.Instance.isUsingVr)
                            {
                                MetaPort.Instance.isUsingVr = false;
                                MovementSystem.Instance.lastSeat.ExitSeat();
                                MetaPort.Instance.isUsingVr = true;
                            }
                            else
                                MovementSystem.Instance.lastSeat.ExitSeat();

                            PlayerSetup.Instance.transform.position = l_pos;
                            PlayerSetup.Instance.transform.rotation = Quaternion.Euler(0f, l_rot.eulerAngles.y, 0f);
                        }

                        MovementSystem.Instance.SetImmobilized(true);
                        PlayerSetup.Instance.animatorManager.SetAnimatorParameterTrigger("CancelEmote");
                        m_ragdolledParameter.SetValue(true);
                        if(BodySystem.isCalibratedAsFullBody)
                            BodySystem.TrackingPositionWeight = 0f;

                        if(!Utils.IsWorldSafe())
                            m_reachedGround = false; // Force player to unragdoll and reach ground first

                        // Copy before set to non-kinematic to reduce stacked forces
                        foreach(var l_link in m_boneLinks)
                            l_link.Item2.CopyGlobal(l_link.Item1);

                        foreach(Rigidbody l_body in m_rigidBodies)
                            l_body.isKinematic = false;

                        Vector3 l_velocity = Vector3.ClampMagnitude(m_velocity * (Utils.IsWorldSafe() ? Settings.VelocityMultiplier : 1f), Utils.GetWorldMovementLimit());
                        if(Settings.ViewVelocity && Utils.IsWorldSafe())
                        {
                            float l_mag = l_velocity.magnitude;
                            l_velocity = PlayerSetup.Instance.GetActiveCamera().transform.forward * l_mag;
                        }

                        foreach(Rigidbody l_body in m_rigidBodies)
                        {
                            l_body.velocity = l_velocity;
                            l_body.angularVelocity = Vector3.zero;
                        }

                        foreach(Collider l_collider in m_colliders)
                            l_collider.enabled = true;

                        m_ragdollLastPos = m_puppetReferences.hips.position;
                        m_downTime = 0f;

                        m_enabled = true;
                    }
                }
                else
                {
                    if(IsSafeToUnragdoll())
                    {
                        MovementSystem.Instance.SetImmobilized(false);
                        m_ragdolledParameter.SetValue(false);
                        if(BodySystem.isCalibratedAsFullBody)
                            BodySystem.TrackingPositionWeight = 1f;

                        foreach(Rigidbody l_body in m_rigidBodies)
                            l_body.isKinematic = true;

                        PlayerSetup.Instance.transform.position = m_puppetReferences.hips.position;
                        PlayerSetup.Instance.transform.position -= (PlayerSetup.Instance.transform.rotation * PlayerSetup.Instance._avatar.transform.localPosition);

                        foreach(Collider l_collider in m_colliders)
                            l_collider.enabled = false;

                        m_lastPosition = PlayerSetup.Instance.transform.position;
                        m_velocity = Vector3.zero;
                        m_downTime = float.MinValue;

                        m_enabled = false;
                    }
                }
            }
        }

        public bool IsRagdolled() => (m_enabled && m_avatarReady);

        static Transform CloneTransform(Transform p_source, Transform p_parent, string p_name)
        {
            Transform l_target = new GameObject(p_name).transform;
            l_target.parent = p_parent;
            p_source.CopyGlobal(l_target);
            return l_target;
        }

        static bool IsSafeToRagdoll()
        {
            bool l_result = true;
            l_result &= !BodySystem.isCalibrating; // Not calibrating
            l_result &= ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown); // Non-combat world or not dead
            return l_result;
        }

        static bool IsSafeToUnragdoll()
        {
            bool l_result = true;
            l_result &= ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown); // Non-combat world or not dead
            return l_result;
        }
    }
}
