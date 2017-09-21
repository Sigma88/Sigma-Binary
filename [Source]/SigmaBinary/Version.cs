using UnityEngine;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class Version : MonoBehaviour
    {
        void Awake()
        {
            UnityEngine.Debug.Log("[SigmaLog] Version Check:   Sigma Binary v1.6.6");
        }
    }
}
