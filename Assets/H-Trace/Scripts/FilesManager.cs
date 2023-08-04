using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
using UnityEngine;

namespace HTrace
{
	//TESTED ON:
	
	//Needed to test:
	//Unity 2023.1+			HDRP 15.0
	//Unity 2022.2+			HDRP 14.0
	//Unity 2022.1.10f1		HDRP 13.1.8 - base project
	//Unity 2022.1.0a8+		HDRP 13.0
	//Unity 2021.2+			HDRP 12.1
	//Unity 2021.2.0b6+		HDRP 12.0
	
	//GetPreviouseFrameRT - problems
	//Unity 2021.1.9f1+		HDRP 11.0
	//Unity 2020.3.18f1+	HDRP 10.10
	//Unity 2020.3.18f1+	HDRP 10.9
	//Unity 2020.3.18f1+	HDRP 10.8
	//Unity 2020.3.18f1+	HDRP 10.7
	//Unity 2020.3.13f1+	HDRP 10.6
	//Unity 2020.3.4f1+		HDRP 10.5
	//Unity 2020.3.0f1+		HDRP 10.4
	//Unity 2020.2.3f1+		HDRP 10.3
	//Unity 2020.2.0b12+	HDRP 10.2
	//Unity 2020.2.0b8+		HDRP 10.1
	//Unity 2020.2.0a20+	HDRP 10.0

#if UNITY_EDITOR
	
	
	// internal class PopupExample : PopupWindowContent
	// {
	// 	bool toggle1 = true;
	// 	bool toggle2 = true;
	// 	bool toggle3 = true;
	//
	// 	public override Vector2 GetWindowSize()
	// 	{
	// 		return new Vector2(200, 150);
	// 	}
	//
	// 	public override void OnGUI(Rect rect)
	// 	{
	// 		GUILayout.Label("Popup Options Example", EditorStyles.boldLabel);
	// 		toggle1 = EditorGUILayout.Toggle("Toggle 1", toggle1);
	// 		toggle2 = EditorGUILayout.Toggle("Toggle 2", toggle2);
	// 		toggle3 = EditorGUILayout.Toggle("Toggle 3", toggle3);
	// 	}
	//
	// 	public override void OnOpen()
	// 	{
	// 		Debug.Log("Popup opened: " + this);
	// 	}
	//
	// 	public override void OnClose()
	// 	{
	// 		Debug.Log("Popup closed: " + this);
	// 	}
	// }
	
	
	public class FilesManager : EditorWindow
	{
		private static string DEFERRED_COMPUTE_PATH = Path.Combine("Runtime","Lighting","LightLoop","Deferred.compute"); 
		private static string SSGI_COMPUTE_PATH = Path.Combine("Runtime","Lighting","ScreenSpaceLighting","ScreenSpaceGlobalIllumination.compute");
		private static string RENDER_GRAPH_PATH = Path.Combine("Runtime","RenderPipeline","HDRenderPipeline.RenderGraph.cs");

		[MenuItem("H-Trace/Patch HDRP Package")]
		private static void PatchHDRPPackage()
		{
			var readyToPatch = EditorUtility.DisplayDialog("Warning", "HDRP package will be moved to the local directory and patched. The editor will have to be restarted. It's recommended to back up your files before this operation. Refer to the documentation for additional information. Continue?", "Patch", "Abort");
			if(readyToPatch == false)
				return;
			// EditorWindow window = EditorWindow.CreateInstance<FilesManager>();
			// window.Show();
			// return;
			MoveHDRPFolder();
			
			var hdrpPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Packages"))
				.FirstOrDefault(name => name.Contains("high-definition"));

			if (string.IsNullOrEmpty(hdrpPath))
			{
				Debug.LogWarning("There is no custom HDRP package in Assets/Packages");
				return;
			}

			var hdrpVersion = HDRPVersion();
			
			DeferredCompute(hdrpPath, hdrpVersion: hdrpVersion);
			SSGICompute(hdrpPath, hdrpVersion: hdrpVersion);
			RenderGraph(hdrpPath, hdrpVersion: hdrpVersion);

			Debug.Log("Patched successfully!");
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			
			var readyToQuit = EditorUtility.DisplayDialog("Warning", "Please, restart the editor to apply the patch.", "Ok");
			// if(readyToQuit)
			// 	Application.Quit();
		}

