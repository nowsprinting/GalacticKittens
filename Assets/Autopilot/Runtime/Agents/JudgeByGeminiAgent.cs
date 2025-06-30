using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin;
using DeNA.Anjin.Agents;
using DeNA.Anjin.ArgumentCapture;
using GemiNet;
using UnityEngine;

namespace Autopilot.Agents
{
    [CreateAssetMenu(fileName = "New JudgeByGeminiAgent", menuName = "Anjin/GalacticKittens/Judge by Gemini Agent",
        order = 42)]
    public class JudgeByGeminiAgent : AbstractAgent
    {
        [Tooltip("処理開始までの遅延（秒）")]
        public int delay;

        [Tooltip("Geminiに渡すプロンプト")]
        [Multiline]
        public string prompt;

        [Tooltip("成功とみなすスコアの閾値")]
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

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                var texture = ScreenCapture.CaptureScreenshotAsTexture(); // TODO: asyncにすべき（Anjinに実装したい）
                var bytes = texture.EncodeToPNG();
                var base64 = Convert.ToBase64String(bytes);
                // Note: 画像サイズは20MB制限があるので、GameViewの解像度を下げておくこと！

                using var ai = new GoogleGenAI();
                ai.ApiKey = apikey.Value();
                var response = await ai.Models.GenerateContentAsync(new GenerateContentRequest
                    {
                        Model = Models.Gemini2_0Flash, // models/gemini-2.0-flash
                        Contents = Content.CreateUserContent(
                            Part.FromBase64(base64, "image/png"),
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

                Logger.Log($"Response: {response}");
                Logger.Log($"Response.text: {response.GetText()}");

                // TODO: thresholdを超えないときは失敗させる
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
