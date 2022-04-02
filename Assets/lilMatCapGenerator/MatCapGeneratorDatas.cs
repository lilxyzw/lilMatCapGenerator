using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MatCapGeneratorDatas : ScriptableObject
{
    public enum LayerType
    {
        Specular,
        MatCap,
        CubeMap,
        ToneCorrection
    }

    [SerializeField]
    public List<LayerType> m_Layers = new List<LayerType>();

    [SerializeField]
    public string m_shaderText = "";

    public static MatCapGeneratorDatas GetFromMaterial(Material material)
    {
        string path = AssetDatabase.GetAssetPath(material);
        return AssetDatabase.LoadAssetAtPath<MatCapGeneratorDatas>(path);
    }

    public static Shader GetShaderFromMaterial(Material material)
    {
        string path = AssetDatabase.GetAssetPath(material);
        return AssetDatabase.LoadAssetAtPath<Shader>(path);
    }

    public void CreateNewShader(string assetPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Shader \"Hidden/" + assetPath + "\"");

        var sr = new StreamReader(AssetDatabase.GUIDToAssetPath("9f54427aabe6ee845817a727843d72f9"));
        string line;

        while((line = sr.ReadLine()) != null)
        {
            if(line.Contains("*PROPERTIES_BLOCK*"))
            {
                InsertProperties(sb);
                continue;
            }
            if(line.Contains("*PROPERTIES_HLSL*"))
            {
                InsertPropertiesHLSL(sb);
                continue;
            }
            if(line.Contains("*LAYERS_HLSL*"))
            {
                InsertSamples(sb);
                continue;
            }
            sb.AppendLine(line);
        }

        m_shaderText = sb.ToString();
    }

    private void InsertProperties(StringBuilder sb)
    {
        for(int i = 0; i < m_Layers.Count; i++)
        {
            string num = i.ToString();
            switch(m_Layers[i])
            {
                case LayerType.Specular:
                    sb.AppendLine("        _LC" + num + " (\"\", Color) = (1,1,1,1)");
                    sb.AppendLine("        _LP" + num + " (\"\", Vector) = (-0.75,1.5,0.5,1.0)");
                    sb.AppendLine("        _Smoothness" + num + " (\"\", Range(0,1)) = 0.7");
                    sb.AppendLine("        _SmoothnessCC" + num + " (\"\", Range(0,1)) = 0.9");
                    sb.AppendLine("        _CCStrength" + num + " (\"\", Float) = 0.25");
                    sb.AppendLine("        _BlendMode" + num + " (\"\", Int) = 10");
                    break;
                case LayerType.MatCap:
                    sb.AppendLine("        _MatCapColor" + num + " (\"\", Color) = (1,1,1,1)");
                    sb.AppendLine("        _MatCap" + num + " (\"\", 2D) = \"black\" {}");
                    sb.AppendLine("        _BlendMode" + num + " (\"\", Int) = 10");
                    break;
                case LayerType.CubeMap:
                    sb.AppendLine("        _CubeMapColor" + num + " (\"\", Color) = (1,1,1,1)");
                    sb.AppendLine("        _CubeMap" + num + " (\"\", Cube) = \"\" {}");
                    sb.AppendLine("        _CubeMapRotation" + num + " (\"\", Vector) = (0.0,0.0,0.0,1.0)");
                    sb.AppendLine("        _Smoothness" + num + " (\"\", Range(0,1)) = 0.7");
                    sb.AppendLine("        _BlendMode" + num + " (\"\", Int) = 10");
                    break;
                case LayerType.ToneCorrection:
                    sb.AppendLine("        _HSVG" + num + " (\"\", Vector) = (0,1,1,1)");
                    sb.AppendLine("        _ClampMin" + num + " (\"\", Float) = 0");
                    sb.AppendLine("        _ClampMax" + num + " (\"\", Float) = 1");
                    break;
            }
        }
    }

    private void InsertPropertiesHLSL(StringBuilder sb)
    {
        for(int i = 0; i < m_Layers.Count; i++)
        {
            string num = i.ToString();
            switch(m_Layers[i])
            {
                case LayerType.Specular:
                    sb.AppendLine("            SPECULAR_LAYER_PROPS(" + num + ")");
                    break;
                case LayerType.MatCap:
                    sb.AppendLine("            MATCAP_LAYER_PROPS(" + num + ")");
                    break;
                case LayerType.CubeMap:
                    sb.AppendLine("            CUBE_LAYER_PROPS(" + num + ")");
                    break;
                case LayerType.ToneCorrection:
                    sb.AppendLine("            TONECORRECTION_LAYER_PROPS(" + num + ")");
                    break;
            }
        }
    }

    private void InsertSamples(StringBuilder sb)
    {
        for(int i = 0; i < m_Layers.Count; i++)
        {
            string num = i.ToString();
            switch(m_Layers[i])
            {
                case LayerType.Specular:
                    sb.AppendLine("                SPECULAR_LAYER(" + num + ")");
                    break;
                case LayerType.MatCap:
                    sb.AppendLine("                MATCAP_LAYER(" + num + ")");
                    break;
                case LayerType.CubeMap:
                    sb.AppendLine("                CUBE_LAYER(" + num + ")");
                    break;
                case LayerType.ToneCorrection:
                    sb.AppendLine("                TONECORRECTION_LAYER(" + num + ")");
                    break;
            }
        }
    }

    [MenuItem("Assets/_lil/MatCapGenerator/Create Material")]
    private static void CreateMaterial()
    {
        var am = CreateInstance<MatCapGeneratorDatas>();
        string path = EditorUtility.SaveFilePanel("Save Material", "Assets", "", "asset");
        if(string.IsNullOrEmpty(path))
        {
            return;
        }
        path = FileUtil.GetProjectRelativePath(path);
        AssetDatabase.CreateAsset(am, path);

        am.CreateNewShader(path);
        Shader shader = ShaderUtil.CreateShaderAsset(am.m_shaderText, false);
        AssetDatabase.AddObjectToAsset(shader, path);

        var material = new Material(shader);
        AssetDatabase.AddObjectToAsset(material, path);
        AssetDatabase.SetMainObject(material, path);

        EditorUtility.SetDirty(am);
        AssetDatabase.SaveAssets();
    }
}