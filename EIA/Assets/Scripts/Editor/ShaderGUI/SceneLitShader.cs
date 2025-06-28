using UnityEngine;
using UnityEditor;
using System.Reflection;

public class SceneLitShaderGUI : BaseShaderGUI
{
    // 反射所需的类型和方法
    private static System.Type litShaderGUIType;
    private static MethodInfo onGUIMethod;
    private static MethodInfo validateMaterialMethod;
    private static object baseShaderGUIInstance;

    // 自定义属性
    private MaterialProperty customTextureProp;
    private MaterialProperty customColorProp;

    static SceneLitShaderGUI()
    {
        // 使用反射获取URP LitShaderGUI类型
        litShaderGUIType = System.Type.GetType("UnityEditor.Rendering.Universal.ShaderGUI.LitShader, Unity.RenderPipelines.Universal.Editor");
        if (litShaderGUIType != null)
        {
            // 创建实例
            baseShaderGUIInstance = System.Activator.CreateInstance(litShaderGUIType);
            // 获取OnGUI方法
            onGUIMethod = litShaderGUIType.GetMethod("OnGUI", 
                BindingFlags.Public | BindingFlags.Instance, 
                null, 
                new System.Type[] { typeof(MaterialEditor), typeof(MaterialProperty[]) }, 
                null);
            validateMaterialMethod = litShaderGUIType.GetMethod("ValidateMaterial", 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new System.Type[] { typeof(Material) }, 
                null);
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 查找自定义属性
        customTextureProp = FindProperty("_BaseMap2", properties, false);
        //customColorProp = FindProperty("_CustomColor", properties, false);

        
        // 添加自定义GUI部分
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Settings", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        
        if (customTextureProp != null)
        {
            materialEditor.TexturePropertySingleLine(
                new GUIContent("_BaseMap2", "自定义贴图"), 
                customTextureProp
            );
        }
        // 如果找到了LitShaderGUI，调用它的OnGUI方法
        if (onGUIMethod != null && baseShaderGUIInstance != null)
        {
            onGUIMethod.Invoke(baseShaderGUIInstance, new object[] { materialEditor, properties });
        }
        else
        {
            // 后备方案：绘制默认GUI
            base.OnGUI(materialEditor, properties);
        }

        
        
        /*if (customColorProp != null)
        {
            materialEditor.ColorProperty(customColorProp, "Custom Color");
        }*/
        
        EditorGUI.indentLevel--;
    }
}