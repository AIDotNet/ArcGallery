using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using TKCode.Arc;
using TKCode.Arc.A2UI.Chat;
using TKCode.Arc.Localization;

[assembly: AvaloniaTestApplication(typeof(TKCode.Arc.A2UI.Chat.Tests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TKCode.Arc.A2UI.Chat.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
                  .UseSkia()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                  .UseArc(arc => arc.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]).UseArcA2UIChat());
}

public class TestApplication : Application
{
}

internal static class Ui
{
    public static void Pump()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>Runs the dispatcher until the task completes.</summary>
    public static void Wait(Task task, int timeoutMs = 10000)
    {
        var until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!task.IsCompleted && DateTime.UtcNow < until)
        {
            Pump();
            Thread.Sleep(1);
        }

        Pump();
        Assert.True(task.IsCompleted, "Timed out.");
        task.GetAwaiter().GetResult();
    }

    public static void Until(Func<bool> condition, int timeoutMs = 10000)
    {
        var until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < until)
        {
            Pump();
            Thread.Sleep(1);
        }

        Assert.True(condition(), "Condition not met in time.");
    }
}

/// <summary>An HTTP handler that answers every request with a canned SSE body and records the request.</summary>
internal sealed class FixtureHandler(string body, string contentType = "text/event-stream") : HttpMessageHandler
{
    public List<(HttpRequestMessage Request, string Body)> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var content = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request, content));
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, contentType)
        };
    }
}

/// <summary>A provider that plays fixed event lists, one per request.</summary>
internal sealed class ScriptedProvider(params ChatStreamEvent[][] responses) : IChatProvider
{
    private int _next;

    public List<ChatRequest> Requests { get; } = new();

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Requests.Add(request);
        var events = _next < responses.Length ? responses[_next++] : [new ChatTextDelta("ok"), new ChatCompleted(ChatStopReason.EndTurn)];
        foreach (var evt in events)
        {
            await Task.Yield();
            yield return evt;
        }
    }

    public Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ChatModelInfo>>([]);
}
