using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Systems.Movement;
using NAK.Contacts;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class RagdollBodypartHandler : MonoBehaviour
    {
        const string c_ragdollPointerType = "ragdoll";
        const string c_grabPointerType = "grab";

        bool m_ready = false;

        Rigidbody m_rigidBody = null;
        Collider m_collider = null;
        PhysicsInfluencer m_physicsInfluencer = null;
        ContactReceiver m_contactReciever = null;

        bool m_attached = false;
        Transform m_attachTransform = null;
        ContactSender m_attachedSender = null;
        FixedJoint m_attachJoint = null;
        static List<ContactSender> ms_attachedSenders = new List<ContactSender>();

        public bool UseBuoyancy { get; set; } = false;

        // Unity events
        void Awake()
        {
            m_collider = this.GetComponent<Collider>();
            m_rigidBody = this.GetComponent<Rigidbody>();

            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = false;
                m_rigidBody.detectCollisions = true;
                m_rigidBody.useGravity = false;
                m_rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if(m_collider != null)
            {
                RemoveGameCollision();

                var l_constactShape = ContactConversion.FromCollider(m_collider, true);
                m_contactReciever = this.gameObject.AddComponent<ContactReceiver>();

                m_contactReciever.shapeType = l_constactShape.shapeType;
                m_contactReciever.localPosition = l_constactShape.localPosition;
                m_contactReciever.localRotation = l_constactShape.localRotation;
                m_contactReciever.radius = l_constactShape.radius;
                m_contactReciever.height = l_constactShape.height;
                m_contactReciever.boxSize = l_constactShape.boxSize;

                m_contactReciever.collisionTags = new string[] { c_ragdollPointerType, c_grabPointerType };
                m_contactReciever.receiverType = ReceiverType.Constant;
                m_contactReciever.contentTypes = ContentType.World | ContentType.Avatar | ContentType.Prop;
                m_contactReciever.SourceContentType = ContentType.Player;
                m_contactReciever.contactValue = 1f;
                m_contactReciever.drawGizmos = false;

                m_contactReciever.OnContactEnter += this.OnContactEnter;
            }
        }

        void Start()
        {
            if((m_rigidBody != null) && (m_collider != null))
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
        }

        void OnDestroy()
        {
            Detach();
        }

        void Update()
        {
            if(m_attached && ((m_attachedSender == null) || !m_attachedSender.isActiveAndEnabled))
                Detach();
        }

        void OnCollisionEnter(Collision p_col)
        {
            if(Settings.ImpactSounds && m_ready && !m_rigidBody.isKinematic && (p_col.gameObject.layer != CVRLayers.PlayerClone))
            {
                if(p_col.impulse.magnitude > 5f)
                    SoundManager.Instance.PlayLocalSound(SoundManager.ImpactType.Soft);
            }
        }

        void OnContactEnter(ContactCollisionInfo p_col)
        {
            if(m_ready && (RagdollController.Instance != null) && ContactManager.Exists)
            {
                ContactSender l_sender = ContactManager.Instance.GetSenderById(p_col.senderContactId);
                if((l_sender != null) && (l_sender.collisionTags != null))
                {
                    foreach(string l_tag in l_sender.collisionTags)
                    {
                        switch(l_tag)
                        {
                            case c_ragdollPointerType:
                            {
                                if(Settings.PointersReaction && !RagdollController.Instance.IsRagdolled() && RestrictionsCheck(l_sender.transform.root))
                                    RagdollController.Instance.Ragdoll();
                            }
                            break;

                            case c_grabPointerType:
                            {
                                if(!m_attached && RagdollController.Instance.IsRagdolled() && RestrictionsCheck(l_sender.transform.root))
                                    Attach(l_sender);
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Arbitrary
        public bool IsReady() => ((m_rigidBody != null) && (m_collider != null) && (m_physicsInfluencer != null) && m_physicsInfluencer.IsReady());

        public void SetColliderMaterial(PhysicMaterial p_material)
        {
            if(m_collider != null)
            {
                m_collider.sharedMaterial = p_material;
                m_collider.material = p_material;
            }
        }

        public void SetAsKinematic(bool p_state)
        {
            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = p_state;
                m_rigidBody.collisionDetectionMode = (p_state ? CollisionDetectionMode.Discrete : CollisionDetectionMode.ContinuousDynamic);
            }
            if(m_collider != null)
                m_collider.isTrigger = p_state;
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
            if(m_collider != null)
                RemoveGameCollision();
        }

        void Attach(ContactSender p_sender)
        {
            if(!m_attached && (m_collider != null) && (m_rigidBody != null) && !ms_attachedSenders.Contains(p_sender))
            {
                m_attachedSender = p_sender;

                GameObject l_attachPoint = new GameObject("[AttachPoint]");
                l_attachPoint.layer = CVRLayers.Default;
                m_attachTransform = l_attachPoint.transform;
                m_attachTransform.parent = p_sender.transform;

                Rigidbody l_body = l_attachPoint.AddComponent<Rigidbody>();
                l_body.isKinematic = true;
                l_body.detectCollisions = false;

                m_attachJoint = this.gameObject.AddComponent<FixedJoint>();
                m_attachJoint.connectedBody = l_body;
                m_attachJoint.breakForce = Mathf.Infinity;
                m_attachJoint.breakTorque = Mathf.Infinity;

                ms_attachedSenders.Add(p_sender);
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

                if(!ReferenceEquals(m_attachedSender, null))
                    ms_attachedSenders.Remove(m_attachedSender);
                m_attachedSender = null;

                m_attached = false;
            }
        }

        void RemoveGameCollision()
        {
            Physics.IgnoreCollision(m_collider, BetterBetterCharacterController.Instance.Collider, true);
            Physics.IgnoreCollision(m_collider, BetterBetterCharacterController.Instance.KinematicTriggerProxy.Collider, true);
            Physics.IgnoreCollision(m_collider, BetterBetterCharacterController.Instance.NonKinematicProxy.Collider, true);
            Physics.IgnoreCollision(m_collider, BetterBetterCharacterController.Instance.SphereProxy.Collider, true);
            BetterBetterCharacterController.Instance.IgnoreCollision(m_collider);
        }

        internal void RestoreContact()
        {
            if((m_contactReciever != null) && ContactManager.Exists)
                ContactManager.Instance.RestoreContact(m_contactReciever, m_contactReciever.isActiveAndEnabled);
        }

        static bool RestrictionsCheck(Transform p_transform) => (p_transform != PlayerSetup.Instance.transform);
    }
}
