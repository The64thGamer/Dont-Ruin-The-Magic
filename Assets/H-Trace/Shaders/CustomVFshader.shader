Shader "HTrace/CustomVFshader"
{
HLSLINCLUDE
#pragma target 4.5
#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
#pragma multi_compile_instancing
#pragma multi_compile _ DOTS_INSTANCING_ON
#pragma vertex Vert
#pragma fragment Frag
ENDHLSL
SubShader
{
Tags{ "RenderPipeline" = "HDRenderPipeline" }
Pass
{
Name "FrontFaceCullPass"
Tags { "LightMode" = "FirstPass" }
Blend  Off
ZWrite Off
ZTest LEqual
Cull Front
HLSLPROGRAM
#define _ALPHATEST_ON
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
PackedVaryingsType Vert(AttributesMesh inputMesh)
{
VaryingsType varyingsType;
varyingsType.vmesh = VertMesh(inputMesh);
return PackVaryingsType(varyingsType);
}
float4 Frag(PackedVaryingsToPS packedInput) : SV_Target
{    
return 0;
}
ENDHLSL
}
Pass
{
Name "MaskRenderPass"
Tags { "LightMode" = "FirstPass" }
Blend  Off
ZWrite Off
ZTest LEqual
Cull Back
HLSLPROGRAM
#define _ALPHATEST_ON
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
PackedVaryingsType Vert(AttributesMesh inputMesh)
{
VaryingsType varyingsType;
varyingsType.vmesh = VertMesh(inputMesh);
return PackVaryingsType(varyingsType);
}
float4 Frag(PackedVaryingsToPS packedInput) : SV_Target
{    
return 0;
}
ENDHLSL
}
}
}