		[MenuItem("H-Trace/Restore HDRP Package")]
		private static void RestoreHDRPPackage()
		{
			var readyToRestore = EditorUtility.DisplayDialog("Warning", "HDRP package will be restored. Continue?", "Restore", "Abort");
			if(readyToRestore == false)
				return;
			
			var hdrpPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Packages"))
				.FirstOrDefault(name => name.Contains("high-definition"));

			if (string.IsNullOrEmpty(hdrpPath))
			{
				Debug.LogWarning("You don't have custom HDRP package in Assets/Packages");
				return;
			}

			DeferredCompute(hdrpPath, true);
			SSGICompute(hdrpPath, true);
			RenderGraph(hdrpPath, true);
			
			Debug.Log("HDRP files restored.");
		}
		
		// Rect buttonRect;
		private void OnGUI()
		{
			// {
			// 	GUILayout.Label("Editor window with Popup example", EditorStyles.boldLabel);
			// 	if (GUILayout.Button("Popup Options", GUILayout.Width(200)))
			// 	{
			// 		PopupWindow.Show(buttonRect, new PopupExample());
			// 	}
			// 	if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();
			// }
		}

		#region Move HDRP

		private static bool MoveHDRPFolder()
		{
			var hdrpPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Library", "PackageCache"))
				.FirstOrDefault(name => name.Contains("com.unity.render-pipelines.high-definition@"));

			if (string.IsNullOrEmpty(hdrpPath))
			{
				Debug.LogWarning($"HDRP package is not installed.");
				return false;
			}
			
			var hdrpPathCustom = Path.Combine(Directory.GetCurrentDirectory(), "Packages");
			
			var nameOfHDRP = hdrpPath.Substring(hdrpPath.IndexOf("com.unity.render-pipelines.high-definition@"));
			hdrpPathCustom = Path.Combine(hdrpPathCustom, nameOfHDRP);
			
			Directory.CreateDirectory(hdrpPathCustom);

			CopyFilesRecursively(hdrpPath, hdrpPathCustom);
			DeleteFullDirectory(hdrpPath);
			return true;
		}
		
		private static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}

		private static bool DeleteFullDirectory(string dirPath)
		{
			System.IO.DirectoryInfo di = new DirectoryInfo(dirPath);

			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete(); 
			}
			foreach (DirectoryInfo dir in di.GetDirectories())
			{
				dir.Delete(true); 
			}
			
			Directory.Delete(dirPath);

