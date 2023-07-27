using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector2 = System.Numerics.Vector2;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;
using DSAnimStudio.TaeEditor;
using DSAnimStudio.ImguiOSD;
using NAudio.Wave;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {

        public static void BuildDebugMenu()
        {
            // Dark Souls: Nightfall tests removed for public version lel
            return;
        }

        private enum DebugValType
        {
            Int,
            Float,
            Bool,
            String
        };

        private static Dictionary<string, int> debugValFields_Int = new Dictionary<string, int>();
        private static Dictionary<string, float> debugValFields_Float = new Dictionary<string, float>();
        private static Dictionary<string, bool> debugValFields_Bool = new Dictionary<string, bool>();
        private static Dictionary<string, string> debugValFields_String = new Dictionary<string, string>();

        private static int UpdateInt(string key, string dispName, int defaultValue = 0)
        {
            var dict = debugValFields_Int;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputInt($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static float UpdateFloat(string key, string dispName, float defaultValue = 0)
        {
            var dict = debugValFields_Float;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputFloat($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static bool UpdateBool(string key, string dispName, bool defaultValue = false)
        {
            var dict = debugValFields_Bool;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.Checkbox($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static string UpdateString(string key, string dispName, string defaultValue = "", uint maxStrLength = 128)
        {
            var dict = debugValFields_String;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputText($"{dispName}##{key}", ref curVal, maxStrLength);
            dict[key] = curVal;

            return curVal;
        }

        private static int GetInt(string key)
        {
            var dict = debugValFields_Int;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private static float GetFloat(string key)
        {
            var dict = debugValFields_Float;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private static bool GetBool(string key)
        {
            var dict = debugValFields_Bool;
            return dict.ContainsKey(key) ? dict[key] : false;
        }

        private static string GetString(string key)
        {
            var dict = debugValFields_String;
            return dict.ContainsKey(key) ? dict[key] : "";
        }

        private static bool DebugTestButton(string name)
        {
            ImGui.Button("TEST: " + name);
            return ImGui.IsItemClicked();
        }
    }
}
