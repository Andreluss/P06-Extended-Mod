using MelonLoader;
using UnityEngine;

namespace P06X
{
    public class Main : MelonPlugin
    {
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
            Debug.Log("P-06 Extended (plugin) has been loaded!");
            // Initialize the X classes:
            XDebug.Instance.Log("P-06X " + XCommon.Version + " loaded", 3f);
            Debug.Log(XFiles.Instance);
        }
    }
}
