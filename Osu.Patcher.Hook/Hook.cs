using System;
using System.Diagnostics;
using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using Osu.Patcher.Hook.Patches;
using Osu.Stubs.Wrappers;
using Osu.Utils;

namespace Osu.Patcher.Hook;

[UsedImplicitly]
public static class Hook
{
    private const string GithubUrl = "https://github.com/Inlayo-osu/osu-patcher";

    /// <summary>
    ///     Entry point into the hook called by the injector.
    /// </summary>
    [UsedImplicitly]
    public static int Initialize(string _)
    {
        var osuDir = Path.GetDirectoryName(OsuAssembly.Assembly.Location)!;

        try
        {
            Notifications.ShowMessage(
                "Loading osu!patcher...",
                NotificationColor.Neutral,
                3000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

#if DEBUG
        ConsoleHook.InitializeConsoleOutput();
        DebugHook.Initialize();
#else
        ConsoleHook.InitializeLogOutput(osuDir);
#endif

        try
        {
            InitializePatches(new Harmony("osu!patcher"));

            Notifications.ShowMessage(
                "osu!patcher initialized!",
                NotificationColor.Neutral,
                5000,
                () => { Process.Start(GithubUrl); }
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ShowErrorNotification();
        }

        return 0;
    }

    /// <summary>
    ///     For every type annotated with <c>[HarmonyPatch]</c> in this assembly, initialize the patch.
    /// </summary>
    /// <param name="harmony">Harmony patcher instance</param>
    private static void InitializePatches(Harmony harmony)
    {
        foreach (var type in AccessTools.GetTypesFromAssembly(typeof(Hook).Assembly))
        {
            if (!OsuPatchProcessor.IsOsuPatch(type))
                continue;

            Debug.WriteLine($"Processing Patch {type.Name}", "Hook");

            try
            {
                new OsuPatchProcessor(harmony, type).Patch();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to initialize patch {type.Name}");
                Console.WriteLine(e);
                ShowErrorNotification();
            }
        }
    }

    /// <summary>
    ///     Show a generic error notification to the user.
    /// </summary>
    private static void ShowErrorNotification()
    {
        try
        {
            Notifications.ShowMessage(
                "osu!patcher experienced an error! Click to report.",
                NotificationColor.Error,
                20000,
                () => { Process.Start($"{GithubUrl}/issues"); });
        }
        catch (Exception e2)
        {
            Console.WriteLine("Failed to show error notification!");
            Console.WriteLine(e2);
        }
    }
}