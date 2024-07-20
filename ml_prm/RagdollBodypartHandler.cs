using ABI.CCK.Components;
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

        Rigidbody m_rigidBody = null;
        public Collider collider { get; set; } = null;
        PhysicsInfluencer m_physicsInfluencer = null;

        bool m_shouldHaveInfluencer = false;
        bool m_activeGravity = true;

        // Unity events
        void Awake()
        {
            this.gameObject.layer = LayerMask.NameToLayer("PlayerLocal");

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
            {
                Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.Collider, true);
                Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.KinematicTriggerProxy.Collider, true);
                Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.NonKinematicProxy.Collider, true);
                Physics.IgnoreCollision(collider, BetterBetterCharacterController.Instance.SphereProxy.Collider, true);
                BetterBetterCharacterController.Instance.IgnoreCollision(collider, true);
            }
        }

        void Start()
        {
            if(m_shouldHaveInfluencer && (m_rigidBody != null) && (collider != null))
            {
                m_physicsInfluencer = this.gameObject.AddComponent<PhysicsInfluencer>();
                m_physicsInfluencer.fluidDrag = 3f;
                m_physicsInfluencer.fluidAngularDrag = 1f;
                m_physicsInfluencer.enableBuoyancy = true;
                m_physicsInfluencer.enableInfluence = false;
                m_physicsInfluencer.forceAlignUpright = false;
                float mass = m_rigidBody.mass;
                m_physicsInfluencer.UpdateDensity();
                m_rigidBody.mass = mass;
                m_physicsInfluencer.volume = mass * 0.005f;
                m_physicsInfluencer.enableLocalGravity = true;
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
        }

        void FixedUpdate()
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.useGravity = false;

                if(m_activeGravity && ((m_physicsInfluencer == null) || !m_physicsInfluencer.enableInfluence || !m_physicsInfluencer.GetSubmerged()))
                    m_rigidBody.AddForce(BetterBetterCharacterController.Instance.GravityResult.AppliedGravity * m_rigidBody.mass);
            }
        }

        void OnTriggerEnter(Collider p_col)
        {
            if(Settings.PointersReaction && (RagdollController.Instance != null))
            {
                CVRPointer l_pointer = p_col.GetComponent<CVRPointer>();
                if((l_pointer != null) && (l_pointer.type == c_ragdollPointerType) && !IsIgnored(l_pointer.transform) && !RagdollController.Instance.IsRagdolled())
                    RagdollController.Instance.SwitchRagdoll();
            }
        }

        // Arbitrary
        public bool IsReady() => ((m_rigidBody != null) && (collider != null) && (!m_shouldHaveInfluencer || ((m_physicsInfluencer != null) && m_physicsInfluencer.IsReady())));
        public void SetInfuencerUsage(bool p_state) => m_shouldHaveInfluencer = p_state;

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
            if(collider != null)
                collider.isTrigger = p_state;
            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = p_state;
                m_rigidBody.collisionDetectionMode = (p_state ? CollisionDetectionMode.Discrete : CollisionDetectionMode.ContinuousDynamic);
            }
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
            m_activeGravity = p_state;

            if(m_physicsInfluencer != null)
                m_physicsInfluencer.enabled = m_activeGravity;
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
                m_physicsInfluencer.enableInfluence = p_state;
        }

        public void ClearFluidVolumes()
        {
            if(m_physicsInfluencer != null)
                m_physicsInfluencer.ClearFluidVolumes();
        }

        static bool IsIgnored(Transform p_transform)
        {
            return (Settings.IgnoreLocal && (p_transform.root == PlayerSetup.Instance.transform));
        }

        // CVRTriggerVolume
        public void TriggerEnter(CVRPointer pointer)
        {
            if(Settings.PointersReaction && (RagdollController.Instance != null))
            {
                if((pointer != null) && (pointer.type == c_ragdollPointerType) && !IsIgnored(pointer.transform) && !RagdollController.Instance.IsRagdolled())
                    RagdollController.Instance.SwitchRagdoll();
            }
        }
        public void TriggerExit(CVRPointer pointer) { }
    }
}
