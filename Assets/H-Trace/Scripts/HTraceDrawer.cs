using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
namespace HTrace
{
#if UNITY_EDITOR
    /// <summary>
    /// HTrace custom pass drawer
    /// </summary>
    [CustomPassDrawer(typeof(HTracePass))]
    public class HTraceDrawer : CustomPassDrawer
    {
        private class Styles
        {
            public static float defaultLineSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            public static float additionalLineSpace = 10f;
            public static float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2;
            public static float checkBoxOffsetWidth = 15f;
            public static float checkBoxWidth = 15f;
            public static float tabOffset = 8f;
            public static GUIContent GeneralSettingsContent = new GUIContent("General Settings");
            public static GUIContent hTraceLayerContent = new GUIContent("H-Trace Layer", "Excludes objects from H-Trace rendering on a per-layer basis");
            public static GUIContent ThicknessModeContent = new GUIContent("Thickness Mode", "Determines the thickness detection method, where:" +
                " 1. Disabled - means infinite thicknes," +
                " 2. Standard - uses a single value for all objects," +
                " 3. Accurate - detects true thickness of every object");
            public static GUIContent ThicknessLayerContent = new GUIContent("Thickness Layer", "Excludes objects from the Accurate thickness detection. Excluded objects will fallback to infinite thickness");
            public static GUIContent ThicknessContent = new GUIContent("Thickness Delta", "Controls the thickness of all objects");
            public static GUIContent MainIntensityContent = new GUIContent("Main Intensity", "Controls the intensity of the main GI output gathered during screen tracing. It's not recommended to raise this value above 1 if you want to keep GI physically correct. You can lower it to 0.5 when the fallback is activated");
            public static GUIContent FakeIntensityContent = new GUIContent("Fake Intensity", "Attempts to add bounced lighting to the places rejected due to the screen-space nature of tracing method. Error-prone, use it carefully");
            public static GUIContent DistributionPowerContent = new GUIContent("Distribution Power", "Distributes samples across the frame. The higher the value - the more samples are placed near the ray origin, producing more accurate indirect shadows");
            public static GUIContent SliceCountContent = new GUIContent("Slice Count", "Controls the number of tracing directions. It's recommended to use more than 1 slice for better AO, SO and Bent Normals / Cones representation");
            public static GUIContent StepCountContent = new GUIContent("Step Count", "Controls the number of samples for each individual direction");
            public static GUIContent ResolutionScaleContent = new GUIContent("Resolution Scale", "Downscales the rendering resolution");
            public static GUIContent FullPrecisionContent = new GUIContent("Full Precision", "Enables better sample placement, especially at low Slice / Sample count");
            public static GUIContent DebugModeContent = new GUIContent("Debug Mode", "Visualizes the debug mode for different rendering components of H-Trace.");
            public static GUIContent OcclusionContent = new GUIContent("Occlusion");
            public static GUIContent NormalsModeContent = new GUIContent("Normals Mode", "Controls which normals will be used for Specular Occlusion and Denoising");
            public static GUIContent IntensityAOContent = new GUIContent("AO Intensity", "Controls the intensity of Ambient Occlusion");
            public static GUIContent IntensitySOContent = new GUIContent("SO Intensity", "Controls the intensity of Specular Occlusion");
            public static GUIContent DenoisingContent = new GUIContent("Denoising");
            public static GUIContent DenoiserIntensityContent = new GUIContent("Denoiser Intensity", "Controls the denoising intensity. Each mode launches its own set of spatial and temporal filters.");
            public static GUIContent DenoiserRadiusContent = new GUIContent("Denoiser Radius", "Controls the spatial filtering radius. Higher values can reduce noise at the cost of blurring small details");
            public static GUIContent DenoiserSampleCountContent = new GUIContent("Sample Count", "Defines the number of samples used during spatial filtering. Higher values help to stabilize and reduce noise at the performance cost");
            public static GUIContent DetailPreservationContent = new GUIContent("Detail Preservation", "Varies the spatial filter radius to preserve small details and contact shadows");
            public static GUIContent AccumulationSpeedContent = new GUIContent("Accumulation", "Widens spatial filter radius for the areas that has low temporal accumulation values. Helps to reduce noise on the screen borders during camera movement");
            public static GUIContent RecurrentBlurContent = new GUIContent("Recurrent Blur", "Enables self-stabilizing recurrent blur which can dramatically reduce noise with no performance impact");
            public static GUIContent DenoiseOcclusionContent = new GUIContent("Denoise Occlusion", "Defines if spatial filtering should be applied to AO, SO and Bent Normals");
            public static GUIContent HistoryFilterContent = new GUIContent("History Filter");
            public static GUIContent FallbackContent = new GUIContent("Fallback");
            public static GUIContent FallbackModeContent = new GUIContent("Fallback Mode", "Determines the fallback mode");
            public static GUIContent CustomProbeContent = new GUIContent("  Custom Probe", "Drag your reflection probe here");
            public static GUIContent FallbackIntensityContent = new GUIContent("Fallback Intensity", "Controls the intensity of the fallback tracing GI output");
            public static GUIContent StochasticThicknessContent = new GUIContent("Stochastic Thickness");
            public static GUIStyle hiddenFoldout = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(),
                fontSize = 12,
                normal = new GUIStyleState()
                {
                    textColor = new Color(0.500f, 0.500f, 0.500f, 1f),
                },
                fontStyle = FontStyle.Bold,
            };
            public static GUIStyle headerFoldout = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(),
                fontSize = 12,
                normal = new GUIStyleState()
                {
                    textColor = new Color(0.903f, 0.903f, 0.903f, 1f),
                },
                fontStyle = FontStyle.Bold,
            };
        }
        private bool _generalSettingsTab = true;
        private bool _occlusionTab = true;
        private bool _denoisingTab = true;
        private bool _fallbackTab = true;
        SerializedProperty HTraceLayer;
        SerializedProperty ThicknessMode;
        SerializedProperty ThicknessLayer;
        SerializedProperty Thickness;
        SerializedProperty MainIntensity;
        SerializedProperty FakeIntensity;
        SerializedProperty DistributionPower;
        SerializedProperty SliceCount;
        SerializedProperty StepCount;
        SerializedProperty ResolutionScale;
        SerializedProperty FullPrecision;
        SerializedProperty DebugMode;
        SerializedProperty EnableOcclusion;
        SerializedProperty NormalsMode;
        SerializedProperty IntensityAO;
        SerializedProperty IntensitySO;
        SerializedProperty EnableDenoising;
        SerializedProperty DenoiserIntensity;
        SerializedProperty DenoiserRadius;
        SerializedProperty DenoiserSampleCount;
        SerializedProperty DetailPreservation;
        SerializedProperty AccumulationSpeed;
        SerializedProperty RecurrentBlur;
        SerializedProperty DenoiseOcclusion;
        SerializedProperty HistoryFilter;
        SerializedProperty EnableFallback;
        SerializedProperty FallbackMode;
        SerializedProperty FallbackIntensity;
        SerializedProperty CustomProbe;
        SerializedProperty InjectionPoint;
        SerializedProperty StochasticThickness;
        bool _enableFallback = true;
        bool _enableOcclusion = true;
        bool _enableDenoising = true;
        private float _windowHeight;
        private float _windowHeightStart = 100;
        protected override PassUIFlag commonPassUIFlags => PassUIFlag.None;
        protected override void Initialize(SerializedProperty customPass)
        {
            PropertiesRelative(customPass);
            _enableFallback = EnableFallback.boolValue;
            _enableOcclusion = EnableOcclusion.boolValue;
            _enableDenoising = EnableDenoising.boolValue; 
        }
        private void PropertiesRelative(SerializedProperty customPass)
        {   
            HTraceLayer = customPass.FindPropertyRelative("HTraceLayer");
            ThicknessMode = customPass.FindPropertyRelative("ThicknessMode"); 
            ThicknessLayer = customPass.FindPropertyRelative("ThicknessLayer");
            Thickness = customPass.FindPropertyRelative("_thickness");
            MainIntensity = customPass.FindPropertyRelative("_mainIntensity");
            FakeIntensity = customPass.FindPropertyRelative("_fakeIntensity");
            DistributionPower = customPass.FindPropertyRelative("_distributionPower");
            SliceCount = customPass.FindPropertyRelative("_sliceCount");
            StepCount = customPass.FindPropertyRelative("_stepCount");
            ResolutionScale = customPass.FindPropertyRelative("ResolutionScale"); 
            FullPrecision = customPass.FindPropertyRelative("FullPrecision");
            DebugMode = customPass.FindPropertyRelative("DebugMode");
            EnableOcclusion = customPass.FindPropertyRelative("_enableOcclusion");
            NormalsMode = customPass.FindPropertyRelative("NormalsMode");
            IntensityAO = customPass.FindPropertyRelative("_intensityAO");
            IntensitySO = customPass.FindPropertyRelative("_intensitySO");
            EnableDenoising = customPass.FindPropertyRelative("_enableDenoising");
            DenoiserIntensity = customPass.FindPropertyRelative("DenoiserIntensity");
            DenoiserRadius = customPass.FindPropertyRelative("_denoiserRadius");
            DenoiserSampleCount = customPass.FindPropertyRelative("_denoiserSampleCount");
            DetailPreservation = customPass.FindPropertyRelative("_detailPreservation");
            AccumulationSpeed = customPass.FindPropertyRelative("_accumulationSpeed");
            RecurrentBlur = customPass.FindPropertyRelative("RecurrentBlur");
            DenoiseOcclusion = customPass.FindPropertyRelative("DenoiseOcclusion");
            HistoryFilter = customPass.FindPropertyRelative("HistoryFilter");
            EnableFallback = customPass.FindPropertyRelative("_enableFallback");
            FallbackMode = customPass.FindPropertyRelative("FallbackMode");
            CustomProbe = customPass.FindPropertyRelative("CustomProbe");
            FallbackIntensity = customPass.FindPropertyRelative("_fallbackIntensity");
            InjectionPoint = customPass.FindPropertyRelative("InjectionPointChecker");
            StochasticThickness = customPass.FindPropertyRelative("StochasticThickness");
        }
        protected override float GetPassHeight(SerializedProperty customPass)
        {
            float height = Styles.defaultLineSpace * 5;
            if (_generalSettingsTab)
            {
                height += Styles.defaultLineSpace * 10;
                height += Styles.additionalLineSpace * 1;
                if (ThicknessMode.enumValueIndex != (int)HTrace.ThicknessMode.Disabled)
                    height += Styles.defaultLineSpace;
                if (InjectionPoint.intValue < 4 && InjectionPoint.intValue > 0)
                {
                    height += Styles.defaultLineSpace;
                    height += Styles.additionalLineSpace * 1;
                }
            }
            if(_occlusionTab && _enableOcclusion)
                height += Styles.defaultLineSpace * 3;
            if(_denoisingTab && _enableDenoising)
                height += Styles.defaultLineSpace * 6;
            if(_denoisingTab && _enableDenoising && DenoiserIntensity.enumValueIndex != 0)
                height += Styles.defaultLineSpace * 1;
            if (_fallbackTab && _enableFallback)
            {
                height += Styles.defaultLineSpace * 2;
                if(FallbackMode.enumValueIndex == 1)
                        height += Styles.defaultLineSpace * 1;
            }
            return height;
        }
        protected override void DoPassGUI(SerializedProperty customPass, Rect rect)
        {
            rect.y += Styles.defaultLineSpace;
            _generalSettingsTab = EditorGUI.BeginFoldoutHeaderGroup(rect, _generalSettingsTab, Styles.GeneralSettingsContent);
            rect.y += Styles.defaultLineSpace;
            rect.x += Styles.tabOffset;
            rect.width -= Styles.tabOffset;
            if (_generalSettingsTab)
            {
                EditorGUI.PropertyField(rect, HTraceLayer, Styles.hTraceLayerContent);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.PropertyField(rect, ThicknessMode, Styles.ThicknessModeContent);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.indentLevel++;
                if (ThicknessMode.enumValueIndex == (int)HTrace.ThicknessMode.Standard)
                {   
                    EditorGUI.Slider(rect, Thickness, 0.0f, 1f, Styles.ThicknessContent); //0.15f
                    rect.y += Styles.defaultLineSpace;
                }
                if (ThicknessMode.enumValueIndex == (int)HTrace.ThicknessMode.Accurate)
                {   
                    EditorGUI.PropertyField(rect, ThicknessLayer, Styles.ThicknessLayerContent);
                    rect.y += Styles.defaultLineSpace;
                }
                EditorGUI.indentLevel--;
                EditorGUI.Slider(rect, MainIntensity, 0.0f, 2.0f, Styles.MainIntensityContent);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.Slider(rect, FakeIntensity, 0.0f, 1.0f, Styles.FakeIntensityContent);
                rect.y += Styles.defaultLineSpace;
                rect.y += Styles.additionalLineSpace;
                EditorGUI.LabelField(rect,"Performance Settings", Styles.headerFoldout);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.Slider(rect, DistributionPower, 1.0f, 4.0f, Styles.DistributionPowerContent);
                rect.y += Styles.defaultLineSpace;
                SliceCount.intValue = EditorGUI.IntSlider(rect, Styles.SliceCountContent,SliceCount.intValue, 1,8);
                rect.y += Styles.defaultLineSpace;
                StepCount.intValue = EditorGUI.IntSlider(rect, Styles.StepCountContent,StepCount.intValue, 1,64);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.PropertyField(rect, ResolutionScale, Styles.ResolutionScaleContent);
                rect.y += Styles.defaultLineSpace;
                EditorGUI.PropertyField(rect, FullPrecision, Styles.FullPrecisionContent);
                rect.y += Styles.defaultLineSpace;
                if (InjectionPoint.intValue < 4 && InjectionPoint.intValue > 0)
                {
                    rect.y += Styles.additionalLineSpace;
                    EditorGUI.PropertyField(rect, DebugMode, Styles.DebugModeContent);
                    rect.y += Styles.defaultLineSpace;
                }
            }
            rect.x -= Styles.tabOffset;
            rect.width += Styles.tabOffset;
            EditorGUI.EndFoldoutHeaderGroup();
            float saveWidth = rect.width;
            rect = new Rect(rect.x - Styles.checkBoxOffsetWidth, rect.y, Styles.checkBoxWidth, rect.height);
            _enableOcclusion = EditorGUI.Toggle(rect, _enableOcclusion);
            rect = new Rect(rect.x + Styles.checkBoxOffsetWidth + Styles.checkBoxWidth, rect.y, saveWidth, rect.height);
            _occlusionTab = EditorGUI.BeginFoldoutHeaderGroup(rect, _occlusionTab, Styles.OcclusionContent, _enableOcclusion ? null : Styles.hiddenFoldout);
            rect.y += Styles.defaultLineSpace;
            rect.x += Styles.tabOffset;
            rect.width -= Styles.tabOffset + Styles.checkBoxWidth;
            if (_occlusionTab)
            {
                if (_enableOcclusion)
                {   
                    EditorGUI.PropertyField(rect, NormalsMode, Styles.NormalsModeContent);
                    rect.y += Styles.defaultLineSpace;
                    EditorGUI.Slider(rect, IntensityAO, 0.001f,1.0f, Styles.IntensityAOContent);
                    rect.y += Styles.defaultLineSpace;
                    if (InjectionPoint.intValue == 4 || DebugMode.intValue != 0)
                    {
                        EditorGUI.Slider(rect, IntensitySO, 0.001f, 1.0f, Styles.IntensitySOContent);
                        rect.y += Styles.defaultLineSpace;
                        EnableOcclusion.boolValue = true;
                    }
                }
                else
                {
                    EnableOcclusion.boolValue = false;
                }
            }
            rect.x -= Styles.tabOffset;
            rect.x -= Styles.checkBoxWidth;
            rect.width += Styles.tabOffset + Styles.checkBoxWidth;
            EditorGUI.EndFoldoutHeaderGroup();
            rect = new Rect(rect.x - Styles.checkBoxOffsetWidth, rect.y, Styles.checkBoxWidth, rect.height);
            _enableDenoising = EditorGUI.Toggle(rect, _enableDenoising);
            rect = new Rect(rect.x + Styles.checkBoxOffsetWidth + Styles.checkBoxWidth, rect.y, saveWidth, rect.height);
            _denoisingTab = EditorGUI.BeginFoldoutHeaderGroup(rect, _denoisingTab, Styles.DenoisingContent, _enableDenoising ? null : Styles.hiddenFoldout);
            rect.y += Styles.defaultLineSpace;
            rect.x += Styles.tabOffset;
            rect.width -= Styles.tabOffset + Styles.checkBoxWidth;
            if (_denoisingTab)
            {
                if (_enableDenoising)
                {   
                    EditorGUI.PropertyField(rect, DenoiserIntensity, Styles.DenoiserIntensityContent);
                    rect.y += Styles.defaultLineSpace;
                    EditorGUI.Slider(rect, DenoiserRadius, 0.001f, 0.5f, Styles.DenoiserRadiusContent);
                    rect.y += Styles.defaultLineSpace;
                    DenoiserSampleCount.intValue = EditorGUI.IntSlider(rect, Styles.DenoiserSampleCountContent, DenoiserSampleCount.intValue, 8, 24);
                    rect.y += Styles.defaultLineSpace;
                    EditorGUI.Slider(rect, DetailPreservation, 0.0f, 1.0f, Styles.DetailPreservationContent);
                    rect.y += Styles.defaultLineSpace;
                    EditorGUI.Slider(rect, AccumulationSpeed, 0.0f, 5.0f, Styles.AccumulationSpeedContent);
                    rect.y += Styles.defaultLineSpace;
                    if (DenoiserIntensity.enumValueIndex != (int)HTrace.DenoiserIntensity.Low)
                    {   
                        if (ResolutionScale.enumValueIndex != (int)HTrace.ResolutionScale.HalfResolution)
                        {
                            EditorGUI.PropertyField(rect, RecurrentBlur, Styles.RecurrentBlurContent);
                            rect.y += Styles.defaultLineSpace;
                        }
                    }
                    EditorGUI.PropertyField(rect, DenoiseOcclusion, Styles.DenoiseOcclusionContent);
                    rect.y += Styles.defaultLineSpace;
                    EnableDenoising.boolValue = true;
                }
                else
                {
                    EnableDenoising.boolValue = false;
                }
            }
            rect.x -= Styles.tabOffset;
            rect.x -= Styles.checkBoxWidth;
            rect.width += Styles.tabOffset + Styles.checkBoxWidth;
            EditorGUI.EndFoldoutHeaderGroup();
            rect = new Rect(rect.x - Styles.checkBoxOffsetWidth, rect.y, Styles.checkBoxWidth, rect.height);
            _enableFallback = EditorGUI.Toggle(rect, _enableFallback);
            rect = new Rect(rect.x + Styles.checkBoxOffsetWidth + Styles.checkBoxWidth, rect.y, saveWidth, rect.height);
            _fallbackTab = EditorGUI.BeginFoldoutHeaderGroup(rect, _fallbackTab, Styles.FallbackContent, _enableFallback ? null : Styles.hiddenFoldout);
            rect.y += Styles.defaultLineSpace;
            rect.x += Styles.tabOffset;
            rect.width -= Styles.tabOffset + Styles.checkBoxWidth;
            if (_fallbackTab)
            {
                if (_enableFallback)
                {
                    EditorGUI.PropertyField(rect, FallbackMode, Styles.FallbackModeContent);
                    rect.y += Styles.defaultLineSpace;
                    if (FallbackMode.enumValueIndex == 1)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.PropertyField(rect, CustomProbe, Styles.CustomProbeContent);
                        rect.y += Styles.defaultLineSpace;
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.Slider(rect, FallbackIntensity, 0.0f, 2.0f, Styles.FallbackIntensityContent);
                    rect.y += Styles.defaultLineSpace;
                    EnableFallback.boolValue = true;
                }
                else
                {
                    EnableFallback.boolValue = false;
                }
            }
            rect.x -= Styles.tabOffset;
            rect.x -= Styles.checkBoxWidth;
            rect.width += Styles.tabOffset + Styles.checkBoxWidth;
            EditorGUI.EndFoldoutHeaderGroup();
        }
        private bool DisableSSGIInVolume()
        {
            var volumes = GameObject.FindObjectsOfType<Volume>();
            for (int i = 0; i < volumes.Length; i++)
            {
                var components = volumes[i].profile.components;
                for (int j = 0; j < components.Count; j++)
                {
                    if (components[j] is GlobalIllumination componentGI && components[j].active)
                    {
                        if (componentGI.enable.value == true)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool SSGIContainsHTrace()
        {
            var hdrpPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Packages")).First(name => name.Contains("high-definition"));
            var ssgiPath = Path.Combine(hdrpPath, "Runtime", "Lighting", "ScreenSpaceLighting",
                "HDRenderPipeline.ScreenSpaceGlobalIllumination.cs");
            var lines = System.IO.File.ReadLines(ssgiPath).ToArray();
            int countHTraceLines = 4;
            for (int i=275; i < 325; i++)
            {
                if (lines[i].Contains("//HTrace"))
                    countHTraceLines--;
                if (countHTraceLines == 0)
                    return true;
            } 
            return false;
        }
    }
#endif
}
