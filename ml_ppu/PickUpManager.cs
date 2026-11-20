using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.Movement;
using NAK.Contacts;
using UnityEngine;

namespace ml_ppu
{
    class PickUpManager : MonoBehaviour
    {
        public static PickUpManager Instance { get; private set; } = null;

        PuppetMaster m_holder = null;
        ContactSender m_holderPointA = null;
        Quaternion m_holderPointAOffset;
        ContactSender m_holderPointB = null;
        Quaternion m_holderPointBOffset;

        CapsuleCollider m_collider = null;
        Matrix4x4 m_colliderOffSet;
        Matrix4x4 m_playerOffSet;

        Transform m_hips = null;
        Transform m_armLeft = null;
        Transform m_armRight = null;
        bool m_ready = false;
        bool m_held = false;

        Vector3 m_lastPosition = Vector3.zero;
        Vector3 m_velocity = Vector3.zero;

        AvatarParameter m_avatarParameter = null;

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
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnAvatarSetup);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(this.OnAvatarClear);
            GameEvents.OnIKScaling.AddListener(this.OnIKScaling);
            GameEvents.OnWorldPreSpawn.AddListener(this.OnWorldPreSpawn);
            GameEvents.OnSeatPreSit.AddListener(this.OnSeatPreSit);

            Settings.OnEnabledChanged.AddListener(this.OnEnabledChanged);
        }

        void OnDestroy()
        {
            if(Instance == this)
                Instance = null;

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnAvatarSetup);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.RemoveListener(this.OnAvatarClear);
            GameEvents.OnIKScaling.RemoveListener(this.OnIKScaling);
            GameEvents.OnWorldPreSpawn.RemoveListener(this.OnWorldPreSpawn);
            GameEvents.OnSeatPreSit.RemoveListener(this.OnSeatPreSit);

            Settings.OnEnabledChanged.RemoveListener(this.OnEnabledChanged);
        }

        void Update()
        {
            if(m_ready)
            {
                if(!m_held)
                {
                    Vector3 l_armsMidPoint = (m_armLeft.position + m_armRight.position) * 0.5f;
                    Quaternion l_avatarRot = PlayerSetup.Instance.AvatarTransform.rotation;

                    m_collider.transform.position = Vector3.zero;
                    m_collider.transform.rotation = Quaternion.identity;
                    m_collider.transform.up = Quaternion.Inverse(l_avatarRot) * (l_armsMidPoint - m_hips.position).normalized;

                    m_collider.transform.position = m_hips.position;
                    m_collider.transform.rotation = l_avatarRot * m_collider.transform.rotation;

                    if(Settings.Enabled)
                        DetectGrab();
                }
                else
                {
                    // Check if our points are still valid
                    if(IsHolderValid() && !ModSupport.IsRagdolled())
                    {
                        Matrix4x4 l_midPoint = Matrix4x4.TRS(
                            Vector3.Lerp(m_holderPointA.transform.position, m_holderPointB.transform.position, 0.5f),
                            Quaternion.Slerp(m_holderPointA.transform.rotation * m_holderPointAOffset, m_holderPointB.transform.rotation * m_holderPointBOffset, 0.5f),
                            Vector3.one
                        );
                        Matrix4x4 l_colliderMat = l_midPoint * m_colliderOffSet;
                        m_collider.transform.position = l_colliderMat.GetPosition();
                        m_collider.transform.rotation = l_colliderMat.rotation;

                        Matrix4x4 l_heldMat = l_colliderMat * m_playerOffSet;
                        BetterBetterCharacterController.Instance.TeleportPlayerTo(l_heldMat.GetPosition(), l_heldMat.rotation, true, false); // Extension method with Quaternion as rotation

                        Vector3 l_position = l_heldMat.GetPosition();
                        m_velocity = (l_position - m_lastPosition) / Time.deltaTime;
                        m_lastPosition = l_position;
                    }
                    else
                    {
                        m_holder = null;
                        m_holderPointA = null;
                        m_holderPointB = null;
                        m_held = false;

                        BetterBetterCharacterController.Instance.SetVelocity(m_velocity * Settings.VelocityMultiplier);
                        m_avatarParameter?.SetValue(false);
                    }
                }
            }
        }

        void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                Animator l_animator = PlayerSetup.Instance.Animator;
                if((l_animator != null) && l_animator.isHuman)
                {
                    m_avatarParameter = new AvatarParameter("PickedUp", PlayerSetup.Instance.AnimatorManager);

                    IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
                    PlayerSetup.Instance.AvatarTransform.localPosition = Vector3.zero;
                    PlayerSetup.Instance.AvatarTransform.localRotation = Quaternion.identity;

                    m_hips = l_animator.GetBoneTransform(HumanBodyBones.Hips);
                    m_armLeft = l_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    m_armRight = l_animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

                    if((m_hips != null) && (m_armLeft != null) && (m_armRight != null))
                    {
                        Matrix4x4 l_avatarMatInv = PlayerSetup.Instance.AvatarTransform.GetMatrix().inverse;
                        Vector3 l_hipsPos = (l_avatarMatInv * m_hips.GetMatrix()).GetPosition();
                        Vector3 l_armPos = (l_avatarMatInv * m_armLeft.GetMatrix()).GetPosition();

                        m_collider = new GameObject("[Collider]").AddComponent<CapsuleCollider>();
                        m_collider.gameObject.layer = CVRLayers.PassPlayer;
                        m_collider.transform.parent = this.transform;
                        m_collider.isTrigger = true;
                        m_collider.height = Vector3.Distance(l_hipsPos, new Vector3(0f, l_armPos.y, l_armPos.z));
                        m_collider.radius = new Vector2(l_armPos.x, l_armPos.z).magnitude;
                        m_collider.center = new Vector3(0f, m_collider.height * 0.5f, 0f);

                        m_ready = true;
                    }
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                m_avatarParameter = null;

                m_ready = false;
                m_held = false;

                if(m_collider != null)
                {
                    Destroy(m_collider.gameObject);
                    m_collider = null;
                }
                m_holder = null;
                m_holderPointA = null;
                m_holderPointB = null;
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnIKScaling(float p_scale)
        {
            if(m_ready)
                m_collider.transform.localScale = Vector3.one * p_scale;
        }

        void OnWorldPreSpawn()
        {
            if(m_ready && m_held)
            {
                m_held = false;
                m_holder = null;
                m_holderPointA = null;
                m_holderPointB = null;
            }
        }

        void OnSeatPreSit(CVRSeat p_seat)
        {
            if(!p_seat.occupied && m_ready && m_held)
            {
                m_held = false;
                m_holder = null;
                m_holderPointA = null;
                m_holderPointB = null;
            }
        }

        void OnEnabledChanged(bool p_state)
        {
            if(!p_state && m_ready && m_held)
            {
                m_held = false;
                m_holder = null;
                m_holderPointA = null;
                m_holderPointB = null;
            }
        }

        void DetectGrab()
        {
            if(m_held || !CVRWorld.Instance.allowFlying || ModSupport.IsRagdolled() || (CVRPlayerManager.Instance == null))
                return;

            foreach(CVRPlayerEntity l_player in CVRPlayerManager.Instance.NetworkPlayers)
            {
                PuppetMaster l_puppet = l_player?.PuppetMaster;
                if((l_puppet == null) || !l_puppet.IsLeftHandGrabbing || !l_puppet.IsRightHandGrabbing)
                    continue;

                if(Settings.FriendsOnly && !Friends.FriendsWith(l_player.Uuid))
                    continue;

                CVRAvatar l_avatar = l_puppet.AvatarDescriptor;
                if((l_avatar == null) || !IsSenderInside(l_avatar.leftHandSender) || !IsSenderInside(l_avatar.rightHandSender))
                    continue;

                OnGrabDetected(l_puppet, l_avatar.leftHandSender, l_avatar.rightHandSender);
                break;
            }
        }

        bool IsSenderInside(ContactSender p_sender)
        {
            if((p_sender == null) || !p_sender.isActiveAndEnabled)
                return false;

            Transform l_transform = p_sender.transform;
            Vector3 l_point = l_transform.TransformPoint(p_sender.localPosition);
            float l_radius = (p_sender.shapeType == ShapeType.Capsule) ? (p_sender.radius + p_sender.height * 0.5f) : p_sender.radius;
            l_radius *= Mathf.Abs(l_transform.lossyScale.x);
            return ((m_collider.ClosestPoint(l_point) - l_point).sqrMagnitude <= (l_radius * l_radius));
        }

        bool IsHolderValid()
        {
            return ((m_holder != null) && m_holder.IsLeftHandGrabbing && m_holder.IsRightHandGrabbing && (m_holderPointA != null) && m_holderPointA.isActiveAndEnabled && (m_holderPointB != null) && m_holderPointB.isActiveAndEnabled);
        }

        void OnGrabDetected(PuppetMaster p_holder, ContactSender p_pointA, ContactSender p_pointB)
        {
            if(m_ready && !m_held)
            {
                m_holder = p_holder;
                m_holderPointA = p_pointA;
                m_holderPointB = p_pointB;

                // Remember offsets
                Vector3 l_playerPos = PlayerSetup.Instance.GetPlayerPosition();
                Quaternion l_playerRot = PlayerSetup.Instance.GetPlayerRotation();
                m_holderPointAOffset = Quaternion.Inverse(m_holderPointA.transform.rotation) * l_playerRot;
                m_holderPointBOffset = Quaternion.Inverse(m_holderPointB.transform.rotation) * l_playerRot;

                Matrix4x4 l_midPoint = Matrix4x4.TRS(
                    Vector3.Lerp(m_holderPointA.transform.position, m_holderPointB.transform.position, 0.5f),
                    l_playerRot,
                    Vector3.one
                );
                m_colliderOffSet = l_midPoint.inverse * m_collider.transform.GetMatrix();
                m_playerOffSet = m_collider.transform.GetMatrix().inverse * Matrix4x4.TRS(l_playerPos, l_playerRot, Vector3.one);
                m_lastPosition = l_playerPos;
                m_velocity = Vector3.zero;
                m_held = true;

                m_avatarParameter?.SetValue(true);
            }
        }
    }
}
