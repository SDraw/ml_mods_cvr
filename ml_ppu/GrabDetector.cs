using ABI.CCK.Components;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_ppu
{
    class GrabDetector : MonoBehaviour
    {
        void OnTriggerEnter(Collider p_collider)
        {
            if(!Settings.Enabled)
                return;

            CVRPointer l_pointer = p_collider.GetComponent<CVRPointer>();
            if((l_pointer != null) && (l_pointer.type == "grab") && RestrictionsCheck(p_collider.transform.root))
                PickUpManager.Instance?.OnGrabDetected(p_collider, l_pointer);
        }

        static bool RestrictionsCheck(Transform p_transform)
        {
            if(p_transform == PlayerSetup.Instance.transform)
                return false;

            PlayerDescriptor l_playerDescriptor = p_transform.GetComponent<PlayerDescriptor>();
            if(l_playerDescriptor != null)
                return (!Settings.FriendsOnly || Friends.FriendsWith(l_playerDescriptor.ownerId));

            return false;
        }
    }
}
