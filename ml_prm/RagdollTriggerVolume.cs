using ABI_RC.Core.Savior;
using ABI.CCK.Components;
using UnityEngine;

namespace ml_prm {

    public class RagdollTriggerVolume : CVRTriggerVolume
    {
        public Collider collider { get; set; }
        public RagdollTrigger trigger { get; set; }
        public void TriggerEnter(CVRPointer pointer) => trigger.OnPointerParticleEnter(pointer);
        public void TriggerExit(CVRPointer pointer)  => trigger.OnPointerParticleExit(pointer);
    }
}
