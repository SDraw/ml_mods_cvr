using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Systems.VRModeSwitch;
using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

namespace ml_pam
{
    [DisallowMultipleComponent]
    class ArmMover : MonoBehaviour
    {
        enum HandState
        {
            Empty = 0,
            Pickup,
            Extended
        }
        struct IKInfo
        {
            public Vector4 m_armsWeights;
            public Transform m_leftHandTarget;
            public Transform m_rightHandTarget;
        }

        static ArmMover ms_instance = null;

        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(270f, 90f, 0f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(270f, 270f, 0f);

        VRIK m_vrIK = null;
        Vector4 m_armsLength; // x,y - from upper arm to hand; z,w - from center to upper arm
        Transform m_camera = null;
        IKInfo m_ikInfo;

        Transform m_root = null;
        Transform m_leftTarget = null;
        Transform m_rightTarget = null;
        Transform m_leftRotationTarget = null;
        Transform m_rightRotationTarget = null;
        ArmIK m_armIKLeft = null;
        ArmIK m_armIKRight = null;
        CVRPickupObject m_pickup = null;
        Matrix4x4 m_offset;

        HandState m_leftHandState = HandState.Empty;
        HandState m_rightHandState = HandState.Empty;
        Vector2 m_handsWeights;

        AvatarBoolParameter m_leftHandParameter = null;
        AvatarBoolParameter m_rightHandParameter = null;

        Coroutine m_disableTask = null;

        // Unity events
        void Awake()
        {
            if(ms_instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            ms_instance = this;
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            m_camera = PlayerSetup.Instance.activeCam.transform;

            m_root = new GameObject("Root").transform;
            m_root.parent = this.transform;
            m_root.localPosition = Vector3.zero;
            m_root.localRotation = Quaternion.identity;

            m_leftTarget = new GameObject("TargetLeft").transform;
            m_leftTarget.parent = m_root;
            m_leftTarget.localPosition = Vector3.zero;
            m_leftTarget.localRotation = Quaternion.identity;

            m_leftRotationTarget = new GameObject("RotationTarget").transform;
            m_leftRotationTarget.parent = m_leftTarget;
            m_leftRotationTarget.localPosition = Vector3.zero;
            m_leftRotationTarget.localRotation = Quaternion.identity;

            m_rightTarget = new GameObject("TargetRight").transform;
            m_rightTarget.parent = m_root;
            m_rightTarget.localPosition = Vector3.zero;
            m_rightTarget.localRotation = Quaternion.identity;

            m_rightRotationTarget = new GameObject("RotationTarget").transform;
            m_rightRotationTarget.parent = m_rightTarget;
            m_rightRotationTarget.localPosition = Vector3.zero;
            m_rightRotationTarget.localRotation = Quaternion.identity;

            Settings.OnEnabledChanged.AddListener(this.OnEnabledChanged);
            Settings.OnGrabOffsetChanged.AddListener(this.OnGrabOffsetChanged);
            Settings.OnLeadingHandChanged.AddListener(this.OnLeadingHandChanged);
            Settings.OnHandsExtensionChanged.AddListener(this.OnHandsExtensionChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.AddListener(this.OnAvatarReuse);
            GameEvents.OnIKScaling.AddListener(this.OnIKScaling);
            GameEvents.OnPickupGrab.AddListener(this.OnPickupGrab);
            GameEvents.OnPickupDrop.AddListener(this.OnPickupDrop);

            VRModeSwitchEvents.OnCompletedVRModeSwitch.AddListener(this.OnVRModeSwitch);
        }

        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            if(m_disableTask != null)
                StopCoroutine(m_disableTask);
            m_disableTask = null;

            RemoveArmIK();

            if(m_leftRotationTarget != null)
                Destroy(m_leftRotationTarget.gameObject);
            m_leftRotationTarget = null;

            if(m_leftTarget != null)
                Destroy(m_leftTarget.gameObject);
            m_leftTarget = null;

            if(m_rightRotationTarget != null)
                Destroy(m_rightRotationTarget.gameObject);
            m_rightRotationTarget = null;

            if(m_rightTarget != null)
                Destroy(m_rightTarget.gameObject);
            m_rightTarget = null;

            if(m_root != null)
                Destroy(m_root.gameObject);
            m_root = null;

            m_pickup = null;
            m_vrIK = null;

            Settings.OnEnabledChanged.RemoveListener(this.OnEnabledChanged);
            Settings.OnGrabOffsetChanged.RemoveListener(this.OnGrabOffsetChanged);
            Settings.OnLeadingHandChanged.RemoveListener(this.OnLeadingHandChanged);
            Settings.OnHandsExtensionChanged.RemoveListener(this.OnHandsExtensionChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarClear.RemoveListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.RemoveListener(this.OnAvatarReuse);
            GameEvents.OnIKScaling.RemoveListener(this.OnIKScaling);
            GameEvents.OnPickupGrab.RemoveListener(this.OnPickupGrab);
            GameEvents.OnPickupDrop.RemoveListener(this.OnPickupDrop);

            VRModeSwitchEvents.OnCompletedVRModeSwitch.RemoveListener(this.OnVRModeSwitch);
        }

        void Update()
        {
            if((m_root != null) && (m_camera != null))
            {
                m_root.position = m_camera.position;
                m_root.rotation = m_camera.rotation;
            }

            if(!ReferenceEquals(m_pickup, null) && (m_pickup == null))
                OnPickupDrop(m_pickup);

            switch(m_leftHandState)
            {
                case HandState.Empty:
                {
                    if(Settings.Enabled && Settings.HandsExtension && Input.GetKey(Settings.LeftHandKey) && !ViewManager.Instance.IsAnyMenuOpen)
                        m_leftHandState = HandState.Extended;
                }
                break;
                case HandState.Extended:
                {
                    if(!Input.GetKey(Settings.LeftHandKey))
                        m_leftHandState = HandState.Empty;
                }
                break;
            }
            switch(m_rightHandState)
            {
                case HandState.Empty:
                {
                    if(Settings.Enabled && Settings.HandsExtension && Input.GetKey(Settings.RightHandKey) && !ViewManager.Instance.IsAnyMenuOpen)
                        m_rightHandState = HandState.Extended;
                }
                break;
                case HandState.Extended:
                {
                    if(!Input.GetKey(Settings.RightHandKey))
                        m_rightHandState = HandState.Empty;
                }
                break;
            }

            m_handsWeights.x = Mathf.Clamp01(m_handsWeights.x + ((m_leftHandState != HandState.Empty) ? 1f : -1f) * Time.unscaledDeltaTime * Settings.ExtensionSpeed);
            m_handsWeights.y = Mathf.Clamp01(m_handsWeights.y + ((m_rightHandState != HandState.Empty) ? 1f : -1f) * Time.unscaledDeltaTime * Settings.ExtensionSpeed);

            UpdateArmIK(m_armIKLeft, m_handsWeights.x);
            UpdateArmIK(m_armIKRight, m_handsWeights.y);

            m_leftHandParameter?.SetValue(m_leftHandState != HandState.Empty);
            m_rightHandParameter?.SetValue(m_rightHandState != HandState.Empty);

            if(m_leftHandState != HandState.Empty)
            {
                if(m_pickup != null)
                {
                    Matrix4x4 l_result = m_pickup.transform.GetMatrix() * m_offset;
                    m_leftTarget.position = l_result.GetPosition();
                    m_leftTarget.rotation = l_result.rotation;
                    m_leftTarget.localPosition = Vector3.ClampMagnitude(m_leftTarget.localPosition, m_armsLength.x);
                }
                else
                {
                    m_leftTarget.localPosition = new Vector3(0f, 0f, m_armsLength.x);
                    m_leftTarget.localRotation = Quaternion.identity;
                }
            }
            if(m_rightHandState != HandState.Empty)
            {
                if(m_pickup != null)
                {
                    Matrix4x4 l_result = m_pickup.transform.GetMatrix() * m_offset;
                    m_rightTarget.position = l_result.GetPosition();
                    m_rightTarget.rotation = l_result.rotation;
                    m_rightTarget.localPosition = Vector3.ClampMagnitude(m_rightTarget.localPosition, m_armsLength.y);
                }
                else
                {
                    m_rightTarget.localPosition = new Vector3(0f, 0f, m_armsLength.y);
                    m_rightTarget.localRotation = Quaternion.identity;
                }
            }
        }

        void LateUpdate()
        {
            if((m_root != null) && (m_camera != null))
            {
                m_root.position = m_camera.position;
                m_root.rotation = m_camera.rotation;
            }
        }

        // VRIK updates
        void OnIKPreUpdate()
        {
            if(!Mathf.Approximately(m_handsWeights.x, 0f))
            {
                m_ikInfo.m_leftHandTarget = m_vrIK.solver.leftArm.target;
                m_ikInfo.m_armsWeights.x = m_vrIK.solver.leftArm.positionWeight;
                m_ikInfo.m_armsWeights.y = m_vrIK.solver.leftArm.rotationWeight;

                m_vrIK.solver.leftArm.positionWeight = m_handsWeights.x;
                m_vrIK.solver.leftArm.rotationWeight = m_handsWeights.x;
                m_vrIK.solver.leftArm.target = m_leftRotationTarget;
            }
            if(!Mathf.Approximately(m_handsWeights.y, 0f))
            {
                m_ikInfo.m_rightHandTarget = m_vrIK.solver.rightArm.target;
                m_ikInfo.m_armsWeights.z = m_vrIK.solver.rightArm.positionWeight;
                m_ikInfo.m_armsWeights.w = m_vrIK.solver.rightArm.rotationWeight;

                m_vrIK.solver.rightArm.positionWeight = m_handsWeights.y;
                m_vrIK.solver.rightArm.rotationWeight = m_handsWeights.y;
                m_vrIK.solver.rightArm.target = m_rightRotationTarget;
            }
        }
        void OnIKPostUpdate()
        {
            if(!Mathf.Approximately(m_handsWeights.x, 0f))
            {
                m_vrIK.solver.leftArm.target = m_ikInfo.m_leftHandTarget;
                m_vrIK.solver.leftArm.positionWeight = m_ikInfo.m_armsWeights.x;
                m_vrIK.solver.leftArm.rotationWeight = m_ikInfo.m_armsWeights.y;
            }
            if(!Mathf.Approximately(m_handsWeights.y, 0f))
            {
                m_vrIK.solver.rightArm.target = m_ikInfo.m_rightHandTarget;
                m_vrIK.solver.rightArm.positionWeight = m_ikInfo.m_armsWeights.z;
                m_vrIK.solver.rightArm.rotationWeight = m_ikInfo.m_armsWeights.w;
            }
        }

        // Settings
        void OnEnabledChanged(bool p_state)
        {
            if(p_state)
            {
                if(this.enabled)
                {
                    if(m_disableTask != null)
                    {
                        StopCoroutine(m_disableTask);
                        m_disableTask = null;
                    }
                }
                else
                    this.enabled = true;

                OnLeadingHandChanged(Settings.LeadingHand);
            }
            else
            {
                m_leftHandState = HandState.Empty;
                m_rightHandState = HandState.Empty;

                m_disableTask = StartCoroutine(WaitToDisable());
            }
        }

        void OnGrabOffsetChanged(float p_value)
        {
            if(m_leftRotationTarget != null)
                m_leftRotationTarget.localPosition = new Vector3(-m_armsLength.z * p_value * 2f, 0f, 0f);
            if(m_rightRotationTarget != null)
                m_rightRotationTarget.localPosition = new Vector3(m_armsLength.w * p_value * 2f, 0f, 0f);
        }

        void OnLeadingHandChanged(Settings.LeadHand p_hand)
        {
            SetLeadingHandState((m_pickup != null) ? HandState.Pickup : HandState.Empty);
            SetUnleadingHandState(HandState.Empty);
        }

        void OnHandsExtensionChanged(bool p_state)
        {
            if(m_leftHandState == HandState.Extended)
                m_leftHandState = HandState.Empty;
            if(m_rightHandState == HandState.Extended)
                m_rightHandState = HandState.Empty;
        }

        // Game events
        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                m_vrIK = null;
                m_armIKLeft = null;
                m_armIKRight = null;
                m_armsLength.Set(0f, 0f, 0f, 0f);
                m_leftHandParameter = null;
                m_rightHandParameter = null;
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                m_camera = PlayerSetup.Instance.activeCam.transform;

                if(PlayerSetup.Instance.Animator.isHuman)
                {
                    m_vrIK = PlayerSetup.Instance.Animator.GetComponent<VRIK>();
                    Utils.SetAvatarTPose();

                    Animator l_animator = PlayerSetup.Instance.Animator;
                    Matrix4x4 l_avatarMatrixInv = l_animator.transform.GetMatrix().inverse; // Animator and avatar are on same game object

                    Transform l_leftHand = l_animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    if(l_leftHand != null)
                        m_leftRotationTarget.localRotation = ms_offsetLeft * (l_avatarMatrixInv * l_leftHand.GetMatrix()).rotation;
                    Transform l_rightHand = l_animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if(l_rightHand != null)
                        m_rightRotationTarget.localRotation = ms_offsetRight * (l_avatarMatrixInv * l_rightHand.GetMatrix()).rotation;

                    m_armsLength.x = GetChainLength(new Transform[]{
                    l_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                    l_animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                    l_animator.GetBoneTransform(HumanBodyBones.LeftHand)
                });
                    m_armsLength.y = GetChainLength(new Transform[]{
                    l_animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                    l_animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                    l_animator.GetBoneTransform(HumanBodyBones.RightHand)
                });
                    m_armsLength.z = Mathf.Abs((l_avatarMatrixInv * l_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetMatrix()).GetPosition().x);
                    m_armsLength.w = Mathf.Abs((l_avatarMatrixInv * l_animator.GetBoneTransform(HumanBodyBones.RightUpperArm).GetMatrix()).GetPosition().x);

                    if(!Utils.IsInVR())
                    {
                        if(m_vrIK != null)
                        {
                            m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                            m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
                        }
                        else
                            SetupArmIK();
                    }
                }

                m_leftHandParameter = new AvatarBoolParameter("LeftHandExtended", PlayerSetup.Instance.AnimatorManager);
                m_rightHandParameter = new AvatarBoolParameter("RightHandExtended", PlayerSetup.Instance.AnimatorManager);

                OnEnabledChanged(Settings.Enabled);
                OnGrabOffsetChanged(Settings.GrabOffset);
                OnIKScaling(1f); // Reset scaling, game doesn't do this anymore on avatar switch
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarReuse()
        {
            // Old VRIK is destroyed by game
            m_vrIK = PlayerSetup.Instance.Animator.GetComponent<VRIK>();

            if(Utils.IsInVR())
                RemoveArmIK();
            else
            {
                if(m_vrIK != null)
                {
                    m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                    m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
                }
                else
                    SetupArmIK();
            }

            OnEnabledChanged(Settings.Enabled);
        }

        void OnPickupGrab(CVRPickupObject p_pickup, Vector3 p_hit)
        {
            if(p_pickup.IsGrabbedByMe && (p_pickup.ControllerRay == PlayerSetup.Instance.desktopRay))
            {
                m_pickup = p_pickup;

                // Set offsets
                if(m_pickup.gripType == CVRPickupObject.GripType.Origin)
                {
                    if(m_pickup.ikReference != null)
                        m_offset = (m_pickup.transform.GetMatrix().inverse * m_pickup.ikReference.GetMatrix());
                    else
                    {
                        if(m_pickup.gripOrigin != null)
                            m_offset = m_pickup.transform.GetMatrix().inverse * m_pickup.gripOrigin.GetMatrix();
                    }
                }
                else
                    m_offset = m_pickup.transform.GetMatrix().inverse * Matrix4x4.TRS(p_hit, m_camera.rotation, Vector3.one);

                if(Settings.Enabled)
                    OnLeadingHandChanged(Settings.LeadingHand);
            }
        }

        void OnPickupDrop(CVRPickupObject p_pickup)
        {
            if(ReferenceEquals(m_pickup, p_pickup) || (m_pickup == p_pickup))
            {
                m_pickup = null;

                if(Settings.Enabled)
                    SetLeadingHandState(HandState.Empty);
            }
        }

        void OnIKScaling(float p_relation)
        {
            if(m_root != null)
                m_root.localScale = Vector3.one * p_relation;
        }

        void OnVRModeSwitch(bool p_state)
        {
            try
            {
                m_camera = PlayerSetup.Instance.activeCam.transform;
                this.enabled = !Utils.IsInVR();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        // Arbitrary
        void SetupArmIK()
        {
            Animator l_animator = PlayerSetup.Instance.Animator;

            Transform l_chest = l_animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if(l_chest == null)
                l_chest = l_animator.GetBoneTransform(HumanBodyBones.Chest);
            if(l_chest == null)
                l_chest = l_animator.GetBoneTransform(HumanBodyBones.Spine);

            m_armIKLeft = PlayerSetup.Instance.AvatarObject.AddComponent<ArmIK>();
            m_armIKLeft.solver.isLeft = true;
            m_armIKLeft.solver.SetChain(
                l_chest,
                l_animator.GetBoneTransform(HumanBodyBones.LeftShoulder),
                l_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                l_animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                l_animator.GetBoneTransform(HumanBodyBones.LeftHand),
                l_animator.transform
            );
            m_armIKLeft.solver.arm.target = m_leftRotationTarget;
            m_armIKLeft.solver.arm.positionWeight = 1f;
            m_armIKLeft.solver.arm.rotationWeight = 1f;
            m_armIKLeft.solver.IKPositionWeight = 0f;
            m_armIKLeft.solver.IKRotationWeight = 0f;
            m_armIKLeft.enabled = false;

            m_armIKRight = PlayerSetup.Instance.AvatarObject.AddComponent<ArmIK>();
            m_armIKRight.solver.isLeft = false;
            m_armIKRight.solver.SetChain(
                l_chest,
                l_animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                l_animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                l_animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                l_animator.GetBoneTransform(HumanBodyBones.RightHand),
                l_animator.transform
            );
            m_armIKRight.solver.arm.target = m_rightRotationTarget;
            m_armIKRight.solver.arm.positionWeight = 1f;
            m_armIKRight.solver.arm.rotationWeight = 1f;
            m_armIKRight.solver.IKPositionWeight = 0f;
            m_armIKRight.solver.IKRotationWeight = 0f;
            m_armIKRight.enabled = false;
        }

        void RemoveArmIK()
        {
            if(m_armIKLeft != null)
                Object.Destroy(m_armIKLeft);
            m_armIKLeft = null;

            if(m_armIKRight != null)
                Object.Destroy(m_armIKRight);
            m_armIKRight = null;
        }

        void UpdateArmIK(ArmIK p_ik, float p_weight)
        {
            if(p_ik != null)
            {
                bool l_state = !Mathf.Approximately(p_weight, 0f);
                p_ik.solver.IKPositionWeight = p_weight;
                p_ik.solver.IKRotationWeight = p_weight;
                p_ik.enabled = l_state;
            }
        }

        void SetLeadingHandState(HandState p_state)
        {
            switch(Settings.LeadingHand)
            {
                case Settings.LeadHand.Left:
                    m_leftHandState = p_state;
                    break;
                case Settings.LeadHand.Right:
                    m_rightHandState = p_state;
                    break;
                case Settings.LeadHand.Both:
                {
                    m_leftHandState = p_state;
                    m_rightHandState = p_state;
                }
                break;
            }
        }
        void SetUnleadingHandState(HandState p_state)
        {
            switch(Settings.LeadingHand)
            {
                case Settings.LeadHand.Left:
                    m_rightHandState = p_state;
                    break;
                case Settings.LeadHand.Right:
                    m_leftHandState = p_state;
                    break;
            }
        }

        IEnumerator WaitToDisable()
        {
            while(!Mathf.Approximately(m_handsWeights.x + m_handsWeights.y, 0f))
                yield return null;

            this.enabled = false;
            m_disableTask = null;
        }

        static float GetChainLength(Transform[] p_chain)
        {
            float l_result = 0f;
            for(int i = 0, j = p_chain.Length - 1; i < j; i++)
            {
                if((p_chain[i] != null) && (p_chain[i + 1] != null))
                    l_result += Vector3.Distance(p_chain[i].position, p_chain[i + 1].position);
            }
            return l_result;
        }
    }
}
