using UnityEngine;

/// <summary>
/// Adaptive quality manager. Monitors rolling average FPS and adjusts Unity quality
/// level to keep the game smooth on low-end devices.
/// DontDestroyOnLoad — call PerformanceManager.EnsureExists() once at startup.
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance { get; private set; }

    private float _fpsAccum;
    private int   _fpsFrames;
    private float _fpsMeasureTimer;
    private const float FpsMeasurePeriod = 3f;
    private float _averageFps;

    private float _lowFpsDuration;
    private float _highFpsDuration;
    private const float LowFpsThreshold  = 45f;
    private const float HighFpsThreshold = 58f;
    private const float DecreaseDuration = 5f;
    private const float IncreaseDuration = 15f;

    public static void EnsureExists()
    {
        if (Instance != null) return;
        var go = new GameObject("PerformanceManager");
        go.AddComponent<PerformanceManager>();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        QualitySettings.vSyncCount = 0;
        ApplyFrameRateTarget();
    }

    private void Update()
    {
        _fpsAccum  += Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
        _fpsFrames++;
        _fpsMeasureTimer += Time.unscaledDeltaTime;

        if (_fpsMeasureTimer < FpsMeasurePeriod)
            return;

        _averageFps      = _fpsAccum / _fpsFrames;
        _fpsAccum        = 0f;
        _fpsFrames       = 0;
        _fpsMeasureTimer = 0f;

        ApplyFrameRateTarget();
        AdjustQuality();
    }

    private void AdjustQuality()
    {
        if (_averageFps < LowFpsThreshold)
        {
            _lowFpsDuration  += FpsMeasurePeriod;
            _highFpsDuration  = 0f;

            if (_lowFpsDuration >= DecreaseDuration && QualitySettings.GetQualityLevel() > 0)
            {
                QualitySettings.DecreaseLevel(true);
                _lowFpsDuration = 0f;
                Utils.DebugLog($"[Perf] Quality decreased — avg FPS {_averageFps:F1}");
            }
        }
        else if (_averageFps > HighFpsThreshold)
        {
            _highFpsDuration += FpsMeasurePeriod;
            _lowFpsDuration   = 0f;

            if (_highFpsDuration >= IncreaseDuration &&
                QualitySettings.GetQualityLevel() < QualitySettings.names.Length - 1)
            {
                QualitySettings.IncreaseLevel(true);
                _highFpsDuration = 0f;
                Utils.DebugLog($"[Perf] Quality increased — avg FPS {_averageFps:F1}");
            }
        }
        else
        {
            _lowFpsDuration  = 0f;
            _highFpsDuration = 0f;
        }
    }

    private static void ApplyFrameRateTarget()
    {
#if UNITY_IOS
        Application.targetFrameRate = UnityEngine.iOS.Device.lowPowerModeEnabled ? 30 : 60;
#endif
    }
}
