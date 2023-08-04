using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
namespace HTrace
{
	public static class HExtensions
	{
		
		private const string HTRACE_RESOURCES_FOLDER_NAME = "H-Trace";
		public static ComputeShader LoadComputeShader(string shaderName)
		{
			var computeShader = (ComputeShader)Resources.Load($"{HTRACE_RESOURCES_FOLDER_NAME}/{shaderName}");
			if (computeShader == null)
			{
				Debug.LogWarning($"{HTRACE_RESOURCES_FOLDER_NAME}/{shaderName} is missing");
				return null;
			}
			return computeShader;
		}
		
		public static string HName<T>(this T src) where T : RTHandle
		{
			return src == null ? string.Empty : string.Join("_",src.name);
		}
		
		
		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos) {
				if (pinfo.CanWrite) {
					try {
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
					catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
				}
			}
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos) {
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return comp as T;
			
		}
		
		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().GetCopyOf(toAdd) as T;
		}
		
		public static bool IsSSGIEnabled(HDCamera camera)
		{
			var ssgi = camera.volumeStack.GetComponent<GlobalIllumination>();
			return camera.frameSettings.IsEnabled(FrameSettingsField.SSGI) && ssgi.enable.value;
		}
		
		/// <summary>
		/// Remap from one range to another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="oldLow"></param>
		/// <param name="oldHigh"></param>
		/// <param name="newLow"></param>
		/// <param name="newHigh"></param>
		/// <returns></returns>
		public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
		{
			float t = Mathf.InverseLerp(oldLow, oldHigh, input);
			return Mathf.Lerp(newLow, newHigh, t);
		}
	}
}