			return true;
		}
		
		#endregion

		#region Files to change

		private static bool DeferredCompute(string hdrpPath, bool revert = false, int hdrpVersion = 0)
		{
			// if (CheckHTraceWords(readAllLines))
			// {
			// 	Debug.LogWarning(
			// 		"File contains HTrace changes.\nIf you have compile errors in ScreenSpaceGlobalIllumination.compute" +
			// 		" or HTrace doesn't work, try to restore this file on this path: " +
			// 		"YOURPROJECT\\Packages\\com.unity.render-pipelines.high-definition@x.x.x\\Runtime\\Lighting\\LightLoop\\Deferred.compute" +
			// 		"\nYou can take origin file from https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/Deferred.compute");
			// 	return false;
			// }
			
			// We start with 13 HDRP version as our default
			string[] pattern1 =
			{
				"#pragma multi_compile _ SHADOWS_SHADOWMASK",
				"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
				"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
				"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH",
				"",
				"#ifdef DEBUG_DISPLAY",
				"				// Don't care about this warning in debug",
				"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
				"#endif",
			};

			
			string[] newpattern1 =
			{
				"#pragma multi_compile _ SHADOWS_SHADOWMASK",
				"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
				"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
				"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH",
				"#pragma multi_compile _ HTRACE_BREAK//HTRACE",
				"",
				"#ifdef DEBUG_DISPLAY",
				"				// Don't care about this warning in debug",
				"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
				"#endif",
			};
			
			
			if (hdrpVersion == 14)
			{
				pattern1 = new[]
				{
					"#pragma multi_compile _ SHADOWS_SHADOWMASK",
					"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
					"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
					"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH",
					"#pragma multi_compile AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH",
					"",
					"#ifdef DEBUG_DISPLAY",
					"    // Don't care about this warning in debug",
					"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
					"#endif",
				};
				newpattern1 = new[]
				{
					"#pragma multi_compile _ SHADOWS_SHADOWMASK",
					"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
					"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
					"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH",
					"#pragma multi_compile AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH",
					"#pragma multi_compile _ HTRACE_BREAK//HTRACE",
					"",
					"#ifdef DEBUG_DISPLAY",
					"    // Don't care about this warning in debug",
					"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
					"#endif",
				};
			}

			string[] pattern2 =
			{
				"//-------------------------------------------------------------------------------------",
				"// variable declaration",
				"//-------------------------------------------------------------------------------------",
				"",
				"TEXTURE2D_X_UINT2(_StencilTexture);",
			};
			
			string[] newpattern2 =
			{
				"//-------------------------------------------------------------------------------------",
				"// variable declaration",
				"//-------------------------------------------------------------------------------------",
				"TEXTURE2D_ARRAY(_SpecularOcclusionBuffer);//HTRACE",
				"",
				"TEXTURE2D_X_UINT2(_StencilTexture);",
			};
			
			string[] pattern3 =
			{
				"    // Alias",
				"    float3 diffuseLighting = lightLoopOutput.diffuseLighting;",
				"    float3 specularLighting = lightLoopOutput.specularLighting;",
				"",
				"    diffuseLighting *= GetCurrentExposureMultiplier();",
				"    specularLighting *= GetCurrentExposureMultiplier();",
				"",
				"    if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))",
			};
			
			string[] newpattern3 =
			{
				"    // Alias",
				"    float3 diffuseLighting = lightLoopOutput.diffuseLighting;",
				"    float3 specularLighting = lightLoopOutput.specularLighting;",
				"",
				"    float SpecularOcclusion = 1;//HTRACE",
				"    #ifdef HTRACE_BREAK//HTRACE",
				"        SpecularOcclusion = LOAD_TEXTURE2D_ARRAY(_SpecularOcclusionBuffer, pixelCoord.xy, 0).x;//HTRACE",
				"    #endif//HTRACE",
				"//H-Trace",
				"    diffuseLighting *= GetCurrentExposureMultiplier();",
				"    specularLighting *= GetCurrentExposureMultiplier();",
				"    specularLighting *= saturate(SpecularOcclusion);//HTRACE",
				"",
				"    if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))",
			};
			
			if (hdrpVersion == 15)
			{
				pattern3 = new[]
				{
					"#pragma multi_compile _ SHADOWS_SHADOWMASK",
					"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
					"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
					"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH",
					"#pragma multi_compile AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH",
					"",
					"#ifdef DEBUG_DISPLAY",
					"    // Don't care about this warning in debug",
					"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
					"#endif",
				};
				newpattern3 = new[]
				{
					"#pragma multi_compile _ SHADOWS_SHADOWMASK",
					"#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON",
					"#pragma multi_compile PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2",
					"#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH",
					"#pragma multi_compile AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH",
					"#pragma multi_compile _ HTRACE_BREAK//HTRACE",
					"",
					"#ifdef DEBUG_DISPLAY",
					"    // Don't care about this warning in debug",
					"#   pragma warning( disable : 4714 ) // sum of temp registers and indexable temp registers times 256 threads exceeds the recommended total 16384.  Performance may be reduced at kernel",
					"#endif",
				};
			}
			
			List<string[]> patterns = new List<string[]>()
			{
				pattern1,
				pattern2,
				pattern3,
			};
			
			List<string[]> newpatterns = new List<string[]>()
			{
				newpattern1,
				newpattern2,
				newpattern3,
			};


			var deferredComputePath = Path.Combine(hdrpPath, DEFERRED_COMPUTE_PATH);

			List<string> resultLines = new List<string>();
			if(revert == false)
				ReplacePatterns(deferredComputePath, patterns, newpatterns, ref resultLines);
			else
				ReplacePatterns(deferredComputePath, newpatterns,patterns, ref resultLines);

			File.WriteAllLines(deferredComputePath, resultLines);
			return true;
		}

		private static bool SSGICompute(string hdrpPath, bool revert = false, int hdrpVersion = 0)
		{
			// if (CheckHTraceWords(ssgiLines))
			// {
			// 	Debug.LogWarning(
			// 		"File contains HTrace changes.\nIf you have compile errors in ScreenSpaceGlobalIllumination.compute" +
			// 		" or HTrace doesn't work, try to restore this file on this path: " +
			// 		"YOURPROJECT\\Packages\\com.unity.render-pipelines.high-definition@x.x.x\\Runtime\\Lighting\\ScreenSpaceLighting\\ScreenSpaceGlobalIllumination.compute" +
			// 		"\nYou can take origin file from https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ScreenSpaceGlobalIllumination.compute");
			// 	return false;
			// }
			
			string[] pattern1 =
			{
				"// deferred opaque always use FPTL",
				"#define USE_FPTL_LIGHTLIST 1",
			};
			
			string[] newpattern1 =
			{
				"// deferred opaque always use FPTL",
				"#define USE_FPTL_LIGHTLIST 1",
				"#pragma multi_compile _ HTRACE_BREAK//HTRACE",
				"",
			};

			string[] pattern2 =
			{
				"// Must be included after the declaration of variables",
				"#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/RayMarching.hlsl\"",
				"",
				"// Output texture that holds the hit point NDC coordinates",
				"RW_TEXTURE2D_X(float2, _IndirectDiffuseHitPointTextureRW);",
				"",
				"[numthreads(INDIRECT_DIFFUSE_TILE_SIZE, INDIRECT_DIFFUSE_TILE_SIZE, 1)]",
				"void TRACE_GLOBAL_ILLUMINATION(uint3 dispatchThreadId : SV_DispatchThreadID, uint2 groupThreadId : SV_GroupThreadID, uint2 groupId : SV_GroupID)",
				"{",
				"    UNITY_XR_ASSIGN_VIEW_INDEX(dispatchThreadId.z);",
				"",
				"    // Compute the pixel position to process",
				"    uint2 currentCoord = dispatchThreadId.xy;",
				"    uint2 inputCoord = dispatchThreadId.xy;",
				"",
				"#if HALF_RES",
			};
			
			string[] newpattern2 =
			{
				"// Must be included after the declaration of variables",
				"#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/RayMarching.hlsl\"",
				"",
				"// Output texture that holds the hit point NDC coordinates",
				"RW_TEXTURE2D_X(float2, _IndirectDiffuseHitPointTextureRW);",
				"",
				"TEXTURE2D_ARRAY(_GIBuffer); // H-Trace",
				"",
				"[numthreads(INDIRECT_DIFFUSE_TILE_SIZE, INDIRECT_DIFFUSE_TILE_SIZE, 1)]",
				"void TRACE_GLOBAL_ILLUMINATION(uint3 dispatchThreadId : SV_DispatchThreadID, uint2 groupThreadId : SV_GroupThreadID, uint2 groupId : SV_GroupID)",
				"{",
				"    UNITY_XR_ASSIGN_VIEW_INDEX(dispatchThreadId.z);",
				"",
				"#ifdef HTRACE_BREAK//HTRACE",
				"    return;//HTRACE",
				"#endif//HTRACE",
				"//HTRACE",
				"    // Compute the pixel position to process",
				"    uint2 currentCoord = dispatchThreadId.xy;",
				"    uint2 inputCoord = dispatchThreadId.xy;",
				"",
				"#if HALF_RES",
			};
			
			string[] pattern3 =
			{
				"[numthreads(INDIRECT_DIFFUSE_TILE_SIZE, INDIRECT_DIFFUSE_TILE_SIZE, 1)]",
				"void REPROJECT_GLOBAL_ILLUMINATION(uint3 dispatchThreadId : SV_DispatchThreadID, uint2 groupThreadId : SV_GroupThreadID, uint2 groupId : SV_GroupID)",
				"{",
				"    UNITY_XR_ASSIGN_VIEW_INDEX(dispatchThreadId.z);",
				"",
				"    // Compute the pixel position to process",
				"    uint2 inputCoord = dispatchThreadId.xy;",
				"    uint2 currentCoord = dispatchThreadId.xy;",
				"#if HALF_RES",
				"    // Compute the full resolution pixel for the inputs that do not have a pyramid",
				"    inputCoord = inputCoord * 2;",
				"#endif",
				"",
			};
			
			string[] newpattern3 =
			{
				"[numthreads(INDIRECT_DIFFUSE_TILE_SIZE, INDIRECT_DIFFUSE_TILE_SIZE, 1)]",
				"void REPROJECT_GLOBAL_ILLUMINATION(uint3 dispatchThreadId : SV_DispatchThreadID, uint2 groupThreadId : SV_GroupThreadID, uint2 groupId : SV_GroupID)",
				"{",
				"    UNITY_XR_ASSIGN_VIEW_INDEX(dispatchThreadId.z);",
				"",
				"    // Compute the pixel position to process",
				"    uint2 inputCoord = dispatchThreadId.xy;",
				"    uint2 currentCoord = dispatchThreadId.xy;",
				"#if HALF_RES",
				"    // Compute the full resolution pixel for the inputs that do not have a pyramid",
				"    inputCoord = inputCoord * 2;",
				"#endif",
				"",
				"#ifdef HTRACE_BREAK//HTRACE",
				"    _IndirectDiffuseTextureRW[COORD_TEXTURE2D_X(currentCoord)] = LOAD_TEXTURE2D_ARRAY(_GIBuffer, inputCoord, 0);//HTRACE",
				"    return;//HTRACE",
				"#endif//HTRACE",
				"//HTRACE",
			};
			
			List<string[]> patterns = new List<string[]>()
			{
				pattern1,
				pattern2,
				pattern3,
			};
			
			List<string[]> newpatterns = new List<string[]>()
			{
				newpattern1,
				newpattern2,
				newpattern3,
			};


			var ssgiPath = Path.Combine(hdrpPath, SSGI_COMPUTE_PATH);

			List<string> resultLines = new List<string>();
			if(revert == false)
				ReplacePatterns(ssgiPath, patterns, newpatterns, ref resultLines);
			else
				ReplacePatterns(ssgiPath, newpatterns,patterns, ref resultLines);

			File.WriteAllLines(ssgiPath, resultLines);
			return true;
		}

		private static bool RenderGraph(string hdrpPath, bool revert = false, int hdrpVersion = 0)
		{
			// if (CheckHTraceWords(renderGraphLines))
			// {
			// 	Debug.LogWarning(
			// 		"File contains HTrace changes.\nIf you have compile errors in HDRenderPipeline.ScreenSpaceGlobalIllumination.cs" +
			// 		" or HTrace doesn't work, try to restore this file on this path: " +
			// 		"YOURPROJECT\\Packages\\com.unity.render-pipelines.high-definition@x.x.x\\Runtime\\RenderPipeline\\HDRenderPipeline.RenderGraph.cs" +
			// 		"\nYou can take origin file from https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/HDRenderPipeline.RenderGraph.cs");
			// 	return false;
			// }
			
			string[] pattern1 =
			{
				"                    RenderShadows(m_RenderGraph, hdCamera, cullingResults, ref shadowResult);",
				"",
				"                    StartXRSinglePass(m_RenderGraph, hdCamera);",
				"",
				"                    // Evaluate the clear coat mask texture based on the lit shader mode",
				"                    var clearCoatMask = hdCamera.frameSettings.litShaderMode == LitShaderMode.Deferred ? prepassOutput.gbuffer.mrt[2] : m_RenderGraph.defaultResources.blackTextureXR;",
			};
			
			string[] newpattern1 =
			{
				"                    RenderShadows(m_RenderGraph, hdCamera, cullingResults, ref shadowResult);",
				"",
				"                    StartXRSinglePass(m_RenderGraph, hdCamera);",
				"",
				"                    bool HTraceExecuted = RenderCustomPass(m_RenderGraph, hdCamera, colorBuffer, prepassOutput, customPassCullingResults, cullingResults, CustomPassInjectionPoint.BeforePreRefraction, aovRequest, aovCustomPassBuffers);//HTRACE",
				"//HTRACE",
				"                    if (hdCamera.IsSSGIEnabled() & HTraceExecuted == true)//HTRACE",
				"                        Shader.EnableKeyword(\"HTRACE_BREAK\");//HTRACE",
				"                    else//HTRACE",
				"                        Shader.DisableKeyword(\"HTRACE_BREAK\");//HTRACE",
				"//H-Trace",
				"                    // Evaluate the clear coat mask texture based on the lit shader mode",
				"                    var clearCoatMask = hdCamera.frameSettings.litShaderMode == LitShaderMode.Deferred ? prepassOutput.gbuffer.mrt[2] : m_RenderGraph.defaultResources.blackTextureXR;",
			};

			string[] pattern2 =
			{
				"            // TODO RENDERGRAPH: Remove this when we properly convert custom passes to full render graph with explicit color buffer reads.",
				"            // To allow users to fetch the current color buffer, we temporarily bind the camera color buffer",
				"            SetGlobalColorForCustomPass(renderGraph, colorBuffer);",
				"            RenderCustomPass(m_RenderGraph, hdCamera, colorBuffer, prepassOutput, customPassCullingResults, cullingResults, CustomPassInjectionPoint.BeforePreRefraction, aovRequest, aovCustomPassBuffers);",
				"            SetGlobalColorForCustomPass(renderGraph, currentColorPyramid);",
			};
			
			string[] newpattern2 =
			{
				"            // TODO RENDERGRAPH: Remove this when we properly convert custom passes to full render graph with explicit color buffer reads.",
				"            // To allow users to fetch the current color buffer, we temporarily bind the camera color buffer",
				"            //SetGlobalColorForCustomPass(renderGraph, colorBuffer);//HTRACEUNCOMMENT",
				"            //RenderCustomPass(m_RenderGraph, hdCamera, colorBuffer, prepassOutput, customPassCullingResults, cullingResults, CustomPassInjectionPoint.BeforePreRefraction, aovRequest, aovCustomPassBuffers);//HTRACEUNCOMMENT",
				"            //SetGlobalColorForCustomPass(renderGraph, currentColorPyramid);//HTRACEUNCOMMENT",
				"",
			};
			
			List<string[]> patterns = new List<string[]>()
			{
				pattern1,
				pattern2,
			};
			
			List<string[]> newpatterns = new List<string[]>()
			{
				newpattern1,
				newpattern2,
			};

			var renderGraphPath = Path.Combine(hdrpPath, RENDER_GRAPH_PATH);

			List<string> resultLines = new List<string>();
			if(revert == false)
				ReplacePatterns(renderGraphPath, patterns, newpatterns, ref resultLines);
			else
				ReplacePatterns(renderGraphPath, newpatterns,patterns, ref resultLines);

			File.WriteAllLines(renderGraphPath, resultLines);
			return true;
		}
		
		#endregion

		#region Private Methods

		private static void ReplacePatterns(string filePath, List<string[]> patterns, List<string[]> newpatterns, ref List<string> resultLines)
		{
			var readAllLines = File.ReadAllLines(filePath);

			int patternIndex = 0;

			for (int i = 0; i < readAllLines.Length; i++)
			{
				var possiblePattern = patterns.Any(pattern => DeleteTabulationAndSpaces(readAllLines[i]) == DeleteTabulationAndSpaces(pattern[0]));
				if (possiblePattern)
				{
					int countLinesPattern = patterns[patternIndex].Length;
					List<string> patternMatch = new List<string>(countLinesPattern);
					for (int j = 0; j < countLinesPattern; j++)
					{
						patternMatch.Add(readAllLines[j + i]);
					}

					if (CheckPattern(patterns[patternIndex], patternMatch.ToArray()))
					{
						resultLines.AddRange(newpatterns[patternIndex]);
						i += patterns[patternIndex].Length - 1;// -1 because our loop has i++
						patternIndex++;
						continue;
					}
				}

				resultLines.Add(readAllLines[i]);
			}
		}

		private static bool CheckPattern(string[] pattern, string[] patternMatch)
		{
			for (int i = 0; i < pattern.Length; i++)
			{
				if (DeleteTabulationAndSpaces(pattern[i]) == DeleteTabulationAndSpaces(patternMatch[i]))
					continue;
				return false;
			}
			return true;
		}

		private static string DeleteTabulationAndSpaces(string input)
		{
			var result = input.Replace("	", "");
			result = result.Replace("\t", "");
			result = result.Replace(" ", "");
			return result;
		}

		private static bool CheckHTraceWords(string[] allLines)
		{
			foreach (var line in allLines)
			{
				if (line.ToUpper().Contains("H-TRACE") || line.ToUpper().Contains("HTRACE"))
				{
					return true;
				}
			}

			return false;
		}

		private static ListRequest Request;
		//[MenuItem("H-Trace/HDRP version")]
		private static int HDRPVersion()
		{
			//Unity friendly
			// Request = Client.List();    // List packages installed for the project
			// EditorApplication.update += Progress;
			// Debug.Log("Waiting...");
			
			//My kek version
			var hdrpPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Packages"))
				.First(name => name.Contains("high-definition"));
			int index = hdrpPath.IndexOf('@');
			string version = hdrpPath.Remove(0, index + 1);
			return Convert.ToInt32(version.Substring(0,2));
		}

		static void Progress()
		{
			string hdrpPackageName = "com.unity.render-pipelines.high-definition";
			
			if (Request.IsCompleted)
			{
				if (Request.Status == StatusCode.Success)
					foreach (var package in Request.Result)
					{
						if (package.name == hdrpPackageName)
						{
							Debug.Log("Package version: " + package.version);
							break;
						}
					}
				else if (Request.Status >= StatusCode.Failure)
					Debug.Log(Request.Error.message);

				EditorApplication.update -= Progress;
				
				//Logic to change files in HDRP package.
			}
		}
		
		#endregion
		
