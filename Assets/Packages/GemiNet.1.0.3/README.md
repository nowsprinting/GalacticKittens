# GemiNet
Gemini Developer API client for .NET and Unity

[![NuGet](https://img.shields.io/nuget/v/GemiNet.svg)](https://www.nuget.org/packages/GemiNet)
[![Releases](https://img.shields.io/github/release/nuskey8/GemiNet.svg)](https://github.com/nuskey8/GemiNet/releases)
[![GitHub license](https://img.shields.io/github/license/nuskey8/GemiNet.svg)](./LICENSE)

English | [日本語](./README_JA.md)

## Overview

GemiNet is a Gemini Developer API client library for .NET/Unity. It is designed based on the official TypeScript SDK ([js-genai](https://github.com/googleapis/js-genai)), and provides an easy-to-use API compared to other libraries ([Google_GenerativeAI](https://github.com/gunpal5/Google_GenerativeAI), [GeminiSharp](https://github.com/dprakash2101/GeminiSharp), etc.). GemiNet also offers extension packages compatible with Microsoft.Extensions.AI abstraction layer, enabling smoother integration with your applications.

## Installation

### NuGet packages

GemiNet requires .NET Standard 2.1 or later. The package is available on NuGet.

### .NET CLI

```ps1
dotnet add package GemiNet
```

### Package Manager

```ps1
Install-Package GemiNet
```

### Unity

You can use GemiNet in Unity by using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). For details, please refer to the NuGetForUnity README.

## Quick Start

You can call the Gemini API using `GoogleGenAI`.

```cs
using GemiNet;

using var ai = new GoogleGenAI
{
    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
};

var response = await ai.Models.GenerateContentAsync(new()
{
    Model = Models.Gemini2_0Flash,    // models/gemini-2.0-flash
    Contents = "Hello, Gemini!"
});

Console.WriteLine(response.GetText());
```

Streaming generation is also supported.

```cs
var request = new GenerateContentRequest
{
    Model = Models.Gemini2_0Flash,
    Contents = "Hello, Gemini!"
};

await foreach (var response in GenerateContentStreamAsync(request))
{
    Console.WriteLine(response.GetText());
}
```

## Features

GemiNet's `GoogleGenAI` is modularized similar to the TypeScript SDK.

* `ai.Models` allows you to generate content, generate vectors, and retrieve available models.
* `ai.Caches` lets you create caches for specific inputs.
* `ai.Files` enables file uploads and deletions.
* `ai.Live` supports the Live API (bidirectional communication via WebSocket). For details, see the [Live API](#live-api) section.

## Live API

GemiNet supports the [Live API](https://ai.google.dev/api/live). You can read messages using `ReceiveAsync()` with `await foreach`.

```cs
using GemiNet;

using var ai = new GoogleGenAI();

await using var session = await ai.Live.ConnectAsync(new()
{
    Model = Models.Gemini2_0FlashLive,
    Config = new()
    {
        ResponseModalities = [Modality.Text]
    }
});

_ = Task.Run(async () =>
{
    await session.SendRealtimeInputAsync(new()
    {
        Text = "Hello, Gemini!"
    });
});

await foreach (var message in session.ReceiveAsync())
{
    Console.WriteLine(message.ServerContent?.ModelTurn?.Parts[0].Text);
}
```

## Microsoft.Extensions.AI

To use GemiNet with Microsoft.Extensions.AI, add the `GemiNet.Extensions.AI` package.

### Installation

#### .NET CLI

```ps1
dotnet add package GemiNet.Extensions.AI
```

#### Package Manager

```ps1
Install-Package GemiNet.Extensions.AI
```

### Usage

You can convert `GoogleGenAI` to Microsoft.Extensions.AI interfaces using `AsChatClient()` and `AsEmbeddingGenerator()`.

```cs
using GemiNet;
using GemiNet.Extensions.AI;

using var ai = new GoogleGenAI();

var client = ai.AsChatClient(Models.Gemini2_0Flash);
var response = await client.GetResponseAsync([new(ChatRole.User, "What is AI?")]);

var embeddingGenerator = ai.AsEmbeddingGenerator(Models.Gemini2_0Flash);
var embedding = await embeddingGenerator.GenerateEmbeddingAsync("Hello, Gemini!");
```

## Limitations

* This library does not currently support Vertex AI. If you want to use Vertex AI, please use [Google.Cloud.AIPlatform.V1](https://www.nuget.org/packages/Google.Cloud.AIPlatform.V1/) instead.

## License

This library is released under the [MIT License](./LICENSE).
