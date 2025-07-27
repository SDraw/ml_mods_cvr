using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.Movement;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class RagdollBodypartHandler : MonoBehaviour, CVRTriggerVolume
    {
        const string c_ragdollPointerType = "ragdoll";
        const string c_grabPointerType = "grab";

        bool m_ready = false;

        Rigidbody m_rigidBody = null;
        public Collider collider { get; set; } = null;

        PhysicsInfluencer m_physicsInfluencer = null;
        public bool UseBuoyancy { get; set; } = false;

        bool m_attached = false;
        CVRPointer m_attachedPointer = null;
        Transform m_attachTransform = null;
        FixedJoint m_attachJoint = null;

        // Unity events
        void Awake()
        {
            collider = this.GetComponent<Collider>();
            m_rigidBody = this.GetComponent<Rigidbody>();

            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = false;
                m_rigidBody.detectCollisions = true;
                m_rigidBody.useGravity = false;
                m_rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if(collider != null)
                RemoveGameCollision();
        }

        void Start()
        {
            if((m_rigidBody != null) && (collider != null))
            {
                m_physicsInfluencer = this.gameObject.AddComponent<PhysicsInfluencer>();
                m_physicsInfluencer.fluidDrag = 3f;
                m_physicsInfluencer.fluidAngularDrag = 1f;
                m_physicsInfluencer.enableInfluence = true;
                m_physicsInfluencer.enableLocalGravity = true;
                m_physicsInfluencer.enableBuoyancy = true;
                m_physicsInfluencer.forceAlignUpright = false;

                float l_mass = m_rigidBody.mass;
                m_physicsInfluencer.UpdateDensity();
                m_rigidBody.mass = l_mass;
                m_physicsInfluencer.volume = l_mass * 0.005f;

                this.gameObject.name = string.Format("{0} [NoGizmo]", this.gameObject.name);
            }

            if(collider != null)
            {
                CVRParticlePointerManager.volumes.Add(this);
                CVRParticlePointerManager.UpdateParticleSystems();
            }
        }

        void OnDestroy()
        {
            if(collider != null)
                CVRParticlePointerManager.RemoveTrigger(collider);

            Detach();
        }

        void Update()
        {
            if(m_attached && ((m_attachedPointer == null) || !m_attachedPointer.isActiveAndEnabled))
                Detach();
        }

        void OnTriggerEnter(Collider p_col)
        {
            if(m_ready && (RagdollController.Instance != null))
            {
                CVRPointer l_pointer = p_col.GetComponent<CVRPointer>();

                // Ragdolling
                if(Settings.PointersReaction && !RagdollController.Instance.IsRagdolled())
                {
                    if((l_pointer != null) && (l_pointer.type == c_ragdollPointerType) && l_pointer.enabled && !IgnoreCheck(l_pointer.transform))
                        RagdollController.Instance.Ragdoll();
                }

                //Attachment
                if(!m_attached && RagdollController.Instance.IsRagdolled())
                {
                    if((l_pointer != null) && (l_pointer.type == c_grabPointerType) && RestrictionsCheck(p_col.transform.root))
                        Attach(l_pointer);
                }
            }
        }

        // Arbitrary
        public bool IsReady() => ((m_rigidBody != null) && (collider != null) && (m_physicsInfluencer != null) && m_physicsInfluencer.IsReady());

        public void SetColliderMaterial(PhysicMaterial p_material)
        {
            if(collider != null)
            {
                collider.sharedMaterial = p_material;
                collider.material = p_material;
            }
        }

        public void SetAsKinematic(bool p_state)
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = p_state;
                m_rigidBody.collisionDetectionMode = (p_state ? CollisionDetectionMode.Discrete : CollisionDetectionMode.ContinuousDynamic);
            }
            if(collider != null)
                collider.isTrigger = p_state;
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.enabled = !p_state;
        }

        public void SetVelocity(Vector3 p_vec)
        {
            if(m_rigidBody != null)
                m_rigidBody.velocity = p_vec;
        }

        public void SetAngularVelocity(Vector3 p_vec)
        {
            if(m_rigidBody != null)
                m_rigidBody.angularVelocity = p_vec;
        }

        public void SetActiveGravity(bool p_state)
        {
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.gravityFactor = (p_state ? 1f : 0f);
        }

        public void SetDrag(float p_value)
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.drag = p_value;
                m_rigidBody.WakeUp();
            }
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.airDrag = p_value;
        }

        public void SetAngularDrag(float p_value)
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.angularDrag = p_value;
                m_rigidBody.WakeUp();
            }
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.airAngularDrag = p_value;
        }

        public void SetBuoyancy(bool p_state)
        {
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.enableBuoyancy = (UseBuoyancy && p_state);
        }

        public void ClearFluidVolumes()
        {
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.ClearFluidVolumes();
        }

        internal void RemovePhysicsController()
        {
            if(this.gameObject.TryGetComponent<CVRSharedPhysicsController>(out var l_controller))
            {
                Object.Destroy(l_controller); // Yeet!
                m_ready = true;
            }
            if(collider != null)
                RemoveGameCollision();
        }

        void Attach(CVRPointer p_pointer)
        {
            if(!m_attached && (collider != null) && (m_rigidBody != null))
            {
                m_attachedPointer = p_pointer;

                GameObject l_attachPoint = new GameObject("[AttachPoint]");
                l_attachPoint.layer = CVRLayers.PlayerNetwork;
                m_attachTransform = l_attachPoint.transform;
                m_attachTransform.parent = p_pointer.transform;

                Rigidbody l_body = l_attachPoint.AddComponent<Rigidbody>();
                l_body.isKinematic = true;
                l_body.detectCollisions = false;

                m_attachJoint = this.gameObject.AddComponent<FixedJoint>();
                m_attachJoint.connectedBody = l_body;
                m_attachJoint.breakForce = Mathf.Infinity;
                m_attachJoint.breakTorque = Mathf.Infinity;

                m_attached = true;
            }
        }

        public void Detach()
        {
            if(m_attached)
            {
                if(m_attachTransform != null)
                    Object.Destroy(m_attachTransform.gameObject);
                m_attachTransform = null;

                if(m_attachJoint != null)
                    Object.Destroy(m_attachJoint);
                m_attachJoint = null;

                m_attachedPointer = null;
                m_attached = false;
            }
        }

        void RemoveGameCollision()
        {
            Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.Collider, true);
            Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.KinematicTriggerProxy.Collider, true);
            Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.NonKinematicProxy.Collider, true);
            Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.SphereProxy.Collider, true);
            BetterBetterCharacterController.Instance.IgnoreCollision(collider);
        }

        // CVRTriggerVolume
        public void TriggerEnter(CVRPointer pointer)
        {
            if(Settings.PointersReaction && (pointer != null) && pointer.enabled && (pointer.type == c_ragdollPointerType) && !IgnoreCheck(pointer.transform) && (RagdollController.Instance != null) && !RagdollController.Instance.IsRagdolled())
                RagdollController.Instance.Ragdoll();
        }
        public void TriggerExit(CVRPointer pointer)
        {
        }

        // Static utility
        static bool IgnoreCheck(Transform p_transform)
        {
            return (Settings.IgnoreLocal && (p_transform.root == PlayerSetup.Instance.transform));
        }

        static bool RestrictionsCheck(Transform p_transform)
        {
            if(p_transform == PlayerSetup.Instance.transform)
                return false;

            PlayerDescriptor l_playerDescriptor = p_transform.GetComponent<PlayerDescriptor>();
            if(l_playerDescriptor != null)
                return (!Settings.FriendsGrab || Friends.FriendsWith(l_playerDescriptor.ownerId));

            return false;
        }
    }
}