#if DEVELOPER
		
		[MenuItem("H-Trace/Delete comments and empty lines")]
		static void DeleteComments()
		{
			var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
			DeleteCommentsInFolder(Path.Combine(assetsPath, "Resources", "H-Trace"));
			DeleteCommentsInFolder(Path.Combine(assetsPath, "H-Trace"));
			DeleteCommentsInFolder(Path.Combine(assetsPath, "H-Trace", "Headers"));
			DeleteCommentsInFolder(Path.Combine(assetsPath, "H-Trace", "Shaders"));
			DeleteCommentsInFolder(Path.Combine(assetsPath, "H-Trace", "Scripts"));
			Debug.Log("Comments deleted.");
		}

		private static void DeleteCommentsInFolder(string path)
		{
			string regex = @"[a-zA-Z0-9+$/.?();:]";
			string[] filesTypes = {".cs",".shader",".hlsl",".cginc",".compute"};
			var filesPath = Directory.GetFiles(path)
				.Where(name => filesTypes.Any(name.Contains))
				.Where(name => !name.Contains(".meta") || !name.Contains(nameof(FilesManager)));

			foreach (var filePath in filesPath)
			{
				if(filePath.Contains("FilesManager"))
					continue;

				var lines = File.ReadAllLines(filePath);
				List<string> resultLines = new List<string>();

				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].Contains("//"))
					{
						var index = lines[i].IndexOf("//");
						if (index >= 0 && !IsSummary(lines[i]))
						{
							var substring = lines[i].Substring(0, index);
							if (!Regex.IsMatch(substring,regex))
							{
								continue;
							}
						}
					}

					string clearStringTest = lines[i].Replace(" ", "");
					if(clearStringTest.Length == 0)
						continue;

					resultLines.Add(lines[i]);
				}
				File.WriteAllLines(filePath,resultLines);	
			}
		}

		private static bool IsSummary(string line)
		{
			if (line.Contains("///"))
			{
				var index = line.IndexOf("///");
				if (index - 1 < 0 && line[index + 1] != '/')
				{
					return true;
				}
				if (index - 1 > 0 && line[index - 1] != '/' && line[index + 1 + 3] != '/')
				{
					return true;
				}
			}
			return false;
		}
#endif
	}
	
#endif
}
