using UnityEngine;
namespace HTrace
{
public static class HMath
{
public static float RemapThickness(float thickness, ThicknessMode thicknessMode)
{
float result = 0f;
switch (thicknessMode)
{
case ThicknessMode.Disabled:
break;
case ThicknessMode.Standard:
result = HExtensions.Remap(thickness, 0f, 1f, 0f, 0.15f);
break;
case ThicknessMode.Accurate:
result = HExtensions.Remap(thickness, 0f, 1f, 0f, 0.05f);
break;
default:
Debug.LogError($"RemapThickness ERROR: thickness: {thickness}, ThicknessMode: {thicknessMode}");
break;
}
return result;
}

/// <summary>
/// Thickness value pre-calculation for GI
/// </summary>
/// <param name="baseThickness"></param>
/// <param name="camera"></param>
/// <returns></returns>
public static Vector2 ThicknessBias(float baseThickness, Camera camera)
{
float n = camera.nearClipPlane;
float f = camera.farClipPlane;
float thicknessScale = 1.0f / (1.0f + baseThickness);
float thicknessBias = -n / (f - n) * (baseThickness * thicknessScale);
return new Vector2((float)thicknessScale, (float)thicknessBias);
}

public static Vector4 ComputeViewportScaleAndLimit(Vector2Int viewportSize, Vector2Int bufferSize)
{
return new Vector4(ComputeViewportScale(viewportSize.x, bufferSize.x),  // Scale(x)
ComputeViewportScale(viewportSize.y, bufferSize.y),                 // Scale(y)
ComputeViewportLimit(viewportSize.x, bufferSize.x),                 // Limit(x)
ComputeViewportLimit(viewportSize.y, bufferSize.y));                // Limit(y)
}

public static float PixelSpreadTangent(float Fov, int Width, int Height)
{
return Mathf.Tan(Fov * Mathf.Deg2Rad * 0.5f) * 2.0f / Mathf.Min(Width, Height);
}
private static float ComputeViewportScale(int viewportSize, int bufferSize)
{
float rcpBufferSize = 1.0f / bufferSize;
return viewportSize * rcpBufferSize;
}
private static float ComputeViewportLimit(int viewportSize, int bufferSize)
{
float rcpBufferSize = 1.0f / bufferSize;
return (viewportSize - 0.5f) * rcpBufferSize;
}
}
}
