using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace lilMatCapGenerator
{
    public class MCGShaderGUI : ShaderGUI
    {
        private ReorderableList _reorderableList;
        private static Material material;
        private static MatCapGeneratorDatas am;
        private static string assetPath;
        private static SerializedObject so;
        private static int textureSize = 512;
        private static int lang = -1;
        private static readonly string[] TEXT_LANGUAGES = new[] {"English", "Japanese"};
        private static readonly string[] TEXT_LAYERS = new[] {"Layers", "レイヤー"};
        private static readonly string[] TEXT_TYPE = new[] {"Type", "種類"};
        private static readonly string[] TEXT_SPECULAR = new[] {"Specular", "スペキュラ"};
        private static readonly string[] TEXT_MATCAP = new[] {"MatCap", "マットキャップ"};
        private static readonly string[] TEXT_CUBEMAP = new[] {"CubeMap", "キューブマップ"};
        private static readonly string[] TEXT_COLOR = new[] {"Color", "色"};
        private static readonly string[] TEXT_NORMALMAP = new[] {"Normal Map", "ノーマルマップ"};
        private static readonly string[] TEXT_REFLECTANCE = new[] {"Reflectance", "反射率"};
        private static readonly string[] TEXT_DIRECTION = new[] {"Direction", "方向"};
        private static readonly string[] TEXT_SMOOTHNESS = new[] {"Smoothness", "Smoothness"};
        private static readonly string[] TEXT_CC_SMOOTHNESS = new[] {"Smoothness (Clear Coat)", "Smoothness (クリアコート)"};
        private static readonly string[] TEXT_CC_STRENGTH = new[] {"Strength (Clear Coat)", "クリアコートの強度"};
        private static readonly string[] TEXT_ROTATION = new[] {"Rotation", "Rotation"};
        private static readonly string[] TEXT_BLENDMODE = new[] {"Blend Mode", "合成モード"};
        private static readonly string[][] TEXT_BLENDMODES = {
            new[] {
                "Normal",
                "Darken",
                "Multiply",
                "ColorBurn",
                "LinearBurn",
                "DarkerColor",
                "Lighten",
                "Screen",
                "ColorDodge",
                "GlowDodge",
                "Add",
                "AddGlow",
                "LighterColor",
                "Overlay",
                "SoftLight",
                "SoftLightPegtop",
                "HardLight",
                "VividLight",
                "LinearLight",
                "PinLight",
                "HardMix",
                "Difference",
                "Exclusion",
                "Subtract",
                "Divide",
                "Hue",
                "Saturation",
                "Color",
                "Brightness"
            },
            new[] {
                "通常",
                "比較(暗)",
                "乗算",
                "焼き込みカラー",
                "焼き込み(リニア)",
                "カラー比較(暗)",
                "比較(明)",
                "スクリーン",
                "覆い焼きカラー",
                "覆い焼き(発光)",
                "加算",
                "加算(発光)",
                "カラー比較(明)",
                "オーバーレイ",
                "ソフトライト (Photoshop)",
                "ソフトライト (Pegtop)",
                "ハードライト",
                "ビビッドライト",
                "リニアライト",
                "ピンライト",
                "ハードミックス",
                "差の絶対値",
                "除外",
                "減算",
                "除算",
                "色相",
                "彩度",
                "カラー",
                "輝度"
            }
        };
        private static readonly string[] TEXT_CLAMP_MIN = new[] {"Clamp Min", "色の下限"};
        private static readonly string[] TEXT_CLAMP_MAX = new[] {"Clamp Max", "色の上限"};

        private static readonly string[] TEXT_MATERIAL_SETTINGS = new[] {"Material Settings", "マテリアル設定"};
        private static readonly string[] TEXT_EXPORT_SETTINGS = new[] {"Export Settings", "書き出し設定"};
        private static readonly string[] TEXT_TEXTURE_SIZE = new[] {"Texture Size", "テクスチャサイズ"};
        private static readonly string[] TEXT_APPLY = new[] {"Apply", "適用"};
        private static readonly string[] TEXT_EXPORT_TEXTURE = new[] {"Texture Export", "テクスチャを書き出し"};
        private static readonly string[] TEXT_EXPORT_SHADER = new[] {"Export Shader", "シェーダーを書き出し"};

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            // Language
            if(lang == -1)
            {
                lang = Application.systemLanguage == SystemLanguage.Japanese ? 1 : 0;
            }
            lang = EditorGUILayout.Popup("Language", lang, TEXT_LANGUAGES);

            Material targetMaterial = (Material)materialEditor.target;
            if(material != targetMaterial)
            {
                material = targetMaterial;
                am = MatCapGeneratorDatas.GetFromMaterial(material);
                so = new SerializedObject(am);
            }
            assetPath = AssetDatabase.GetAssetPath(material);

            so.Update();
            var listProp = so.FindProperty("m_Layers");

            if(_reorderableList == null)
            {
                _reorderableList = new ReorderableList(so, listProp)
                {
                    draggable = true,
                    drawHeaderCallback = rect => EditorGUI.LabelField(rect, TEXT_LAYERS[lang]),
                    elementHeightCallback = index => (EditorGUIUtility.singleLineHeight + 2) * 7 + 6,
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        string num = index.ToString();
                        var elementProperty = listProp.GetArrayElementAtIndex(index);
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(rect, elementProperty, new GUIContent(TEXT_TYPE[lang]));

                        switch((MatCapGeneratorDatas.LayerType)elementProperty.enumValueIndex)
                        {
                            case MatCapGeneratorDatas.LayerType.Specular:
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawColorProperty(rect, material, "_LC" + num, TEXT_COLOR[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawVectorProperty(rect, material, "_LP" + num, TEXT_DIRECTION[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawRangeProperty(rect, material, "_Smoothness" + num, TEXT_SMOOTHNESS[lang], 0.0f, 1.0f);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawRangeProperty(rect, material, "_SmoothnessCC" + num, TEXT_CC_SMOOTHNESS[lang], 0.0f, 1.0f);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawFloatProperty(rect, material, "_CCStrength" + num, TEXT_CC_STRENGTH[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawBlendModeProperty(rect, material, "_BlendMode" + num, TEXT_BLENDMODE[lang]);
                                break;
                            case MatCapGeneratorDatas.LayerType.MatCap:
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawColorProperty(rect, material, "_MatCapColor" + num, TEXT_COLOR[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawTextureProperty(rect, material, "_MatCap" + num, TEXT_MATCAP[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawBlendModeProperty(rect, material, "_BlendMode" + num, TEXT_BLENDMODE[lang]);
                                break;
                            case MatCapGeneratorDatas.LayerType.CubeMap:
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawColorProperty(rect, material, "_CubeMapColor" + num, TEXT_COLOR[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawCubemapProperty(rect, material, "_CubeMap" + num, TEXT_CUBEMAP[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawVectorProperty(rect, material, "_CubeMapRotation" + num, TEXT_ROTATION[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawRangeProperty(rect, material, "_Smoothness" + num, TEXT_SMOOTHNESS[lang], 0.0f, 1.0f);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawBlendModeProperty(rect, material, "_BlendMode" + num, TEXT_BLENDMODE[lang]);
                                break;
                            case MatCapGeneratorDatas.LayerType.ToneCorrection:
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawFloatProperty(rect, material, "_ClampMin" + num, TEXT_CLAMP_MIN[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawFloatProperty(rect, material, "_ClampMax" + num, TEXT_CLAMP_MAX[lang]);
                                rect.y += EditorGUIUtility.singleLineHeight + 2;
                                DrawHSVGProperty(rect, material, "_HSVG" + num);
                                break;
                        }
                    }
                };
            }

            EditorGUILayout.LabelField(TEXT_MATERIAL_SETTINGS[lang], EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            MaterialProperty _Color = FindProperty("_Color", props, false);
            MaterialProperty _NormalMap = FindProperty("_NormalMap", props, false);
            MaterialProperty _NormalScale = FindProperty("_NormalScale", props, false);
            MaterialProperty _Reflectance = FindProperty("_Reflectance", props, false);
            if(_Color != null) materialEditor.ShaderProperty(_Color, TEXT_COLOR[lang]);
            if(_Reflectance != null) materialEditor.ShaderProperty(_Reflectance, TEXT_REFLECTANCE[lang]);
            if(_NormalMap != null && _NormalScale != null) materialEditor.TexturePropertySingleLine(new GUIContent(TEXT_NORMALMAP[lang]), _NormalMap, _NormalScale);
            EditorGUILayout.Space();
            _reorderableList.DoLayoutList();

            if(GUILayout.Button(TEXT_APPLY[lang]))
            {
                var shaders = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x is Shader);
                foreach(var shader in shaders)
                {
                    Object.DestroyImmediate(shader, true);
                }
                am.CreateNewShader(assetPath);
                Shader newShader = ShaderUtil.CreateShaderAsset(am.m_shaderText, false);
                AssetDatabase.AddObjectToAsset(newShader, assetPath);
                material.shader = newShader;
                EditorUtility.SetDirty(am);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            so.ApplyModifiedProperties();

            EditorGUILayout.LabelField(TEXT_EXPORT_SETTINGS[lang], EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            textureSize = EditorGUILayout.IntField(TEXT_TEXTURE_SIZE[lang], textureSize);
            if(GUILayout.Button(TEXT_EXPORT_TEXTURE[lang]))
            {
                ExportTexture(material, textureSize);
            }
            if(GUILayout.Button(TEXT_EXPORT_SHADER[lang]))
            {
                string exportPath = EditorUtility.SaveFilePanel(TEXT_EXPORT_SHADER[lang], Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath), "shader");
                if(string.IsNullOrEmpty(exportPath)) return;
                File.WriteAllText(exportPath, am.m_shaderText);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private static void DrawColorProperty(Rect rect, Material material, string propname, string dispname)
        {
            Color val;
            if(material.HasProperty(propname))  val = material.GetColor(propname);
            else                                val = new Color(1,1,1,1);
            EditorGUI.BeginChangeCheck();
            val = EditorGUI.ColorField(rect, new GUIContent(dispname), val, true, true, true);
            if(EditorGUI.EndChangeCheck()) material.SetColor(propname, val);
        }

        private static void DrawTextureProperty(Rect rect, Material material, string propname, string dispname)
        {
            Texture val = null;
            if(material.HasProperty(propname))  val = material.GetTexture(propname);
            EditorGUI.BeginChangeCheck();
            val = (Texture)EditorGUI.ObjectField(rect, dispname, val, typeof(Texture2D), false);
            if(EditorGUI.EndChangeCheck()) material.SetTexture(propname, val);
        }

        private static void DrawCubemapProperty(Rect rect, Material material, string propname, string dispname)
        {
            Texture val = null;
            if(material.HasProperty(propname))  val = material.GetTexture(propname);
            EditorGUI.BeginChangeCheck();
            val = (Texture)EditorGUI.ObjectField(rect, dispname, val, typeof(Cubemap), false);
            if(EditorGUI.EndChangeCheck()) material.SetTexture(propname, val);
        }

        private static void DrawVectorProperty(Rect rect, Material material, string propname, string dispname)
        {
            Vector4 val;
            if(material.HasProperty(propname))  val = material.GetVector(propname);
            else val = new Vector4(0,0,0,1);
            EditorGUI.BeginChangeCheck();
            val = EditorGUI.Vector4Field(rect, dispname, val);
            if(EditorGUI.EndChangeCheck()) material.SetVector(propname, val);
        }

        private static void DrawFloatProperty(Rect rect, Material material, string propname, string dispname)
        {
            float val = 0;
            if(material.HasProperty(propname))  val = material.GetFloat(propname);
            EditorGUI.BeginChangeCheck();
            val = EditorGUI.FloatField(rect, dispname, val);
            if(EditorGUI.EndChangeCheck()) material.SetFloat(propname, val);
        }

        private static void DrawHSVGProperty(Rect rect, Material material, string propname)
        {
            Vector4 val;
            if(material.HasProperty(propname))  val = material.GetVector(propname);
            else val = new Vector4(0,0,0,1);
            EditorGUI.BeginChangeCheck();
            val = EditorGUI.Vector4Field(rect, "", val);
            if(EditorGUI.EndChangeCheck()) material.SetVector(propname, val);
        }

        private static void DrawRangeProperty(Rect rect, Material material, string propname, string dispname, float min, float max)
        {
            float val = 0;
            if(material.HasProperty(propname))  val = material.GetFloat(propname);
            EditorGUI.BeginChangeCheck();
            val = EditorGUI.Slider(rect, dispname, val, min, max);
            if(EditorGUI.EndChangeCheck()) material.SetFloat(propname, val);
        }

        private enum BlendModes
        {
            Normal,             // 通常
            Darken,             // 比較(暗)
            Multiply,           // 乗算
            ColorBurn,          // 焼き込みカラー
            LinearBurn,         // 焼き込み(リニア)
            DarkerColor,        // カラー比較(暗)
            Lighten,            // 比較(明)
            Screen,             // スクリーン
            ColorDodge,         // 覆い焼きカラー
            GlowDodge,          // 覆い焼き(発光)
            Add,                // 加算 / 覆い焼き(リニア)
            AddGlow,            // 加算(発光)
            LighterColor,       // カラー比較(明)
            Overlay,            // オーバーレイ
            SoftLight,          // ソフトライト (Photoshop)
            SoftLightPegtop,    // ソフトライト (Pegtop)
            HardLight,          // ハードライト
            VividLight,         // ビビッドライト
            LinearLight,        // リニアライト
            PinLight,           // ピンライト
            HardMix,            // ハードミックス
            Difference,         // 差の絶対値
            Exclusion,          // 除外
            Subtract,           // 減算
            Divide,             // 除算
            Hue,                // 色相
            Saturation,         // 彩度
            Color,              // カラー
            Brightness          // 輝度
        }

        private static void DrawBlendModeProperty(Rect rect, Material material, string propname, string dispname)
        {
            BlendModes val = BlendModes.Add;
            if(material.HasProperty(propname))  val = (BlendModes)material.GetFloat(propname);
            EditorGUI.BeginChangeCheck();
            val = (BlendModes)EditorGUI.Popup(rect, dispname, (int)val, TEXT_BLENDMODES[lang]);
            if(EditorGUI.EndChangeCheck()) material.SetFloat(propname, (int)val);
        }

        private static void ExportTexture(Material material, int size)
        {
            var bakeMaterial = new Material(material);
            bakeMaterial.EnableKeyword("IS_OUTPUT_MODE");
            var srcTexture = new Texture2D(size, size);
            var outTexture = new Texture2D(size, size);

            var bufRT = RenderTexture.active;
            var dstTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
            RenderTexture.active = dstTexture;

            Graphics.Blit(srcTexture, dstTexture, bakeMaterial);
            outTexture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            outTexture.Apply();

            RenderTexture.active = bufRT;
            RenderTexture.ReleaseTemporary(dstTexture);

            string path = EditorUtility.SaveFilePanel("Save Texture", "Assets", "", "png");
            if(!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, outTexture.EncodeToPNG());
            }
            Object.DestroyImmediate(bakeMaterial);
            Object.DestroyImmediate(srcTexture);
            Object.DestroyImmediate(outTexture);
        }
    }
}