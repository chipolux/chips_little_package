using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode] // 让脚本在编辑器模式下运行
public class MaterialColorController : MonoBehaviour
{
    private Material targetMaterial; // 设为私有，避免被误修改
    [SerializeField] private Color albedoColor = Color.white; // 颜色仍然可以在 Inspector 里调整

    private string materialPath = "Assets/Morehead/Materials/Player Avatar - Body.mat"; // 固定材质路径

    private void OnEnable()
    {
        LoadMaterial();
    }

    private void OnValidate()
    {
        if (targetMaterial == null)
        {
            LoadMaterial();
        }

        if (targetMaterial != null)
        {
            targetMaterial.SetColor("_AlbedoColor", albedoColor);
        }
    }

    private void LoadMaterial()
    {
        targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (targetMaterial == null)
        {
            Debug.LogError("Material not found at: " + materialPath);
        }
    }
}
#endif
