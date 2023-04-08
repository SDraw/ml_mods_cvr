using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.MovementSystem;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    class RagdollController : MonoBehaviour
    {
        VRIK m_vrIK = null;
        float m_vrIkWeight = 1f;

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

        RagdollToggle m_avatarRagdollToggle = null;

        internal RagdollController()
        {
            m_rigidBodies = new List<Rigidbody>();
            m_colliders = new List<Collider>();
            m_boneLinks = new List<System.Tuple<Transform, Transform>>();
        }

        // Unity events
        void Start()
        {
            m_puppetRoot = new GameObject("[PlayerAvatarPuppet]").transform;
            m_puppetRoot.parent = PlayerSetup.Instance.transform;
            m_puppetRoot.localPosition = Vector3.zero;
            m_puppetRoot.localRotation = Quaternion.identity;

            Settings.SwitchChange += this.SwitchRagdoll;
            Settings.MovementDragChange += this.OnMovementDragChange;
            Settings.AngularDragChange += this.OnAngularDragChange;
            Settings.GravityChange += this.OnGravityChange;
        }

        void OnDestroy()
        {
            Settings.SwitchChange -= this.SwitchRagdoll;
            Settings.MovementDragChange -= this.OnMovementDragChange;
            Settings.AngularDragChange -= this.OnAngularDragChange;
            Settings.GravityChange -= this.OnGravityChange;
        }

        void Update()
        {
            Vector3 l_pos = PlayerSetup.Instance.transform.position;
            m_velocity = (m_velocity + (l_pos - m_lastPosition) / Time.deltaTime) * 0.5f;
            m_lastPosition = l_pos;

            if(Settings.Hotkey && Input.GetKeyDown(KeyCode.R) && !ViewManager.Instance.isGameMenuOpen())
                SwitchRagdoll();

            if((m_avatarRagdollToggle != null) && m_avatarRagdollToggle.isActiveAndEnabled && m_avatarRagdollToggle.shouldOverride && (m_enabled != m_avatarRagdollToggle.isOn))
                SwitchRagdoll();

            if(m_enabled && m_avatarReady && BodySystem.isCalibratedAsFullBody)
                BodySystem.TrackingPositionWeight = 0f;
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
            m_rigidBodies.Clear();
            m_colliders.Clear();
            m_puppetReferences = new BipedRagdollReferences();
            m_boneLinks.Clear();
        }

        internal void OnAvatarSetup()
        {
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

                BipedRagdollCreator.Options l_options = BipedRagdollCreator.AutodetectOptions(m_puppetReferences);
                l_options.joints = RagdollCreator.JointType.Character;
                BipedRagdollCreator.Create(m_puppetReferences, l_options);

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
                            Physics.IgnoreCollision(MovementSystem.Instance.proxyCollider, l_collider, true);
                            l_collider.enabled = false;
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

            if(m_avatarReady)
            {
                foreach(Rigidbody l_body in m_rigidBodies)
                {
                    l_body.useGravity = (!Utils.IsWorldSafe() || Settings.Gravity);
                }
            }
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

        // Arbitrary
        public void SwitchRagdoll()
        {
            if(m_avatarReady && (MovementSystem.Instance.lastSeat == null) && !BodySystem.isCalibrating)
            {
                m_enabled = !m_enabled;

                MovementSystem.Instance.SetImmobilized(m_enabled);
                PlayerSetup.Instance.animatorManager.SetAnimatorParameterTrigger("CancelEmote");

                if(m_enabled)
                {
                    if(BodySystem.isCalibratedAsFullBody)
                        BodySystem.TrackingPositionWeight = 0f;

                    foreach(var l_link in m_boneLinks)
                        l_link.Item2.CopyGlobal(l_link.Item1);

                    foreach(Rigidbody l_body in m_rigidBodies)
                        l_body.isKinematic = false;

                    Vector3 l_velocity = m_velocity * Mathf.Clamp(Settings.VelocityMultiplier, 1f, (Utils.IsWorldSafe() ? Utils.GetWorldFlyMultiplier() : 1f));
                    foreach(Rigidbody l_body in m_rigidBodies)
                    {
                        l_body.velocity = l_velocity;
                        l_body.angularVelocity = Vector3.zero;
                    }
                }
                else
                {
                    if(BodySystem.isCalibratedAsFullBody)
                        BodySystem.TrackingPositionWeight = 1f;

                    foreach(Rigidbody l_body in m_rigidBodies)
                        l_body.isKinematic = true;

                    if((m_puppetReferences.hips != null))
                    {
                        Vector3 l_hipsPos = m_puppetReferences.hips.position;

                        if(!Settings.RestorePosition)
                        {
                            if(Utils.IsInVR())
                            {
                                Vector3 l_diff = l_hipsPos - PlayerSetup.Instance._avatar.transform.position;
                                Vector3 l_playerPos = PlayerSetup.Instance.transform.position;
                                PlayerSetup.Instance.transform.position = l_playerPos + l_diff;
                            }
                            else
                                PlayerSetup.Instance.transform.position = l_hipsPos;
                        }
                    }
                }
            }

            foreach(Collider l_collider in m_colliders)
                l_collider.enabled = m_enabled;
        }

        public bool IsRagdolled() => (m_enabled && m_avatarReady);

        static Transform CloneTransform(Transform p_source, Transform p_parent, string p_name)
        {
            Transform l_target = new GameObject(p_name).transform;
            l_target.parent = p_parent;
            p_source.CopyGlobal(l_target);
            return l_target;
        }
    }
}
