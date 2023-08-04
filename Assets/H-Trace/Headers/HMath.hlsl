#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariablesGlobal.cs.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#define HILBERT_LEVEL  6U
#define HILBERT_WIDTH  ((1U << HILBERT_LEVEL))
#define HILBERT_AREA   (HILBERT_WIDTH * HILBERT_WIDTH)
#define DEPTH_MULTIPLIER 0.99999
float3 GetPosition(float2 UV, float Depth) 
{ 
float2 p11_22 = float2(UNITY_MATRIX_P._11, UNITY_MATRIX_P._22);
float2 p13_31 = float2(UNITY_MATRIX_P._13, UNITY_MATRIX_P._23);
float3 Position = float3((UV * 2 - 1 - p13_31) / p11_22 , 1) * Depth;
return Position;
}
uint HilbertIndex( uint posX, uint posY )
{   
uint index = 0U;
for( uint curLevel = HILBERT_WIDTH/2U; curLevel > 0U; curLevel /= 2U )
{
uint regionX = ( posX & curLevel ) > 0U;
uint regionY = ( posY & curLevel ) > 0U;
index += curLevel * curLevel * ( (3U * regionX) ^ regionY);
if( regionY == 0U )
{
if( regionX == 1U )
{
posX = uint( (HILBERT_WIDTH - 1U) ) - posX;
posY = uint( (HILBERT_WIDTH - 1U) ) - posY;
}
uint temp = posX;
posX = posY;
posY = temp;
}
}
return index;
}
float2 SpatioTemporalNoise(float2 UV, uint TemporalIndex)
{ 
float Index = HilbertIndex(UV.x, UV.y);
Index += 288 * ( (_FrameCount * TemporalIndex) % 64); // Without TAA TemporalIndex is always 0 _Temporal_Enabled
return float2(frac(0.5 + Index * float2(0.75487766624669276005, 0.5698402909980532659114)));
}
void ColorToSH(float3 Color, float3 Direction, out float4 shY, out float2 CoCg)
{
float Co = Color.r - Color.b;
float t =  Color.b + Co * 0.5;
float Cg = Color.g - t;
float Y = max(t + Cg * 0.5, 0.0);
CoCg = float2(Co, Cg);
float L00 =  0.282095;
float L1_1 = 0.488603 * Direction.y;
float L10 =  0.488603 * Direction.z;
float L11 =  0.488603 * Direction.x;
shY = float4(L11, L1_1, L10, L00) * Y;
}
float3 ResolveSH(float4 shY, float2 CoCg, float3 N)
{
float d = dot(shY.xyz, N);
float Y = 2.0 * (1.023326 * d + 0.886226 * shY.w);
Y = max(Y, 0.0);
CoCg *= 0.282095 * Y / (shY.w + 1e-6);
float T = Y - CoCg.y * 0.5;
float G = CoCg.y + T;
float B = T - CoCg.x * 0.5;
float R = B + CoCg.x;
return max(float3(R, G, B), 0.0);
}
uint2 ThreadGroupTilingX (
const uint2 dipatchGridDim,		// Arguments of the Dispatch call, eg:[numthreads(8, 8, 1)] -> uint2(_ScreenSize.x / 8, _ScreenSize.y / 8)
const uint2 ctaDim,			    // Already known in HLSL, eg:[numthreads(8, 8, 1)] -> uint2(8, 8)
const uint maxTileWidth,		// User parameter (N). Recommended values: 8, 16 or 32.
const uint2 groupThreadID,		// SV_GroupThreadID
const uint2 groupId			    // SV_GroupID
)
{
const uint Number_of_CTAs_in_a_perfect_tile = maxTileWidth * dipatchGridDim.y;
const uint Number_of_perfect_tiles = dipatchGridDim.x / maxTileWidth;
const uint Total_CTAs_in_all_perfect_tiles = Number_of_perfect_tiles * maxTileWidth * dipatchGridDim.y;
const uint vThreadGroupIDFlattened = dipatchGridDim.x * groupId.y + groupId.x;
const uint Tile_ID_of_current_CTA = vThreadGroupIDFlattened / Number_of_CTAs_in_a_perfect_tile;
const uint Local_CTA_ID_within_current_tile = vThreadGroupIDFlattened % Number_of_CTAs_in_a_perfect_tile;
uint Local_CTA_ID_y_within_current_tile;
uint Local_CTA_ID_x_within_current_tile;
if (Total_CTAs_in_all_perfect_tiles <= vThreadGroupIDFlattened)
{
uint X_dimension_of_last_tile = dipatchGridDim.x % maxTileWidth;
#ifdef DXC_STATIC_DISPATCH_GRID_DIM
X_dimension_of_last_tile = max(1, X_dimension_of_last_tile);
#endif
Local_CTA_ID_y_within_current_tile = Local_CTA_ID_within_current_tile / X_dimension_of_last_tile;
Local_CTA_ID_x_within_current_tile = Local_CTA_ID_within_current_tile % X_dimension_of_last_tile;
}
else
{
Local_CTA_ID_y_within_current_tile = Local_CTA_ID_within_current_tile / maxTileWidth;
Local_CTA_ID_x_within_current_tile = Local_CTA_ID_within_current_tile % maxTileWidth;
}
const uint Swizzled_vThreadGroupIDFlattened =
Tile_ID_of_current_CTA * maxTileWidth +
Local_CTA_ID_y_within_current_tile * dipatchGridDim.x +
Local_CTA_ID_x_within_current_tile;
uint2 SwizzledvThreadGroupID;
SwizzledvThreadGroupID.y = Swizzled_vThreadGroupIDFlattened / dipatchGridDim.x;
SwizzledvThreadGroupID.x = Swizzled_vThreadGroupIDFlattened % dipatchGridDim.x;
uint2 SwizzledvThreadID;
SwizzledvThreadID.x = ctaDim.x * SwizzledvThreadGroupID.x + groupThreadID.x;
SwizzledvThreadID.y = ctaDim.y * SwizzledvThreadGroupID.y + groupThreadID.y;
return SwizzledvThreadID.xy;
}
float3x3 RotFromToMatrix( float3 from, float3 to )
{
const float e       = dot(from, to);
const float f       = abs(e);
const float3 v      = cross( from, to );
const float h       = (1.0)/(1.0 + e);
const float hvx     = h * v.x;
const float hvz     = h * v.z;
const float hvxy    = hvx * v.y;
const float hvxz    = hvx * v.z;
const float hvyz    = hvz * v.y;
float3x3 mtx;
mtx[0][0] = e + hvx * v.x;
mtx[0][1] = hvxy - v.z;
mtx[0][2] = hvxz + v.y;
mtx[1][0] = hvxy + v.z;
mtx[1][1] = e + h * v.y * v.y;
mtx[1][2] = hvyz - v.x;
mtx[2][0] = hvxz - v.y;
mtx[2][1] = hvyz + v.x;
mtx[2][2] = e + hvz * v.z;
return mtx;
}
float HFastSqrt(float x)
{
return (asfloat(0x1fbd1df5 + (asint(x) >> 1)));
}
float HFastACos( float inX )
{ 
float pi = 3.141593;
float half_pi = 1.570796;
float x = abs(inX); 
float res = -0.156583 * x + half_pi;
res *= HFastSqrt(1.0 - x);
return (inX >= 0) ? res : pi - res;
}
float sqr(float value)
{ return value * value; }
float Gaussian(float radius, float sigma)
{ return exp(-sqr(radius / sigma)); }
float PackFloats(float a, float b)
{
uint a16 = f32tof16(a);
uint b16 = f32tof16(b);
uint abPacked = (a16 << 16) | b16;
return asfloat(abPacked);
}
void UnpackFloats(float input, out float a, out float b)
{
uint uintInput = asuint(input);
a = f16tof32(uintInput >> 16);
b = f16tof32(uintInput);
}
float4 PackFloats4(float4 a, float4 b)
{
uint4 a16 = f32tof16(a);
uint4 b16 = f32tof16(b);
uint4 abPacked = (a16 << 16) | b16;
return asfloat(abPacked);
}
void UnpackFloats4(float4 input, out float4 a, out float4 b)
{
uint4 uintInput = asuint(input);
a = f16tof32(uintInput >> 16);
b = f16tof32(uintInput);
}
uint ConvertToUint(float input)
{
uint ui = asuint(input);
uint ui1 = ui & 0xffff;
uint ui2 = ui >> 16;
return ui2;
}
float SpecularOcclusion(float3 V, float3 bentNormalWS, float3 normalWS, float ambientOcclusion, float roughness)
{
float vs = -1.0f / min(sqrt(1.0f - ambientOcclusion) - 1.0f, -0.001f);
float us = 0.8f;
float NoV = dot(V, normalWS);
float3 NDFAxis = (2 * NoV * normalWS - V) * (0.5f / max(roughness * roughness * NoV, 0.001f));
float umLength1 = length(NDFAxis + vs * bentNormalWS);
float umLength2 = length(NDFAxis + us * normalWS);
float d1 = 1 - exp(-2 * umLength1);
float d2 = 1 - exp(-2 * umLength2);
float expFactor1 = exp(umLength1 - umLength2 + us - vs);
return saturate(expFactor1 * (d1 * umLength2) / (d2 * umLength1));
}
#define MAX_REPROJECTION_DISTANCE 0.1
#define MAX_PIXEL_TOLERANCE 4
#define PROJECTION_EPSILON 0.000001
float ComputeMaxReprojectionWorldRadius(float3 positionWS, float3 normalWS, float pixelSpreadAngleTangent, float maxDistance, float pixelTolerance)
{
const float3 viewWS = GetWorldSpaceNormalizeViewDir(positionWS);
float parallelPixelFootPrint = pixelSpreadAngleTangent * length(positionWS);
float realPixelFootPrint = parallelPixelFootPrint / max(abs(dot(normalWS, viewWS)), PROJECTION_EPSILON);
return max(maxDistance, realPixelFootPrint * pixelTolerance);
}
float ComputeMaxReprojectionWorldRadius(float3 positionWS, float3 normalWS, float pixelSpreadAngleTangent)
{
return ComputeMaxReprojectionWorldRadius(positionWS, normalWS, pixelSpreadAngleTangent, MAX_REPROJECTION_DISTANCE, MAX_PIXEL_TOLERANCE);
}
float3x3 HGetLocalFrame(float3 localZ)
{
float x  = localZ.x;
float y  = localZ.y;
float z  = localZ.z;
float sz = FastSign(z);
float a  = 1 / (sz + z);
float ya = y * a;
float b  = x * ya;
float c  = x * sz;
float3 localX = float3(c * x * a - 1, sz * b, c);
float3 localY = float3(b, y * ya - sz, y);
return float3x3(localX, localY, localZ);
}
real2 HSampleDiskCubic(real u1, real u2)
{
real r   = u1;
real phi = TWO_PI * u2;
real sinPhi, cosPhi;
sincos(phi, sinPhi, cosPhi);
return r * real2(cosPhi, sinPhi);
}
