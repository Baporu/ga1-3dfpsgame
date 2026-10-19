using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class GameLogger
{
    // ENABLE_LOG 심볼이 정의되어 있을 때만 이 메서드가 호출됨
    [Conditional("ENABLE_LOG")]
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    [Conditional("ENABLE_LOG")]
    public static void Log(string message, Object obj)
    {
        Debug.Log(message, obj);
    }

    // Warning과 Error는 빌드 시에도 떠야 하므로 조건부 컴파일 속성을 넣지 않는다.
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public static void LogWarning(string message, Object obj)
    {
        Debug.LogWarning(message, obj);
    }

    public static void LogError(string message)
    {
        Debug.LogError(message);
    }

    public static void LogError(string message, Object obj)
    {
        Debug.LogError(message, obj);
    }
}