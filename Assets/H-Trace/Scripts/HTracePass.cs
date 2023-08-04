using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;
namespace HTrace
{
public class HTracePass : CustomPass
{
private string ERROR_OUT_RANGE_VALUE = "Your \"{0}\" value is out of range: {1}";
private const string HRenderGI_SHADER_NAME = "HRenderGI";
private const string HMipChain_SHADER_NAME = "HMipChain";
private const string HDenoiseTemporal_SHADER_NAME = "HDenoiseTemporal";
private const string HDenoiseSpatial_SHADER_NAME = "HDenoiseSpatial";
private const string HDenoiseCompose_SHADER_NAME = "HDenoiseCompose";
#region PROPERTY SCRIPTING PROTECTION -------------------------->
[SerializeField]
private float _aoRadius = 0.5f;
/// <summary>
/// Ambient Occlusion radius
/// </summary>
/// <value>[0;1]</value>
public float AORadius
{
get => _aoRadius;    
set
{
if(Mathf.Abs(value - _aoRadius) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1f)
{
_aoRadius = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(AORadius), "[0;1]");
}
}
}
[SerializeField]
private float _intensityAO = 0.25f;
/// <summary>
/// Ambient Occlusion power
/// </summary>
/// <value>[0.00;1]</value>
public float IntensityAO
{
get => _intensityAO;    
set
{
if(Mathf.Abs(value - _intensityAO) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1.0f)
{
_intensityAO = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(IntensityAO), "[0.00;1]");
}
}
}
[SerializeField]
private float _distributionPower = 3.2f;
/// <summary>
/// Distribution power
/// </summary>
/// <value>[1;4]</value>
public float DistributionPower
{
get => _distributionPower;    
set
{
if(Mathf.Abs(value - _distributionPower) < Mathf.Epsilon)
return;
if (value >= 1f && value <= 4f)
{
_distributionPower = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(DistributionPower), "[1,4]");
}
}
}
[SerializeField]
private int _sliceCount = 1;
/// <summary>
/// Slice count
/// </summary>
/// <value>[1;4]</value>
public int SliceCount
{
get => _sliceCount;    
set
{
if(value == _sliceCount)
return;
if (value >= 1 && value <= 8)
{
_sliceCount = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(SliceCount), "[1;8]");
}
}
}
[SerializeField]
private int _stepCount = 24;
/// <summary>
/// Step count
/// </summary>
/// <value>[0.01;64]</value>
public int StepCount
{
get => _stepCount;    
set
{
if(value == _stepCount)
return;
if (value >= 1 && value <= 64)
{
_stepCount = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(StepCount), "[1;64]");
}
}
}
[SerializeField]
private float _thickness = 0.25f;
/// <summary>
/// Thickness
/// </summary>
/// <value>[0.0;1.0]</value>
public float Thickness
{
get => _thickness;    
set
{
if(Mathf.Abs(value - _thickness) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1f)
{
_thickness = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(Thickness), "[0.0;1.0]");
}
}
}
[SerializeField]
private float _denoiserRadius = 0.15f;
/// <summary>
/// Denoiser Radius
/// </summary>
/// <value>[0.001;1]</value>
public float DenoiserRadius
{
get => _denoiserRadius;    
set
{
if(Mathf.Abs(value - _denoiserRadius) < Mathf.Epsilon)
return;
if (value >= 0.001f && value <= 0.5f)
{
_denoiserRadius = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(DenoiserRadius), "[0.001;0.5]");
}
}
}
[SerializeField]
private float _fakeIntensity = 0.3f;
/// <summary>
/// Fake Intensity
/// </summary>
/// <value>[0,1]</value>
public float FakeIntensity
{
get => _fakeIntensity;    
set
{
if(Mathf.Abs(value - _fakeIntensity) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1f)
{
_fakeIntensity = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(FakeIntensity), "[0.0,1.0]");
}
}
}
[SerializeField]
private float _intensitySO = 0.25f;
/// <summary>
/// SO Power
/// </summary>
/// <value>[0,1]</value>
public float IntensitySO
{
get => _intensitySO;    
set
{
if(Mathf.Abs(value - _intensitySO) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1f)
{
_intensitySO = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(IntensitySO), "[0.0,1.0]");
}
}
}
[SerializeField]
private int _denoiserSampleCount = 8;
/// <summary>
/// Denoiser sample count
/// </summary>
/// <value>[8,24]</value>
public int DenoiserSampleCount
{
get => _denoiserSampleCount;    
set
{
if(value == _denoiserSampleCount)
return;
if (value >= 1 && value <= 64)
{
_denoiserSampleCount = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(DenoiserSampleCount), "[8;24]");
}
}
}
[SerializeField]
private float _detailPreservation = 0.8f;
/// <summary>
/// Detail Preservation
/// </summary>
/// <value>[0,1]</value>
public float DetailPreservation
{
get => _detailPreservation;    
set
{
if(Mathf.Abs(value - _detailPreservation) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 1f)
{
_detailPreservation = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(DetailPreservation), "[0.0,1.0]");
}
}
}
[SerializeField]
private float _mainIntensity = 1f;
/// <summary>
/// Main Intensity
/// </summary>
/// <value>[0,2]</value>
public float MainIntensity
{
get => _mainIntensity;    
set
{
if(Mathf.Abs(value - _mainIntensity) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 2f)
{
_mainIntensity = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(MainIntensity), "[0.0,2.0]");
}
}
}
[SerializeField]
private float _fallbackIntensity = 1f;
/// <summary>
/// Main Intensity
/// </summary>
/// <value>[0,2]</value>
public float FallbackIntensity
{
get => _fallbackIntensity;    
set
{
if(Mathf.Abs(value - _fallbackIntensity) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 2f)
{
_fallbackIntensity = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(FallbackIntensity), "[0.0,2.0]");
}
}
}
[SerializeField]
private float _accumulationSpeed = 1f;
/// <summary>
/// Main Intensity
/// </summary>
/// <value>[0,5]</value>
public float AccumulationSpeed
{
get => _accumulationSpeed;    
set
{
if(Mathf.Abs(value - _accumulationSpeed) < Mathf.Epsilon)
return;
if (value >= 0.0f && value <= 5f)
{
_accumulationSpeed = value;
}
else
{
Debug.LogErrorFormat(ERROR_OUT_RANGE_VALUE, nameof(AccumulationSpeed), "[0.0,5.0]");
}
}
}
#endregion
#region PUBLIC PROPERTY SETTINGS -------------------------->
public DenoiserIntensity DenoiserIntensity = DenoiserIntensity.Medium;
public ResolutionScale ResolutionScale = ResolutionScale.None;
public ThicknessMode ThicknessMode = ThicknessMode.Standard;
public FallbackMode FallbackMode = FallbackMode.ReflectionProbes;
public NormalsMode NormalsMode = NormalsMode.BentNormals;
public DebugMode DebugMode = DebugMode.None;
public LayerMask HTraceLayer = 1;
public LayerMask ThicknessLayer = 0;
public HDAdditionalReflectionData CustomProbe;
public bool StochasticThickness = true;
public bool FullPrecision = true;
public bool RecurrentBlur = true;
public bool HistoryFilter = false;
public bool DenoiseOcclusion = true;
[SerializeField]
private bool _enableFallback = true;
internal bool EnableFallback
{
get
{
return _enableFallback;
}
set
{
_enableFallback = value;
}
}
[SerializeField]
private bool _enableOcclusion = true;
internal bool EnableOcclusion
{
get
{
return _enableOcclusion;
}
set
{
_enableOcclusion = value;
}
}
[SerializeField]
private bool _enableDenoising = true;
internal bool EnableDenoising
{
get
{
return _enableDenoising;
}
set
{
_enableDenoising = value;
}
}
[SerializeField]
private int InjectionPointChecker = 0;
#endregion
private Vector2 _actualResolution = new Vector2(0, 0);
private bool _actualThicknessAware = false;
private bool _actualDenoising = false;
private bool _overwriteSSGI = false;
private int _actualThicknessMode = 1;
private ComputeShader HRenderGI = null;
private ComputeShader HMipChain = null;
private ComputeShader HDenoiseTemporal = null;
private ComputeShader HDenoiseSpatial = null;
private ComputeShader HDenoiseCompose = null;
private ProfilingSampler renderDepthSampler = new ProfilingSampler("Render Depth");
private ProfilingSampler generateMipSampler = new ProfilingSampler("Generate Mips");
private ProfilingSampler renderGISampler = new ProfilingSampler("Render GI");
private ProfilingSampler denoiseSampler = new ProfilingSampler("Denoise");
private ProfilingSampler outputSampler = new ProfilingSampler("Output");
[SerializeField]
Shader                  FinalOutputShader;
Material                FinalOutputMaterial;
Shader                  CustomVFshader;
Material                CustomVFmaterial;
[SerializeField]
ComputeBuffer pointDistribution;
#region RTHADNLES ------------------------------------>
RTHandle Depth_Front_Buffer;
RTHandle Normal_Buffer;
RTHandle Color_Buffer;
RTHandle Depth_Back_Render;
RTHandle Depth_Back_Filter;
RTHandle Depth_Back_Buffer;
RTHandle MaskedDepth;
RTHandle GI_Output;
RTHandle HTrace_Output_Bent_AO;
RTHandle HTrace_Output_GI;
RTHandle LoopOutput;
RTHandle HTrace_Output_SO;
RTHandle Normal_Buffer_History;
RTHandle Depth_Buffer_History;
RTHandle Temporal_History_1;
RTHandle Temporal_Output_1;
RTHandle Spatial_Output_1;
RTHandle Temporal_History_2;
RTHandle Temporal_Output_2;
RTHandle Spatial_Output_2;
RTHandle Denoiser_History_Fix;
RTHandle Spatial_Cache_Weights;
RTHandle Spatial_Cache_UVs;
RTHandle Temporal_Cache_Weights;
RTHandle Denoiser_Output;
RTHandle DefaultBlackCube;
#endregion
#region MATERIAL & RESOURCE LOAD --------------------->
private void ResourcesLoad()
{
HRenderGI = HExtensions.LoadComputeShader(HRenderGI_SHADER_NAME);
HMipChain = HExtensions.LoadComputeShader(HMipChain_SHADER_NAME);
HDenoiseTemporal = HExtensions.LoadComputeShader(HDenoiseTemporal_SHADER_NAME);
HDenoiseSpatial = HExtensions.LoadComputeShader(HDenoiseSpatial_SHADER_NAME);
HDenoiseCompose = HExtensions.LoadComputeShader(HDenoiseCompose_SHADER_NAME);
}
private void MaterialSetup()
{
CustomVFshader = Shader.Find("HTrace/CustomVFshader");
CustomVFmaterial = CoreUtils.CreateEngineMaterial(CustomVFshader);
FinalOutputShader = Shader.Find("HTrace/FinalCompose");
FinalOutputMaterial = CoreUtils.CreateEngineMaterial(FinalOutputShader);
}
#endregion
#region TEXTURE ALLOCATION --------------------------->
private void AllocateMainBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Depth_Front_Buffer);
RTHandles.Release(Normal_Buffer);
RTHandles.Release(Color_Buffer);
RTHandles.Release(GI_Output);
RTHandles.Release(DefaultBlackCube);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Depth_Front_Buffer = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false, 
colorFormat: GraphicsFormat.R16_SFloat, name: "_Depth_Front_Buffer", enableRandomWrite: true);
Normal_Buffer = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false,
colorFormat: GraphicsFormat.R8G8B8A8_SNorm, name: "_Normal_Buffer", enableRandomWrite: true);
Color_Buffer = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: true,
colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, name: "_Color_Buffer", enableRandomWrite: true);
GI_Output = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray, 
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_GI_Output", enableRandomWrite: true);
DefaultBlackCube = RTHandles.Alloc(16, 16, 1, dimension: TextureDimension.Cube,
colorFormat: GraphicsFormat.R8_SInt, name: "_DefaultBlackCube");
}
private void AllocateBackfaceDepthBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Depth_Back_Render);
RTHandles.Release(Depth_Back_Filter);
RTHandles.Release(Depth_Back_Buffer);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Depth_Back_Render = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension,
colorFormat: GraphicsFormat.R16_SFloat, name: "_Depth_Back_Render", depthBufferBits: DepthBits.Depth32);
Depth_Back_Filter = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false,
colorFormat: GraphicsFormat.R16_SFloat, name: "_Depth_Back_Filter", enableRandomWrite: true);
Depth_Back_Buffer = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false, 
colorFormat: GraphicsFormat.R16_SFloat, name: "_Depth_Back_Buffer", enableRandomWrite: true);
}
private void AllocateFullIntegrationBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(HTrace_Output_GI);
RTHandles.Release(HTrace_Output_SO);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
HTrace_Output_GI = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray, 
colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, name: "_HTrace_Output_GI", enableRandomWrite: true);
HTrace_Output_SO = RTHandles.Alloc(Vector2.one, dimension: TextureDimension.Tex2DArray, 
colorFormat: GraphicsFormat.R16_SFloat, name: "_HTrace_Output_SO", enableRandomWrite: true);
}
private void AllocateLoopBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(LoopOutput);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
LoopOutput = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension,
colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, name: "_LoopOutput", enableRandomWrite: true);
}
private void AllocateMaskBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(MaskedDepth);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
MaskedDepth = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension,
colorFormat: GraphicsFormat.R16_SFloat, name: "_MaskedDepth", depthBufferBits: DepthBits.Depth32);
}
private void AllocateMainDenoiserBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Normal_Buffer_History);
RTHandles.Release(Depth_Buffer_History);
RTHandles.Release(Denoiser_Output);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Normal_Buffer_History = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false,
colorFormat: GraphicsFormat.R8G8B8A8_SNorm, name: "_Normal_Buffer_History", enableRandomWrite: true);
Depth_Buffer_History = RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension, useMipMap: true, autoGenerateMips: false, 
colorFormat: GraphicsFormat.R32_SFloat, name: "_Depth_Buffer_History", enableRandomWrite: true);
Denoiser_Output = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Denoiser_Output", enableRandomWrite: true);
}
private void AllocateLowDenoiserBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Temporal_History_1);
RTHandles.Release(Temporal_Output_1);
RTHandles.Release(Spatial_Output_1);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Temporal_History_1 = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Temporal_History_1", enableRandomWrite: true);
Temporal_Output_1 = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Temporal_Output_1", enableRandomWrite: true);
Spatial_Output_1 =  RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Spatial_Output_1", enableRandomWrite: true);
}
private void AllocateMediumDenoiserBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Temporal_History_2);
RTHandles.Release(Temporal_Output_2);
RTHandles.Release(Temporal_Cache_Weights);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Temporal_History_2 = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Temporal_History_2", enableRandomWrite: true);
Temporal_Output_2 = RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Temporal_Output_2", enableRandomWrite: true);
Temporal_Cache_Weights =  RTHandles.Alloc(Vector2.one, dimension: TextureXR.dimension,
colorFormat: GraphicsFormat.R16_SFloat, name: "_Temporal_Cache_Weights", enableRandomWrite: true);
}
private void AllocateHighDenoiserBuffers(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Spatial_Output_2);
RTHandles.Release(Spatial_Cache_Weights);
RTHandles.Release(Spatial_Cache_UVs);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Spatial_Output_2 =  RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Spatial_Output_2", enableRandomWrite: true);
Spatial_Cache_Weights =  RTHandles.Alloc(Vector2.one, 8, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16_SFloat, name: "_Spatial_Cache_Weights", enableRandomWrite: true);
Spatial_Cache_UVs =  RTHandles.Alloc(Vector2.one, 8, dimension: TextureDimension.Tex2DArray,
colorFormat: GraphicsFormat.R16G16_SFloat, name: "_Spatial_Cache_UVs", enableRandomWrite: true);
}
private void AllocateHistoryFixBuffer(bool onlyRelease = false)
{
void ReleaseTextures()
{
RTHandles.Release(Denoiser_History_Fix);
}
if (onlyRelease)
{
ReleaseTextures();
return;
}
ReleaseTextures();
Denoiser_History_Fix =  RTHandles.Alloc(Vector2.one, 3, dimension: TextureDimension.Tex2DArray, useMipMap: true, autoGenerateMips: true,
colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_Denoiser_History_Fix ", enableRandomWrite: true);
}
#endregion
protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
{
ResourcesLoad();
MaterialSetup();
pointDistribution = new ComputeBuffer(16 * 4, 2 * sizeof(float));
}
private void ReallocTextures(Vector2 currentResolution)
{
if (_actualResolution != currentResolution || _actualThicknessMode != (int)ThicknessMode || _actualDenoising != EnableDenoising)
{
_actualResolution = currentResolution; ;
_actualThicknessMode = (int)ThicknessMode;
_actualDenoising = EnableDenoising;
AllocateMainBuffers();
AllocateLoopBuffers();
AllocateFullIntegrationBuffers();
AllocateBackfaceDepthBuffers(ThicknessMode != ThicknessMode.Accurate);
AllocateMaskBuffers(HTraceLayer == 0);
AllocateMainDenoiserBuffers(!EnableDenoising);
AllocateLowDenoiserBuffers(!EnableDenoising);
AllocateMediumDenoiserBuffers(!EnableDenoising && DenoiserIntensity != DenoiserIntensity.Low);
AllocateHighDenoiserBuffers(!EnableDenoising && DenoiserIntensity != DenoiserIntensity.Low && DenoiserIntensity != DenoiserIntensity.Medium);
AllocateHistoryFixBuffer(!EnableDenoising && !HistoryFilter);
}
}
protected override void Execute(CustomPassContext ctx)
{   
var cmdList = ctx.cmd;
var hdCamera = ctx.hdCamera.camera;
int ScreenResX = ctx.hdCamera.actualWidth;
int ScreenResY = ctx.hdCamera.actualHeight;
InjectionPointChecker = (int)injectionPoint;
if ((RecurrentBlur || HistoryFilter) && DenoiserIntensity == DenoiserIntensity.Low)
{
RecurrentBlur = false;
HistoryFilter = false;
}
if (RecurrentBlur && ResolutionScale == ResolutionScale.HalfResolution)
RecurrentBlur = false;
if (injectionPoint == CustomPassInjectionPoint.BeforePreRefraction && HExtensions.IsSSGIEnabled(ctx.hdCamera))
_overwriteSSGI = true;
else
_overwriteSSGI = false;
Vector4 prevScale = HMath.ComputeViewportScaleAndLimit(ctx.hdCamera.historyRTHandleProperties.previousViewportSize, ctx.hdCamera.historyRTHandleProperties.previousRenderTargetSize);
ReallocTextures(new Vector2(ScreenResX, ScreenResY));
RTHandle CurrentCameraColor_Buffer = _overwriteSSGI ? ctx.hdCamera.GetPreviousFrameRT((int)HDCameraFrameHistoryType.ColorBufferMipChain): LoopOutput;
using (new ProfilingScope(cmdList, renderDepthSampler))
{
if (ThicknessMode == ThicknessMode.Accurate)
{
CoreUtils.SetRenderTarget(cmdList, Depth_Back_Render, ClearFlag.All);
CustomPassUtils.DrawRenderers(ctx, ThicknessLayer, RenderQueueType.AllOpaque, CustomVFmaterial, 0,
overrideRenderState: new RenderStateBlock(RenderStateMask.Depth){depthState = new DepthState(true, CompareFunction.LessEqual)});
}
CoreUtils.SetRenderTarget(cmdList, MaskedDepth, ClearFlag.All);
CustomPassUtils.DrawRenderers(ctx, ~HTraceLayer, RenderQueueType.AllOpaque, CustomVFmaterial, 1,
overrideRenderState: new RenderStateBlock(RenderStateMask.Depth){ depthState = new DepthState(true, CompareFunction.LessEqual) });
}
#region KEYWORDS REGION ---------------------->
if (ThicknessMode != ThicknessMode.Disabled)
HRenderGI.EnableKeyword("THICKNESS_AWARE");
else
HRenderGI.DisableKeyword("THICKNESS_AWARE");
if (StochasticThickness == true)
HRenderGI.EnableKeyword("STOCHASTIC_THICKNESS");
else
HRenderGI.DisableKeyword("STOCHASTIC_THICKNESS");
if (EnableFallback == true)
{
if (FallbackMode == FallbackMode.ReflectionProbes)
HRenderGI.EnableKeyword("REFLECTION_PROBE_FALLBACK");
else
HRenderGI.DisableKeyword("REFLECTION_PROBE_FALLBACK");
if (FallbackMode == FallbackMode.CustomProbe)
HRenderGI.EnableKeyword("CUSTOM_PROBE_FALLBACK");
else
HRenderGI.DisableKeyword("CUSTOM_PROBE_FALLBACK");
}
else
{
HRenderGI.DisableKeyword("REFLECTION_PROBE_FALLBACK");
HRenderGI.DisableKeyword("CUSTOM_PROBE_FALLBACK");
}
if (ResolutionScale == ResolutionScale.Checkerboard)
{
HRenderGI.EnableKeyword("ADAPTIVE_CHECKERBOARD");
HDenoiseTemporal.EnableKeyword("ADAPTIVE_CHECKERBOARD");
HDenoiseSpatial.EnableKeyword("ADAPTIVE_CHECKERBOARD");
}
else
{
HRenderGI.DisableKeyword("ADAPTIVE_CHECKERBOARD");
HDenoiseTemporal.DisableKeyword("ADAPTIVE_CHECKERBOARD");
HDenoiseSpatial.DisableKeyword("ADAPTIVE_CHECKERBOARD");
}
if (ResolutionScale == ResolutionScale.HalfResolution)
{
HRenderGI.EnableKeyword("HALF_RESOLUTION");  
HDenoiseTemporal.EnableKeyword("HALF_RESOLUTION"); 
HDenoiseSpatial.EnableKeyword("HALF_RESOLUTION");
HDenoiseCompose.EnableKeyword("HALF_RESOLUTION");
}
else
{
HRenderGI.DisableKeyword("HALF_RESOLUTION");
HDenoiseTemporal.DisableKeyword("HALF_RESOLUTION");
HDenoiseSpatial.DisableKeyword("HALF_RESOLUTION");
HDenoiseCompose.DisableKeyword("HALF_RESOLUTION");
}
if (EnableDenoising == true)
{
HRenderGI.EnableKeyword("DENOISE");
HDenoiseCompose.EnableKeyword("DENOISE");
}
else
{
HRenderGI.DisableKeyword("DENOISE");
HDenoiseCompose.DisableKeyword("DENOISE");
}
if (HistoryFilter == true)
HDenoiseCompose.EnableKeyword("HISTORY_FILTER");
else
HDenoiseCompose.DisableKeyword("HISTORY_FILTER");
if (EnableOcclusion == true)
{
if (NormalsMode == NormalsMode.BentNormals)
HRenderGI.EnableKeyword("BENT_NORMALS");
else
HRenderGI.DisableKeyword("BENT_NORMALS");
if (NormalsMode == NormalsMode.BentCones)
HRenderGI.EnableKeyword("BENT_CONES");
else
HRenderGI.DisableKeyword("BENT_CONES");
if (NormalsMode == NormalsMode.BentNormals | NormalsMode == NormalsMode.BentCones)
HDenoiseCompose.EnableKeyword("BENT_DATA");
else
HDenoiseCompose.DisableKeyword("BENT_DATA");
HDenoiseCompose.EnableKeyword("ENABLE_OCCLUSION");
}
else
{
HRenderGI.DisableKeyword("BENT_NORMALS");
HRenderGI.DisableKeyword("BENT_CONES");
HDenoiseCompose.DisableKeyword("BENT_DATA");
HDenoiseCompose.DisableKeyword("ENABLE_OCCLUSION");
}
if (_overwriteSSGI == false)
HMipChain.EnableKeyword("VIEWPORT_SCALE");
else
HMipChain.DisableKeyword("VIEWPORT_SCALE");
if (DenoiseOcclusion == true)
HDenoiseSpatial.EnableKeyword("DENOISE_OCCLUSION");
else
HDenoiseSpatial.DisableKeyword("DENOISE_OCCLUSION");
#if UNITY_2022_2_OR_NEWER
HRenderGI.EnableKeyword("PROBE_TRACE_ATLAS_ON");
#else
HRenderGI.DisableKeyword("PROBE_TRACE_ATLAS_ON");
#endif
CoreUtils.SetKeyword(FinalOutputMaterial, "OVERWRITE_SSGI", _overwriteSSGI);
CoreUtils.SetKeyword(FinalOutputMaterial, "HALF_RESOLUTION", ResolutionScale == ResolutionScale.HalfResolution);
CoreUtils.SetKeyword(FinalOutputMaterial, "DENOISE", EnableDenoising);
CoreUtils.SetKeyword(FinalOutputMaterial, "DEBUG_GLOBAL_ILLUMINATION", DebugMode == DebugMode.GlobalIllumination  & injectionPoint != CustomPassInjectionPoint.BeforePreRefraction);
CoreUtils.SetKeyword(FinalOutputMaterial, "DEBUG_AMBIENT_OCCLUSION",   DebugMode == DebugMode.AmbientOcclusion    & injectionPoint != CustomPassInjectionPoint.BeforePreRefraction);
CoreUtils.SetKeyword(FinalOutputMaterial, "DEBUG_SPECULAR_OCCLUSION",  DebugMode == DebugMode.SpecularOcclusion   & injectionPoint != CustomPassInjectionPoint.BeforePreRefraction);
CoreUtils.SetKeyword(FinalOutputMaterial, "DEBUG_BENT_NORMALS",        DebugMode == DebugMode.Normals             & injectionPoint != CustomPassInjectionPoint.BeforePreRefraction);
#endregion
Vector2Int runningRes = new Vector2Int(ScreenResX, ScreenResY);
int computeResX_8 = (runningRes.x + 8 - 1) / 8;
int computeResY_8 = (runningRes.y + 8 - 1) / 8;
int computeResX_16 = (runningRes.x + 16 - 1) / 16;
int computeResY_16 = (runningRes.y + 16 - 1) / 16;
int downscale = ResolutionScale == ResolutionScale.HalfResolution ? 2 : 1;
bool runningViewport = hdCamera.cameraType == CameraType.SceneView;
using (new ProfilingScope(cmdList, generateMipSampler))
{
int Mip_Diffuse_Kernel = HMipChain.FindKernel("Mip_Color");
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_Input_MIP0", Color_Buffer, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_Input_MIP1", Color_Buffer, 1);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_Input_MIP2", Color_Buffer, 2);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_Input_MIP3", Color_Buffer, 3);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_Input_MIP4", Color_Buffer, 4);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_CameraMotionVectorsTexture", ctx.cameraMotionVectorsBuffer); 
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Previous_Depth", Depth_Front_Buffer);
cmdList.SetComputeTextureParam(HMipChain, Mip_Diffuse_Kernel, "_Color_Buffer_History", CurrentCameraColor_Buffer);
cmdList.SetComputeVectorParam(HMipChain, "_PrevScale", prevScale);
cmdList.DispatchCompute(HMipChain, Mip_Diffuse_Kernel, computeResX_16, computeResY_16, 1);
int Mip_Normal_Kernel = HMipChain.FindKernel("Mip_Normal");
cmdList.SetComputeTextureParam(HMipChain, Mip_Normal_Kernel, "_Normal_Buffer_MIP0", Normal_Buffer, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_Normal_Kernel, "_Normal_Buffer_MIP1", Normal_Buffer, 1);
cmdList.SetComputeTextureParam(HMipChain, Mip_Normal_Kernel, "_Normal_Buffer_MIP2", Normal_Buffer, 2);
cmdList.SetComputeTextureParam(HMipChain, Mip_Normal_Kernel, "_Normal_Buffer_MIP3", Normal_Buffer, 3);
cmdList.SetComputeTextureParam(HMipChain, Mip_Normal_Kernel, "_Normal_Buffer_MIP4", Normal_Buffer, 4);
cmdList.DispatchCompute(HMipChain, Mip_Normal_Kernel, computeResX_16, computeResY_16, 1); 
int Mip_Front_Depth_Kernel = HMipChain.FindKernel("Mip_Front_Depth");
cmdList.SetComputeTextureParam(HMipChain, Mip_Front_Depth_Kernel, "_Front_Depth_Buffer_MIP0", Depth_Front_Buffer, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_Front_Depth_Kernel, "_Front_Depth_Buffer_MIP1", Depth_Front_Buffer, 1);
cmdList.SetComputeTextureParam(HMipChain, Mip_Front_Depth_Kernel, "_Front_Depth_Buffer_MIP2", Depth_Front_Buffer, 2);
cmdList.SetComputeTextureParam(HMipChain, Mip_Front_Depth_Kernel, "_Front_Depth_Buffer_MIP3", Depth_Front_Buffer, 3);
cmdList.SetComputeTextureParam(HMipChain, Mip_Front_Depth_Kernel, "_Front_Depth_Buffer_MIP4", Depth_Front_Buffer, 4);
cmdList.DispatchCompute(HMipChain, Mip_Front_Depth_Kernel, computeResX_16, computeResY_16, 1);
if (ThicknessMode == ThicknessMode.Accurate)
{
int Filter_Depth_Kernel = HMipChain.FindKernel("Filter_Depth");
cmdList.SetComputeTextureParam(HMipChain, Filter_Depth_Kernel, "_Depth_Back_Buffer",  Depth_Back_Render, 0);
cmdList.SetComputeTextureParam(HMipChain, Filter_Depth_Kernel, "_Filtered_Depth_MIP0", Depth_Back_Filter, 0);
cmdList.SetComputeTextureParam(HMipChain, Filter_Depth_Kernel, "_Filtered_Depth_MIP1", Depth_Back_Filter, 1);
cmdList.SetComputeTextureParam(HMipChain, Filter_Depth_Kernel, "_Filtered_Depth_MIP2", Depth_Back_Filter, 2);
cmdList.SetComputeIntParam(HMipChain, "_RunningInViewport", runningViewport ? 1 : 0);
cmdList.DispatchCompute(HMipChain, Filter_Depth_Kernel, computeResX_8, computeResY_8, 1); 
int Mip_Back_Depth_Kernel = HMipChain.FindKernel("Mip_Back_Depth");
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Source_Depth", Depth_Back_Filter, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Back_Depth_Buffer_MIP0", Depth_Back_Buffer, 0); 
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Back_Depth_Buffer_MIP1", Depth_Back_Buffer, 1);
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Back_Depth_Buffer_MIP2", Depth_Back_Buffer, 2);
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Back_Depth_Buffer_MIP3", Depth_Back_Buffer, 3);
cmdList.SetComputeTextureParam(HMipChain, Mip_Back_Depth_Kernel, "_Back_Depth_Buffer_MIP4", Depth_Back_Buffer, 4);
cmdList.DispatchCompute(HMipChain, Mip_Back_Depth_Kernel, computeResX_16, computeResY_16, 1);
}
}
using (new ProfilingScope(cmdList, renderGISampler))
{
int computeResX_GI = (runningRes.x / downscale + 8 - 1) / 8;
int computeResY_GI = (runningRes.y / downscale + 8 - 1) / 8;
int render_GI_Kernel = HRenderGI.FindKernel("Render_GI");
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_Depth_Front", Depth_Front_Buffer );
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_Depth_Back", ThicknessMode == ThicknessMode.Accurate ? Depth_Back_Buffer : Depth_Front_Buffer);
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_Color_Buffer_Input", Color_Buffer);
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_Normal_Buffer", Normal_Buffer);
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_MaskedDepth", MaskedDepth);
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_GI_Output", GI_Output);
cmdList.SetComputeFloatParam(HRenderGI, "_Distribution_Power", DistributionPower);
cmdList.SetComputeFloatParam(HRenderGI, "_FallbackIntensity", FallbackIntensity);
cmdList.SetComputeFloatParam(HRenderGI, "_Camera_FOV", hdCamera.fieldOfView);
cmdList.SetComputeFloatParam(HRenderGI, "_MainIntensity", MainIntensity);
cmdList.SetComputeFloatParam(HRenderGI, "_FakeIntensity", FakeIntensity);
cmdList.SetComputeFloatParam(HRenderGI, "_AO_Radius", AORadius);
cmdList.SetComputeVectorParam(HRenderGI, "_TraceThickness", ThicknessMode == ThicknessMode.Accurate ? new Vector2(1, 0) : HMath.ThicknessBias(_thickness, hdCamera));
cmdList.SetComputeIntParam(HRenderGI, "_RadiusScale", FullPrecision == true ? 1 : 2);
cmdList.SetComputeIntParam(HRenderGI, "_SliceCount", SliceCount); 
cmdList.SetComputeIntParam(HRenderGI, "_StepCount", StepCount); 
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_CustomProbeColor", DefaultBlackCube);
if (FallbackMode == FallbackMode.CustomProbe && CustomProbe != null && CustomProbe.realtimeTextureRTH != null)
{
cmdList.SetComputeTextureParam(HRenderGI, render_GI_Kernel, "_CustomProbeColor", CustomProbe.realtimeTextureRTH);
cmdList.SetComputeFloatParam(HRenderGI, "_CustomProbeCompression", CustomProbe.rangeCompressionFactor);
cmdList.SetComputeFloatParam(HRenderGI, "_CustomProbeMultiplier", CustomProbe.multiplier);
}
cmdList.SetComputeIntParam(HRenderGI, "_Temporal_Enabled", EnableDenoising == true ? 1 : 0);
cmdList.DispatchCompute(HRenderGI, render_GI_Kernel, computeResX_GI, computeResY_GI, 1);
}
void SpatialDenoiser(RTHandle Input, RTHandle Output, bool secondPass)
{
int Spatial_Denoiser_Kernel = HDenoiseSpatial.FindKernel("Spatial_Denoise_1");
if (secondPass == true)
Spatial_Denoiser_Kernel = HDenoiseSpatial.FindKernel("Spatial_Denoise_2");
int spatialDownscale = secondPass == true ? 1 : downscale;
int computeResX_Spatial = (runningRes.x / spatialDownscale + 8 - 1) / 8;
int computeResY_Spatial = (runningRes.y / spatialDownscale + 8 - 1) / 8;
cmdList.SetComputeBufferParam(HDenoiseSpatial, Spatial_Denoiser_Kernel,"_PointDistribution", pointDistribution);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Spatial_Cache_Weights_RW", Spatial_Cache_Weights);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Spatial_Cache_UVs_RW", Spatial_Cache_UVs);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Spatial_Cache_Weights", Spatial_Cache_Weights);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Spatial_Cache_UVs", Spatial_Cache_UVs);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Input_LOD", Denoiser_History_Fix);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_MaskedDepth", MaskedDepth);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Input", Input);
cmdList.SetComputeTextureParam(HDenoiseSpatial, Spatial_Denoiser_Kernel, "_Output", Output);
cmdList.SetComputeFloatParam(HDenoiseSpatial, "_DenoiserFilterRadius", DenoiserRadius);
cmdList.SetComputeFloatParam(HDenoiseSpatial, "_DetailPreservation", DetailPreservation);
cmdList.SetComputeFloatParam(HDenoiseSpatial, "_AccumulationSpeed", AccumulationSpeed);
cmdList.SetComputeIntParam(HDenoiseSpatial, "_SecondPass", secondPass == true ? 1 : 0);
cmdList.SetComputeIntParam(HDenoiseSpatial, "_DenoiserSampleCount", DenoiserSampleCount);
cmdList.SetComputeIntParam(HDenoiseSpatial, "_RunningInViewport", runningViewport ? 1 : 0);
cmdList.DispatchCompute(HDenoiseSpatial, Spatial_Denoiser_Kernel, computeResX_Spatial, computeResY_Spatial, 1);
}
void TemporalDenoiser(RTHandle Input, RTHandle Output, RTHandle History, bool SecondPass)
{
int Temporal_Denoiser_Kernel = HDenoiseTemporal.FindKernel("Temporal_Denoise_1");
if (SecondPass == true)
Temporal_Denoiser_Kernel = HDenoiseTemporal.FindKernel("Temporal_Denoise_2");
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Temporal_Cache_Weights_RW", Temporal_Cache_Weights);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Temporal_Cache_Weights", Temporal_Cache_Weights);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Normal_Buffer_History", Normal_Buffer_History);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Depth_Buffer_History", Depth_Buffer_History);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_MaskedDepth", MaskedDepth);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Accumulation_Buffer", History);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Input", Input);
cmdList.SetComputeTextureParam(HDenoiseTemporal, Temporal_Denoiser_Kernel, "_Output", Output);
cmdList.SetComputeIntParam(HDenoiseTemporal, "_SecondPass", SecondPass == true ? 1 : 0);
cmdList.DispatchCompute(HDenoiseTemporal, Temporal_Denoiser_Kernel, computeResX_8, computeResY_8, 1);
}
void MipMapGI(RTHandle Input, RTHandle Output)
{
int Mip_GI_Kernel = HMipChain.FindKernel("Mip_GI");
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_Input", Input, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_MIP0", Output, 0);
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_MIP1", Output, 1);
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_MIP2", Output, 2);
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_MIP3", Output, 3);
cmdList.SetComputeTextureParam(HMipChain, Mip_GI_Kernel, "_GI_Buffer_MIP4", Output, 4);
cmdList.DispatchCompute(HMipChain, Mip_GI_Kernel, computeResX_16, computeResY_16, 1); 
}
using (new ProfilingScope(cmdList, denoiseSampler))
{
void Denoise(RTHandle Input, RTHandle Output)
{
int Point_Distribution_Kernel = HDenoiseSpatial.FindKernel("Point_Distribution");
cmdList.SetComputeBufferParam(HDenoiseSpatial, Point_Distribution_Kernel, "_PointDistributionRW",
pointDistribution);
cmdList.DispatchCompute(HDenoiseSpatial, Point_Distribution_Kernel, 1, 1, 1);
RTHandle Output_Final = null;
switch (DenoiserIntensity)
{
default:
HDenoiseSpatial.DisableKeyword("CACHE");
HDenoiseCompose.DisableKeyword("SPATIAL_2X");
HDenoiseCompose.DisableKeyword("TEMPORAL_2X");
TemporalDenoiser(Input, Temporal_Output_1, Temporal_History_1, false);
MipMapGI(Temporal_Output_1, Denoiser_History_Fix);
SpatialDenoiser(Temporal_Output_1, Spatial_Output_1, false);
Output_Final = Spatial_Output_1;
break;
case DenoiserIntensity.Medium:
HDenoiseSpatial.DisableKeyword("CACHE");
HDenoiseCompose.DisableKeyword("SPATIAL_2X");
HDenoiseCompose.EnableKeyword("TEMPORAL_2X");
TemporalDenoiser(Input, Temporal_Output_1, Temporal_History_1, false);
MipMapGI(Temporal_Output_1, Denoiser_History_Fix);
SpatialDenoiser(Temporal_Output_1, Spatial_Output_1, false);
TemporalDenoiser(Spatial_Output_1, Temporal_Output_2, Temporal_History_2, true);
Output_Final = Temporal_Output_2;
break;
case DenoiserIntensity.High:
HDenoiseSpatial.EnableKeyword("CACHE");
HDenoiseCompose.EnableKeyword("SPATIAL_2X");
HDenoiseCompose.EnableKeyword("TEMPORAL_2X");
TemporalDenoiser(Input, Temporal_Output_1, Temporal_History_1, false);
MipMapGI(Temporal_Output_1, Denoiser_History_Fix);
SpatialDenoiser(Temporal_Output_1, Spatial_Output_1, false);
TemporalDenoiser(Spatial_Output_1, Temporal_Output_2, Temporal_History_2, true);
SpatialDenoiser(Temporal_Output_2, Spatial_Output_2, true);
Output_Final = Spatial_Output_2;
break;
}
int denoisingComposeKernel = HDenoiseCompose.FindKernel("Denoising_Compose");
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_Depth",
Depth_Buffer_History);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_Normal",
Normal_Buffer_History);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_Final_LOD",
Denoiser_History_Fix);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Input_History_First",
RecurrentBlur == true ? Spatial_Output_1 : Temporal_Output_1);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_History_First",
Temporal_History_1);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Input_History_Second",
Temporal_Output_2);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_History_Second",
Temporal_History_2);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Spatial_Output_First",
Spatial_Output_1);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Specular_Occlusion",
HTrace_Output_SO);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_MaskedDepth",
MaskedDepth);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_Final_LOD",
Denoiser_History_Fix);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Output_Final",
Output_Final);
cmdList.SetComputeTextureParam(HDenoiseCompose, denoisingComposeKernel, "_Denoise_Output", Output);
cmdList.SetComputeFloatParam(HDenoiseCompose, "_AO_Power", IntensityAO);
cmdList.SetComputeFloatParam(HDenoiseCompose, "_SO_Intensity", IntensitySO);
cmdList.DispatchCompute(HDenoiseCompose, denoisingComposeKernel, computeResX_8, computeResY_8, 1);
}
if (EnableDenoising == true)
{
Denoise(GI_Output, Denoiser_Output);
}
}
ctx.propertyBlock.SetTexture( "_Color_Buffer_History", CurrentCameraColor_Buffer);
ctx.propertyBlock.SetTexture("_GI_Output", EnableDenoising == true ? Denoiser_Output: GI_Output);
ctx.propertyBlock.SetTexture("_MaskedDepth", MaskedDepth);
using (new ProfilingScope(cmdList, outputSampler))
{
if (injectionPoint == CustomPassInjectionPoint.BeforePreRefraction)
CoreUtils.DrawFullScreen(cmdList, FinalOutputMaterial, HTrace_Output_GI, shaderPassId: 0,
properties: ctx.propertyBlock);
else
{
RenderTargetIdentifier[] Multiple_Output;
Multiple_Output = new RenderTargetIdentifier[2];
Multiple_Output[0] = ctx.cameraColorBuffer;
Multiple_Output[1] = LoopOutput;
CoreUtils.DrawFullScreen(cmdList, FinalOutputMaterial, Multiple_Output, shaderPassId: 0,
properties: ctx.propertyBlock);
}
}
cmdList.SetGlobalTexture("_GIBuffer", HTrace_Output_GI);
cmdList.SetGlobalTexture("_SpecularOcclusionBuffer", HTrace_Output_SO);
}
protected override void Cleanup()
{
CoreUtils.SafeRelease(pointDistribution);
AllocateLoopBuffers(true);
AllocateMainBuffers(true);
AllocateMaskBuffers(true);
AllocateBackfaceDepthBuffers(true);
AllocateFullIntegrationBuffers(true);
AllocateMainDenoiserBuffers(true);
AllocateLowDenoiserBuffers(true);
AllocateMediumDenoiserBuffers(true);
AllocateHighDenoiserBuffers(true);
AllocateHistoryFixBuffer(true);
_actualResolution = new Vector2(0, 0);
}
}
}
