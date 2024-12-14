using System;
using System.Linq;
using System.Threading;
using Autopilot.TestDoubles;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Autopilot
{
    public enum Mode
    {
        Host,
        Join,
    }

    /// <summary>
    /// Agent for Menu scene.
    /// 1. Hit any key (using stub input)
    /// 2. Click "Host" or "Join" button
    /// </summary>
    [CreateAssetMenu(fileName = "New MenuAgent", menuName = "Anjin/GalacticKittens/Menu Agent", order = 40)]
    public class MenuAgent : AbstractAgent
    {
        [Tooltip("Use it if not specified by MPPM tags")]
        public Mode defaultMode;

        [Tooltip("Delay millis before click Buttons")]
        public int delayMillis;

        public override async UniTask Run(CancellationToken token)
        {
            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                var menuContainer = GameObject.Find("MenuContainer");
                if (menuContainer == null)
                {
                    await UniTask.Delay(delayMillis, ignoreTimeScale: true, cancellationToken: token);
                    await HitAnyKey(token);
                }

                await UniTask.Delay(delayMillis, ignoreTimeScale: true, cancellationToken: token);
                await SelectMode(token);
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }

        private async UniTask HitAnyKey(CancellationToken token)
        {
            var menuManager = FindAnyObjectByType<MenuManager>();
            Assert.IsNotNull(menuManager);

            menuManager.Input = new StubInputAnyKey(); // Hit any key to continue
            Logger.Log("Hit any key (stub input)");
            await UniTask.WaitWhile(() => GameObject.Find("MenuContainer") == null, cancellationToken: token);
        }

        private async UniTask SelectMode(CancellationToken token)
        {
            string buttonName;
            string[] tags;
            if (Application.isEditor && !Application.isBatchMode)
            {
                tags = CurrentPlayer.ReadOnlyTags();
            }
            else
            {
                tags = Array.Empty<string>();
            }

            if (tags.Contains("host"))
            {
                buttonName = "Host";
            }
            else if (tags.Contains("join"))
            {
                buttonName = "Join";
            }
            else
            {
                buttonName = this.defaultMode.ToString();
            }

            GameObject button = null;
            await UniTask.WaitWhile(() =>
            {
                button = GameObject.Find(buttonName);
                return button == null;
            }, cancellationToken: token);

            Logger.Log($"Click {button.name} button");
            button.GetComponent<Button>().onClick.Invoke();
        }
    }
}