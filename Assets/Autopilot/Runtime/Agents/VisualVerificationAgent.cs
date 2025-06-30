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
    /// <see href="https://github.com/nuskey8/GemiNet"/>
    /// <see href="https://ai.google.dev/gemini-api/docs/image-understanding"/>
    [CreateAssetMenu(fileName = "New VisualVerificationAgent",
        menuName = "Anjin/GalacticKittens/Visual Verification Agent", order = 42)]
    public class VisualVerificationAgent : AbstractAgent
    {
        [Tooltip("処理開始までの遅延時間（秒）")]
        public int delay;

        [Tooltip("判定基準（Geminiに渡すプロンプト）")]
        public string prompt;

        [Tooltip("成功とみなすスコアの閾値（Max 1.0）")]
        public float threshold = 0.8f;

        public override async UniTask Run(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
                Logger.Log($"Enter {this.name}.Run()");

                var apikey = new Argument<string>("GEMINI_API_KEY");
                if (!apikey.IsCaptured())
                {
                    var message =
                        "GEMINI_API_KEY is not set. Please set it in the command line argument or environment variable.";
                    Logger.Log(message);
                    AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message).Forget();
                    return;
                }

                var coroutineRunner = new GameObject().AddComponent<CoroutineRunner>();
                await UniTask.WaitForEndOfFrame(coroutineRunner, cancellationToken);
                Destroy(coroutineRunner);
                // Note: UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate)では解像度を大きくしたらダメだった

                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                var bytes = texture.EncodeToPNG();
                var base64 = Convert.ToBase64String(bytes);
                // Note: GameViewの解像度が低いとLLMが読み取れないので注意

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
                if (score < threshold)
                {
                    var message = $"Failure! score:{score} comment:{comment}";
                    Logger.Log(message);
                    AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message).Forget();
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
