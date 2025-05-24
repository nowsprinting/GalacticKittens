using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using InstantReplay;
using UnityEngine;

namespace Autopilot.Reporters
{
    [CreateAssetMenu(fileName = "New VideoRecordingReporter", menuName = "Anjin/Video Recording Reporter", order = 100)]
    public class VideoRecordingReporter : AbstractReporter
    {
        [field: SerializeField]
        public int NumFrames { get; private set; } = 900;

        [field: SerializeField]
        public double FixedFrameRate { get; private set; } = 30;

        [field: SerializeField]
        public int MaxWidth { get; private set; } = 320;

        [field: SerializeField]
        public int MaxHeight { get; private set; } = 180;

        private InstantReplaySession _session;

        [InitializeOnLaunchAutopilot]
        private static void InitializeReporter()
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (var reporter in AutopilotState.Instance.settings.reporters.OfType<VideoRecordingReporter>())
            {
                // Start recording
                reporter._session = new InstantReplaySession(
                    numFrames: reporter.NumFrames,
                    fixedFrameRate: reporter.FixedFrameRate,
                    maxWidth: reporter.MaxWidth,
                    maxHeight: reporter.MaxHeight);
            }
        }

        /// <inheritdoc/>
        public override async UniTask PostReportAsync(string message, string stackTrace, ExitCode exitCode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_session == null)
            {
                Debug.LogWarning("Video Recording is not started.");
                return;
            }

            var outputPath = await _session.StopAndTranscodeAsync(ct: cancellationToken);
            if (outputPath != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                var exportPath = Path.Combine(AutopilotState.Instance.settings.ScreenshotsPath, $"{this.name}.mp4");
                File.Move(outputPath, exportPath);
            }
            else
            {
                Debug.LogWarning("Video Exporting failed.");
            }

            _session.Dispose();
        }
    }
}
