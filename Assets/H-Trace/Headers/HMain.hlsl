#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/TextureXR.hlsl"
#define H_TEXTURE                                                           TEXTURE2D_X
#define H_TEXTURE_ARRAY(textureName)                                        TEXTURE2D_ARRAY(textureName)
#define H_RW_TEXTURE(type, textureName)                                     RW_TEXTURE2D_X(type, textureName)
#define H_RW_TEXTURE_ARRAY(type, textureName)                               RW_TEXTURE2D_ARRAY(type, textureName)
#define H_TEXTURE_UINT2(textureName)                                        TEXTURE2D_X_UINT2(textureName)
#define H_RW_TEXTURE_UINT2(textureName)                                     RW_TEXTURE2D_X_UINT2(textureName)
#define H_LOAD(textureName, unCoord2)                                       LOAD_TEXTURE2D_X(textureName, unCoord2)
#define H_LOAD_LOD(textureName, unCoord2, lod)                              LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, lod)
#define H_LOAD_ARRAY(textureName, unCoord2, index)                          LOAD_TEXTURE2D_ARRAY(textureName, unCoord2, index)
#define H_LOAD_ARRAY_LOD(textureName, unCoord2, index, lod)                 LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, index, lod)
#define H_SAMPLE(textureName, samplerName, coord2)                          SAMPLE_TEXTURE2D_X(textureName, samplerName, coord2)
#define H_SAMPLE_LOD(textureName, samplerName, coord2, lod)                 SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)
#define H_SAMPLE_ARRAY_LOD(textureName, samplerName, coord2, index, lod)    SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)
#define H_GATHER_RED(textureName, samplerName, coord2, offset)              textureName.GatherRed(samplerName, coord2, offset)
#define H_COORD(pixelCoord)                                             COORD_TEXTURE2D_X(pixelCoord)
#define H_INDEX_ARRAY(slot)                                             INDEX_TEXTURE2D_ARRAY_X(slot)
#define H_TILE_SIZE                                                     TILE_SIZE_FPTL
#define HBUFFER_NORMAL_WS(pixCoordWS)                               GetNormalWS(pixCoordWS)
#define HBUFFER_ROUGHNESS(pixCoord)                                 GetRoughness(pixCoord)
#define HBUFFER_DEPTH(pixCoord)                                     GetDepth(pixCoord)
#define HBUFFER_COLOR(pixCoord)                                     GetColor(pixCoord)
#define HBUFFER_DIFFUSE(pixCoord)                                   GetDiffuse(pixCoord)
#define HBUFFER_MOTION_VECTOR(pixCoord)                             GetMotionVector(pixCoord)
#define H_SAMPLER_POINT_CLAMP                     s_point_clamp_sampler
#define H_SAMPLER_LINEAR_CLAMP                    s_linear_clamp_sampler
#define H_SAMPLER_LINEAR_REPEAT                   s_linear_repeat_sampler
#define H_SAMPLER_TRILINEAR_CLAMP                 s_trilinear_clamp_sampler
#define H_SAMPLER_TRILINEAR_REPEAT                s_trilinear_repeat_sampler
#define H_SAMPLER_LINEAR_CLAMP_COMPARE            s_linear_clamp_compare_sampler
#define UNITY_MATRIX_PREV_I_VP_H                  UNITY_MATRIX_PREV_I_VP
H_TEXTURE(_GBufferTexture0);
float3 GetNormalWS(uint2 pixCoordWS)
{
NormalData normalData;
DecodeFromNormalBuffer(pixCoordWS, normalData);
return normalData.normalWS;
}
float GetRoughness(uint2 pixCoord)
{
NormalData normalData;
DecodeFromNormalBuffer(pixCoord, normalData);
return normalData.perceptualRoughness;
}
float GetDepth(uint2 pixCoord)
{
return LoadCameraDepth(pixCoord);
}
float4 GetColor(uint2 pixCoord)
{
return LOAD_TEXTURE2D_X(_ColorPyramidTexture, pixCoord);
}
float4 GetDiffuse(uint2 pixCoord)
{
return LOAD_TEXTURE2D_X(_GBufferTexture0, pixCoord);
}
float2 GetMotionVector(uint2 pixCoord)
{
float2 MotionVectors;
DecodeMotionVector(LOAD_TEXTURE2D_X(_CameraMotionVectorsTexture, pixCoord), MotionVectors);
return MotionVectors;
}
