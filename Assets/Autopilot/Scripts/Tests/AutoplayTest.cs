using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autopilot.Tests
{
    /// <summary>
    /// Multiplayer tests using Anjin.
    /// </summary>
    /// <remarks>
    /// Required to create a build before running this test.
    /// </remarks>
    [TestFixture]
    [Timeout(310000)] // Autopilot lifespan + 10 seconds
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer)]
    public class AutoplayTest
    {
        private Process _externalPlayer;

        private static async UniTask<Process> LaunchExternalPlayer(string settingsPath, string logsPath)
        {
            var process = Process.Start(
                "Builds/Galactic Kittens.app/Contents/MacOS/Galactic Kittens",
                "-screen-width 320 -screen-height 180 " +
                $"-LAUNCH_AUTOPILOT_SETTINGS {settingsPath} " +
                $"-OUTPUT_ROOT_DIRECTORY_PATH {Path.GetFullPath(logsPath)}"
            );
            await UniTask.Delay(5000);
            return process;
        }

        private static AutopilotSettings LoadAutopilotSettings(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<AutopilotSettings>(
                Path.Combine("Assets/Autopilot/Resources", $"{path}.asset"));
#else
            return Resources.Load<AutopilotSettings>(path);
#endif
        }

        [SetUp]
        public async Task SetUp()
        {
            if (Application.isBatchMode)
            {
                PlayModeWindow.SetCustomRenderingResolution(320, 180, "QVGAW");
            }

            await SceneManager.LoadSceneAsync("Bootstrap");
        }

        [TearDown]
        public void TearDown()
        {
            if (_externalPlayer == null || _externalPlayer.HasExited)
            {
                return;
            }

            _externalPlayer.Kill();
            _externalPlayer.WaitForExit(500);
            _externalPlayer.Dispose();
            _externalPlayer = null;
        }

        [Test]
        public async Task MultiplayerHost()
        {
            var logsPath = Path.Combine("Logs", TestContext.CurrentContext.Test.Name);

            _externalPlayer = await LaunchExternalPlayer("Join/AutopilotSettings", Path.Combine(logsPath, "Join"));
            Assume.That(_externalPlayer, Is.Not.Null);
            Assume.That(_externalPlayer.HasExited, Is.False);

            var settings = LoadAutopilotSettings("Host/AutopilotSettings");
            settings.outputRootPath = Path.Combine(logsPath, "Host");

            await Launcher.LaunchAutopilotAsync(settings);
        }

        [Test]
        public async Task MultiplayerJoin()
        {
            var logsPath = Path.Combine("Logs", TestContext.CurrentContext.Test.Name);

            _externalPlayer = await LaunchExternalPlayer("Host/AutopilotSettings", Path.Combine(logsPath, "Host"));
            Assume.That(_externalPlayer, Is.Not.Null);
            Assume.That(_externalPlayer.HasExited, Is.False);

            var settings = LoadAutopilotSettings("Join/AutopilotSettings");
            settings.outputRootPath = Path.Combine(logsPath, "Join");

            await Launcher.LaunchAutopilotAsync(settings);
        }

        [Test]
        public async Task Singleplayer()
        {
            var logsPath = Path.Combine("Logs", TestContext.CurrentContext.Test.Name);

            var settings = LoadAutopilotSettings("Host/AutopilotSettings");
            settings.outputRootPath = logsPath;

            await Launcher.LaunchAutopilotAsync(settings);
        }
    }
}