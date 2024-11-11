using System.Linq;
using Cysharp.Threading.Tasks;
using DeNA.Anjin;
using DeNA.Anjin.Settings;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Autopilot.Scripts
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod]
        public static void LaunchAutopilotWhenMultiplayerPlayMode()
        {
            var tags = CurrentPlayer.ReadOnlyTags();
            if (!tags.Contains("autopilot"))
            {
                return;
            }

#if UNITY_EDITOR
            var settings =
                AssetDatabase.LoadAssetAtPath<AutopilotSettings>("Assets/Autopilot/Settings/AutopilotSettings.asset");
#endif
            Assert.IsNotNull(settings);
            Launcher.LaunchAutopilotAsync(settings).Forget();
        }
    }
}