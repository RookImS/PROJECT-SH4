using UnityEngine;

namespace Sh4
{
    public static class UnityEditorTools
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG")]
        public static void Log(object message) => Debug.Log(message);

        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG")]
        public static void Log(object message, Object context) => Debug.Log(message, context);

        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG"), System.Diagnostics.Conditional("UNITY_EDITOR_WARNING")]
        public static void Warning(object message) => Debug.LogWarning(message);

        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG"), System.Diagnostics.Conditional("UNITY_EDITOR_WARNING")]
        public static void Warning(object message, Object context) => Debug.LogWarning(message, context);

        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG"), System.Diagnostics.Conditional("UNITY_EDITOR_WARNING"), System.Diagnostics.Conditional("UNITY_EDITOR_ERROR")]
        public static void LogError(object message) => Debug.LogError(message);

        [System.Diagnostics.Conditional("UNITY_EDITOR_LOG"), System.Diagnostics.Conditional("UNITY_EDITOR_WARNING"), System.Diagnostics.Conditional("UNITY_EDITOR_ERROR")]
        public static void LogError(object message, Object context) => Debug.LogError(message, context);
    }
}
