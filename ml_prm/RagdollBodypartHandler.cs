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

        bool m_attached = false;
        Transform m_attachedHand = null;
        Transform m_attachTransform = null;
        FixedJoint m_attachJoint = null;

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

        void FixedUpdate()
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.useGravity = false;

                if(!m_attached && m_activeGravity && ((m_physicsInfluencer == null) || !m_physicsInfluencer.enableInfluence || !m_physicsInfluencer.GetSubmerged()))
                    m_rigidBody.AddForce(BetterBetterCharacterController.Instance.GravityResult.AppliedGravity * m_rigidBody.mass);
            }
        }

        void Update()
        {
            if(m_attached && !ReferenceEquals(m_attachTransform, null) && (m_attachTransform == null))
            {
                m_attachTransform = null;

                if(m_attachJoint != null)
                    Object.Destroy(m_attachJoint);
                m_attachJoint = null;

                m_attachedHand = null;
                m_attached = false;
            }
        }

        void OnTriggerEnter(Collider p_col)
        {
            if(Settings.PointersReaction && (RagdollController.Instance != null) && !RagdollController.Instance.IsRagdolled())
            {
                CVRPointer l_pointer = p_col.GetComponent<CVRPointer>();
                if((l_pointer != null) && (l_pointer.type == c_ragdollPointerType) && l_pointer.enabled && !IsIgnored(l_pointer.transform))
                    RagdollController.Instance.Ragdoll();
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

        public bool Attach(Transform p_hand, Vector3 p_pos)
        {
            bool l_result = false;

            if(!m_attached && (collider != null))
            {
                if(Vector3.Distance(p_pos, collider.ClosestPoint(p_pos)) <= Settings.GrabDistance)
                {
                    GameObject l_attachPoint = new GameObject("[AttachPoint]");
                    m_attachTransform = l_attachPoint.transform;
                    m_attachTransform.parent = p_hand;
                    m_attachTransform.position = p_pos;

                    Rigidbody l_body = l_attachPoint.AddComponent<Rigidbody>();
                    l_body.isKinematic = true;
                    l_body.detectCollisions = false;

                    m_attachJoint = this.gameObject.AddComponent<FixedJoint>();
                    m_attachJoint.connectedBody = l_body;
                    m_attachJoint.breakForce = Mathf.Infinity;
                    m_attachJoint.breakTorque = Mathf.Infinity;

                    m_attached = true;
                    m_attachedHand = p_hand;
                    l_result = true;
                }
            }
            return l_result;
        }

        public void Detach() => Detach(m_attachedHand);
        public void Detach(Transform p_hand)
        {
            if(m_attached && ReferenceEquals(m_attachedHand, p_hand))
            {
                if(m_attachTransform != null)
                    Object.Destroy(m_attachTransform.gameObject);
                m_attachTransform = null;

                if(m_attachJoint != null)
                    Object.Destroy(m_attachJoint);
                m_attachJoint = null;

                m_attachedHand = null;
                m_attached = false;
            }
        }

        // CVRTriggerVolume
        public void TriggerEnter(CVRPointer pointer)
        {
            if(Settings.PointersReaction && (RagdollController.Instance != null))
            {
                if((pointer != null) && (pointer.type == c_ragdollPointerType) && pointer.enabled && !IsIgnored(pointer.transform) && !RagdollController.Instance.IsRagdolled())
                    RagdollController.Instance.SwitchRagdoll();
            }
        }
        public void TriggerExit(CVRPointer pointer)
        {
        }
    }
}
