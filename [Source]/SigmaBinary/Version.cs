using UnityEngine;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly string number = "v1.6.7";
        void Awake()
        {
            UnityEngine.Debug.Log("[SigmaLog] Version Check:   Sigma Binary " + number);
        }
    }
}
