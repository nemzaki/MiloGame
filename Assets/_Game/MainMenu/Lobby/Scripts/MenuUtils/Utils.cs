using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public static class Utils
{
    public static bool TryGetQuantumFrame(out Frame frame)
    {
        frame = null;

        if (QuantumRunner.Default == null)
            return false;

        if (QuantumRunner.Default.Game == null)
            return false;

        frame = QuantumRunner.Default.Game.Frames.Predicted;

        if (frame == default)
            return false;

        return true;
    }

    public static void DebugLog(string message)
    {
        Debug.Log($"{Time.time} {message}");
    }

    public static void DebugLogWarning(string message)
    {
        Debug.LogWarning($"{Time.time} {message}");
    }

    public static void DebugLogError(string message)
    {
        Debug.LogError($"{Time.time} {message}");
    }

    public static string GetRandomName()
    {
        return "player" + Random.Range(1000, 6000);
    }

}