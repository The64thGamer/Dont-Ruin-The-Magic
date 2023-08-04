#define USE_FPTL_LIGHTLIST 1
#pragma once
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl"
void TraceReflectionProbes(PositionInputs posInput, float3 normalWS, float3 rayDirection, inout float totalWeight, inout float3 result)
{
    uint envLightStart, envLightCount;
    GetCountAndStart(posInput, LIGHTCATEGORY_ENV, envLightStart, envLightCount);
    totalWeight = 0.0f;
    uint envStartFirstLane;
    bool fastPath = IsFastPath(envLightStart, envStartFirstLane);
    if (fastPath)
        envLightStart = envStartFirstLane;
    uint v_envLightListOffset = 0;
    uint v_envLightIdx = envLightStart;
    while (v_envLightListOffset < envLightCount)
    {
        v_envLightIdx = FetchIndex(envLightStart, v_envLightListOffset);
        uint s_envLightIdx = ScalarizeElementIndex(v_envLightIdx, fastPath);
        if (s_envLightIdx == -1)
            break;
        #ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
        s_envLightIdx = WaveReadLaneFirst(s_envLightIdx);
        #endif
        EnvLightData envLightData = FetchEnvLight(s_envLightIdx);    // Scalar load.
        if (s_envLightIdx >= v_envLightIdx)
        {
            v_envLightListOffset++;
            if (IsEnvIndexCubemap(envLightData.envIndex) && totalWeight < 1.0)
            {
                float3 R = rayDirection;
                float weight = 1.0f;
                float intersectionDistance = EvaluateLight_EnvIntersection(posInput.positionWS, normalWS, envLightData, envLightData.influenceShapeType, R, weight);
                int index = abs(envLightData.envIndex) - 1;
                float3 probeResult = 0 ;
                #if UNITY_VERSION >= 202220
                    float2 atlasCoords = GetReflectionAtlasCoordsCube(_CubeScaleOffset[index], R, 0);
                    probeResult = SAMPLE_TEXTURE2D_ARRAY_LOD(_ReflectionAtlas, s_trilinear_clamp_sampler, atlasCoords, 0, 0).rgb * envLightData.rangeCompressionFactorCompensation;
                #else 
                    probeResult = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, R, _EnvSliceSize * index, 0).rgb * envLightData.rangeCompressionFactorCompensation;
                #endif
                probeResult = ClampToFloat16Max(probeResult);
                UpdateLightingHierarchyWeights(totalWeight, weight);
                result += weight * probeResult * envLightData.multiplier;
            }
        }
    }
    totalWeight = saturate(totalWeight);
}
void TraceCustomProbe(PositionInputs posInput, TEXTURECUBE(_Cubemap), float3 normalWS, float3 rayDirection, float Compression, float multiplier,  inout float totalWeight, inout float3 result)
{
    float3 R = rayDirection;
    float3 probeResult = SAMPLE_TEXTURECUBE_LOD(_Cubemap, s_trilinear_clamp_sampler, R, 0).rgb * Compression.xxx;
    probeResult = ClampToFloat16Max(probeResult);
    result = probeResult * multiplier;
    totalWeight = 1;
}
