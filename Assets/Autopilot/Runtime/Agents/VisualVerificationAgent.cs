using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin;
using DeNA.Anjin.Agents;
using DeNA.Anjin.ArgumentCapture;
using GemiNet;
using UnityEngine;

namespace Autopilot.Agents
{
    /// <summary>
    /// Visual Verification Agent using Google Gemini AI.
    /// </summary>
    /// <see href="https://ai.google.dev/gemini-api/docs/image-understanding"/>
    /// <see href="https://github.com/nuskey8/GemiNet"/>
    [CreateAssetMenu(fileName = "New VisualVerificationAgent",
        menuName = "Anjin/GalacticKittens/Visual Verification Agent", order = 42)]
    public class VisualVerificationAgent : AbstractAgent
    {
        [Tooltip("判定開始までの遅延時間（秒）")]
        public int delaySec;

        [Tooltip("判定基準（Geminiに渡すプロンプト）")]
        public string prompt;

        [Tooltip("成功とみなすスコア（Max 1.0）の閾値")]
        public float successThreshold = 0.8f;

        [Tooltip("成功時にもAutopilotを終了する")]
        public bool terminateOnSuccess;

        public override async UniTask Run(CancellationToken cancellationToken)
        {
            var apikey = new Argument<string>("GEMINI_API_KEY");
            if (!apikey.IsCaptured())
            {
                Logger.Log(LogType.Warning,
                    $"Skip {this.name}.Run() because GEMINI_API_KEY is not set. Please set it in the command line argument or environment variable.");
                return;
            }

            try
            {
                Logger.Log($"Enter {this.name}.Run()");
                await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: cancellationToken);

                var coroutineRunner = new GameObject().AddComponent<CoroutineRunner>();
                await UniTask.WaitForEndOfFrame(coroutineRunner, cancellationToken);
                Destroy(coroutineRunner);
                // Note: UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate)では解像度を大きくしたらエラー（failed to generate texture! Was method called before the 'end of frame' state was reached?）が出るため、WaitForEndOfFrame(MonoBehaviour)を使用

                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                var bytes = texture.EncodeToPNG();
                var base64 = Convert.ToBase64String(bytes);
                // Note: GameViewの解像度が低いとLLMが画像を正確に読み取れず失敗します。800x600以上あれば安定しています

                using var ai = new GoogleGenAI();
                ai.ApiKey = apikey.Value();

                var file = await ai.Files.UploadAsync(
                    new UploadFileRequest()
                    {
                        File = new UploadFileContent(new Blob() { Data = base64, MimeType = "image/png" }),
                        MimeType = "image/png",
                    },
                    cancellationToken: cancellationToken);

                Logger.Log($"Upload screenshot to {file.Uri}");

                var response = await ai.Models.GenerateContentAsync(new GenerateContentRequest
                    {
                        Model = Models.Gemini2_0Flash, // models/gemini-2.0-flash
                        Contents = Content.CreateUserContent(
                            Part.FromUri(file.Uri, file.MimeType),
                            Part.FromText(prompt)
                        ),
                        SystemInstruction =
                            "Analyze the image and determine whether it meets the user prompt's requirements. The response consists of a score (maximum = 1.0) and a corresponding comment in Japanese.",
                        GenerationConfig = new GenerationConfig
                        {
                            ResponseMimeType = "application/json",
                            ResponseSchema = new Schema
                            {
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "score", new Schema { Type = DataType.Number } },
                                    { "comment", new Schema { Type = DataType.String } }
                                },
                                Required = new[] { "score", "comment" },
                                Type = DataType.Object
                            },
                        }
                    },
                    cancellationToken: cancellationToken);

                Logger.Log($"Response: {response.GetText()}\n{response}");

                var json = JsonDocument.Parse(response.GetText()).RootElement;
                var score = json.GetProperty("score").GetDouble();
                var comment = json.GetProperty("comment").GetString();
                if (score < successThreshold)
                {
                    var message = $"Visual verification is a failure! score:{score} comment:{comment}";
                    AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message).Forget();
                }

                if (terminateOnSuccess)
                {
                    var message = $"Visual verification is a success! score:{score} comment:{comment}";
                    AutopilotInstance.TerminateAsync(ExitCode.Normally, message).Forget();
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, e.ToString(), e.StackTrace).Forget();
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
