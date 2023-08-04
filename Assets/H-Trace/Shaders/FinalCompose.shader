Shader "HTrace/FinalCompose"
{
HLSLINCLUDE
#pragma vertex Vert
#pragma target 4.5
#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
#include "../Headers/HMain.hlsl"
#include "../../H-Trace/Headers/HMath.hlsl"
ENDHLSL
SubShader
{   
Pass
{
Name "FinalFramePass"
ZWrite Off
ZTest Always
Blend SrcAlpha OneMinusSrcAlpha
Cull Off
HLSLPROGRAM
#pragma fragment FinalFramePass
#pragma shader_feature_local DENOISE
#pragma shader_feature_local OVERWRITE_SSGI
#pragma shader_feature_local HALF_RESOLUTION
#pragma shader_feature_local DEBUG_GLOBAL_ILLUMINATION
#pragma shader_feature_local DEBUG_AMBIENT_OCCLUSION
#pragma shader_feature_local DEBUG_SPECULAR_OCCLUSION
#pragma shader_feature_local DEBUG_BENT_NORMALS
H_TEXTURE_ARRAY(_GI_Output);
TEXTURE2D_X(_MaskedDepth);
TEXTURE2D_X(_Color_Buffer_History);
struct Outputs
{   
float4 Final_Output : SV_Target0;
#ifndef OVERWRITE_SSGI
float4 Loop_Output  : SV_Target1;
#endif
};
Outputs FinalFramePass(Varyings varyings)
{
Outputs outputs;
float Depth = HBUFFER_DEPTH(varyings.positionCS.xy);
float MaskFromDepth = H_LOAD(_MaskedDepth, varyings.positionCS.xy).x;
if (MaskFromDepth <= Depth * DEPTH_MULTIPLIER)
MaskFromDepth = 0;
if (HBUFFER_DEPTH(varyings.positionCS.xy) <= 1e-7)
{
outputs.Final_Output = 0;
#ifndef OVERWRITE_SSGI
outputs.Loop_Output  = 0;
#endif
return outputs;
}
float2 UV = varyings.positionCS.xy;
#if defined HALF_RESOLUTION & !defined DENOISE
UV *= 0.5;
#endif
float3 Output   = H_LOAD_ARRAY(_GI_Output, UV, 0).xyz;
float3 Normals  = H_LOAD_ARRAY(_GI_Output, UV, 2).xyz;
float3 AO       = H_LOAD_ARRAY(_GI_Output, UV, 2).www;
float SO        = H_LOAD_ARRAY(_GI_Output, UV, 1).z;
float3 Albedo = HBUFFER_DIFFUSE(varyings.positionCS.xy).xyz;
float3 A =  2.0404 * Albedo - 0.3324;
float3 B = -4.7951 * Albedo + 0.6417;
float3 C =  2.7552 * Albedo + 0.6903;
AO = max(AO, ((AO * A + B) * AO + C) * AO);
float LumaOutput = saturate(Luminance(Output));
float LumaDelta = abs(AO.x - LumaOutput);
Output = Output * lerp(1, AO * AO, LumaDelta);
float3 DebuGI_Output = Output;
#ifndef OVERWRITE_SSGI
Output *= HBUFFER_DIFFUSE(varyings.positionCS.xy).xyz;
Output += HBUFFER_COLOR(varyings.positionCS.xy).xyz;
#endif
#ifdef DEBUG_AMBIENT_OCCLUSION
Output = AO;
#endif
#ifdef DEBUG_SPECULAR_OCCLUSION
Output = SO;
#endif
#ifdef DEBUG_BENT_NORMALS
Normals.yz = -Normals.yz;
Output = mul(UNITY_MATRIX_I_V, float4(Normals, 1)).xyz;
#endif
outputs.Final_Output = float4(Output.xyz, 1);
#ifdef DEBUG_GLOBAL_ILLUMINATION
outputs.Final_Output = float4(DebuGI_Output.xyz, 1);
#endif
#ifndef OVERWRITE_SSGI
outputs.Loop_Output  = float4(Output.xyz, 1);
#endif
return outputs;
}             
ENDHLSL
}
}
Fallback Off
}
