using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetBundleBuilder : EditorWindow
{
    private string outputPath = "Assets/AssetBundles";
    private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    
    // 模型列表，用于批量打包
    private List<ModelInfo> modelList = new List<ModelInfo>();
    
    // 临时变量，用于添加新模型
    private GameObject tempModelAsset;
    private string tempModelName = "";
    private string tempGroupName = "";
    private int tempSelectedParentTagIndex = 0;
    
    // 全局组名设置，应用于批量打包中的所有模型
    private string globalGroupName = "";
    private bool useGlobalGroup = false;
    private bool prevUseGlobalGroup = false;
    
    // Parent tag options
    private readonly string[] parentTagOptions = new string[] { "head", "neck", "body", "hip", "leftarm", "rightarm", "leftleg", "rightleg", "world" };
    
    // Help information
    private bool showHelp = false;
    
    // 滚动视图位置
    private Vector2 scrollPosition;
    
    // 默认窗口尺寸
    private const float MIN_WINDOW_WIDTH = 500f;
    private const float MIN_WINDOW_HEIGHT = 700f;
    
    // 模型信息类
    [System.Serializable]
    private class ModelInfo
    {
        public GameObject modelAsset;
        public string modelName;
        public string groupName;
        public string parentTag;
        
        public ModelInfo(GameObject asset, string name, string group, string tag)
        {
            modelAsset = asset;
            modelName = name;
            groupName = group;
            parentTag = tag;
        }
    }
    
    [MenuItem("Tools/Head Decorations Builder")]
    public static void ShowWindow()
    {
        AssetBundleBuilder window = GetWindow<AssetBundleBuilder>("Head Decorations Builder");
        window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
    }
    
    private void OnEnable()
    {
        position = new Rect(position.x, position.y, 
            Mathf.Max(position.width, MIN_WINDOW_WIDTH), 
            Mathf.Max(position.height, MIN_WINDOW_HEIGHT));
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Head Decorations Asset Bundle Builder", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Help button
        if (GUILayout.Button("Show/Hide Help"))
        {
            showHelp = !showHelp;
        }
        
        // Display help information
        if (showHelp)
        {
            EditorGUILayout.HelpBox(
                "Instructions:\n" +
                "1. Add one or more 3D models to the list\n" +
                "2. For each model, enter a name and select parent position\n" +
                "3. Optionally add a group name for MoreHeadUtilities compatibility\n" +
                "4. Click 'Build All AssetBundles' to create .hhh files for all models\n" +
                "5. Generated file format: modelName~groupName_parentTag.hhh (if group is specified)\n" +
                "   or modelName_parentTag.hhh (if no group)\n" +
                "6. Place the .hhh files in the MOD's Decorations directory\n\n" +
                "Note: File name format is very important, the system will determine the decoration's position based on the tag in the filename\n\n" +
                "Group Name (Optional): When specified, enables grouping in MoreHeadUtilities mod. Leave empty for original MoreHead behavior.\n\n" +
                "Global Group Feature (Optional):\n" +
                "- Check 'Use Global Group' to apply the same group name to all models\n" +
                "- This is useful when creating multiple decorations that should appear in the same group\n" +
                "- When not checked, each model can have its own individual group name\n" +
                "- Group names are only used by MoreHeadUtilities mod and can be left empty for original MoreHead\n\n" +
                "Parent Position Options:\n" +
                "- head: Attaches to the head, follows all head movements\n" +
                "- neck: Attaches to the neck area\n" +
                "- body: Attaches to the body\n" +
                "- hip: Attaches to the hip/lower body area\n" +
                "- leftarm: Attaches to the left arm\n" +
                "- rightarm: Attaches to the right arm\n" +
                "- leftleg: Attaches to the left leg\n" +
                "- rightleg: Attaches to the right leg\n" +
                "- world: Only follows character position, maintains horizontal orientation",
                MessageType.Info);
            
            EditorGUILayout.Space();
        }
        
        // Output path
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("Select AssetBundle Output Path", outputPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Build target platform
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target:", buildTarget);
        
        EditorGUILayout.Space();
        
        // 全局组名设置
        EditorGUILayout.LabelField("Group Settings (Optional)", EditorStyles.boldLabel);
        
        // 勾选框 - 决定是否使用全局组名
        bool newUseGlobalGroup = EditorGUILayout.Toggle("Use Global Group:", useGlobalGroup);
        
        // 全局组名输入框 - 只在勾选"Use Global Group"时显示
        if (newUseGlobalGroup)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Global Group Name:", GUILayout.Width(140));
            globalGroupName = EditorGUILayout.TextField(globalGroupName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox(
                string.IsNullOrEmpty(globalGroupName) 
                    ? "Global group name is empty. Will use original format: modelName_parentTag.hhh" 
                    : $"All models will use group: {globalGroupName}, format: modelName~{globalGroupName}_parentTag.hhh",
                string.IsNullOrEmpty(globalGroupName) ? MessageType.Info : MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Using individual group names for each model.",
                MessageType.Info);
        }
        
        // 检测全局组名设置的变化
        if (newUseGlobalGroup != useGlobalGroup)
        {
            prevUseGlobalGroup = useGlobalGroup;
            useGlobalGroup = newUseGlobalGroup;
        }
        
        EditorGUILayout.Space();
        
        // 添加新模型区域
        EditorGUILayout.LabelField("Add New Model", EditorStyles.boldLabel);
        
        // 模型资源
        tempModelAsset = (GameObject)EditorGUILayout.ObjectField("Model:", tempModelAsset, typeof(GameObject), false);
        
        // 模型名称
        string oldName = tempModelName;
        tempModelName = EditorGUILayout.TextField("Model Name:", tempModelName);
        
        // 自动填充模型名称
        if (tempModelAsset != null && string.IsNullOrEmpty(tempModelName))
        {
            tempModelName = tempModelAsset.name.Replace("_", "").Replace("~", "");
        }
        
        // 检查模型名称是否包含下划线或波浪线
        if (tempModelName.Contains("_") || tempModelName.Contains("~"))
        {
            EditorGUILayout.HelpBox("Model name should not contain underscores (_) or tildes (~). These are reserved for system use.", MessageType.Warning);
            
            // 自动移除特殊字符
            if (oldName != tempModelName)
            {
                tempModelName = tempModelName.Replace("_", "").Replace("~", "");
            }
        }
        
        // 组名输入 (如果未使用全局组名)
        if (!useGlobalGroup)
        {
            tempGroupName = EditorGUILayout.TextField("Group Name (Optional):", tempGroupName);
            if (!string.IsNullOrEmpty(tempGroupName))
            {
                EditorGUILayout.HelpBox(
                    $"Using group: {tempGroupName}, format: {tempModelName}~{tempGroupName}_{parentTagOptions[tempSelectedParentTagIndex]}.hhh", 
                    MessageType.None);
            }
        }
        
        // 父级标签选择
        tempSelectedParentTagIndex = EditorGUILayout.Popup("Parent Position:", tempSelectedParentTagIndex, parentTagOptions);
        
        EditorGUILayout.Space();
        
        // 拖放区域
        GUILayout.Box("Drag and drop decoration model here", GUILayout.ExpandWidth(true), GUILayout.Height(50));
        Rect dropArea = GUILayoutUtility.GetLastRect();
        
        // 添加模型按钮
        GUI.enabled = tempModelAsset != null && !string.IsNullOrEmpty(tempModelName);
        if (GUILayout.Button("Add Model to List"))
        {
            AddModelToList();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Models to Build", EditorStyles.boldLabel);
        
        // 显示模型列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        if (modelList.Count == 0)
        {
            EditorGUILayout.HelpBox("No models added yet. Add models using the form above.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                ModelInfo model = modelList[i];
                
                // 显示模型信息和文件名
                string fileName = GetFileName(model);
                EditorGUILayout.LabelField($"{i+1}. {fileName}", EditorStyles.boldLabel);
                
                // 移除按钮
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    modelList.RemoveAt(i);
                    GUIUtility.ExitGUI(); // 防止GUI错误
                    return;
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Model:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(model.modelAsset, typeof(GameObject), false);
                EditorGUILayout.EndHorizontal();
                
                // 显示组名 (如果有)
                if (!string.IsNullOrEmpty(model.groupName) && !useGlobalGroup)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Group:", GUILayout.Width(50));
                    EditorGUILayout.LabelField(model.groupName);
                    EditorGUILayout.EndHorizontal();
                }
                else if (useGlobalGroup && !string.IsNullOrEmpty(globalGroupName))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Group:", GUILayout.Width(50));
                    EditorGUILayout.LabelField(globalGroupName + " (global)");
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space();
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        // 批量构建按钮
        GUI.enabled = modelList.Count > 0;
        if (GUILayout.Button("Build All AssetBundles"))
        {
            BuildAllAssetBundles();
        }
        GUI.enabled = true;
        
        // 清空列表按钮
        if (modelList.Count > 0)
        {
            if (GUILayout.Button("Clear Model List"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to clear the model list?", "Yes", "No"))
                {
                    modelList.Clear();
                }
            }
        }
        
        // 处理拖放
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;
                
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject)
                        {
                            tempModelAsset = obj as GameObject;
                            // 如果模型名称为空，使用模型资源名称（移除下划线和波浪线）
                            if (string.IsNullOrEmpty(tempModelName))
                            {
                                tempModelName = tempModelAsset.name.Replace("_", "").Replace("~", "");
                            }
                            break;
                        }
                    }
                    Repaint();
                }
                break;
        }
    }
    
    // 获取文件名称（带或不带组名）
    private string GetFileName(ModelInfo model)
    {
        // 判断是否使用全局组名
        string groupToUse = useGlobalGroup ? globalGroupName : model.groupName;
        
        // 如果有组名，则使用~格式，否则使用原始格式
        if (!string.IsNullOrEmpty(groupToUse))
        {
            return $"{model.modelName}~{groupToUse}_{model.parentTag}.hhh";
        }
        else
        {
            return $"{model.modelName}_{model.parentTag}.hhh";
        }
    }
    
    // 添加模型到列表
    private void AddModelToList()
    {
        if (tempModelAsset == null || string.IsNullOrEmpty(tempModelName))
            return;
        
        // 确定使用的组名
        string groupToUse = useGlobalGroup ? globalGroupName : tempGroupName;
        
        // 创建新的模型信息
        ModelInfo newModel = new ModelInfo(
            tempModelAsset,
            tempModelName,
            groupToUse,
            parentTagOptions[tempSelectedParentTagIndex]
        );
        
        // 添加到列表
        modelList.Add(newModel);
        
        // 清空临时变量，准备添加下一个
        tempModelAsset = null;
        tempModelName = "";
        if (!useGlobalGroup)
        {
            tempGroupName = ""; // 只有在不使用全局组时才清空
        }
        // 保持父级标签选择不变
    }
    
    // 构建所有AssetBundle
    private void BuildAllAssetBundles()
    {
        if (modelList.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No models to build", "OK");
            return;
        }
        
        // 确保输出目录存在
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        // 创建AssetBundleBuild数组
        AssetBundleBuild[] buildMap = new AssetBundleBuild[modelList.Count];
        
        // 填充构建映射
        for (int i = 0; i < modelList.Count; i++)
        {
            ModelInfo model = modelList[i];
            
            // 设置bundle名称 - 使用基本名称，不包含组名（组名只用于最终文件名）
            string bundleName = $"{model.modelName}_{model.parentTag}";
            buildMap[i].assetBundleName = bundleName;
            
            // 获取模型资源路径
            string assetPath = AssetDatabase.GetAssetPath(model.modelAsset);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Error", $"Cannot get asset path for model: {model.modelName}", "OK");
                return;
            }
            
            // 设置资源路径
            buildMap[i].assetNames = new string[] { assetPath };
        }
        
        // 构建AssetBundle
        BuildPipeline.BuildAssetBundles(outputPath, buildMap, BuildAssetBundleOptions.None, buildTarget);
        
        // 重命名文件，添加.hhh扩展名和可能的组名
        List<string> builtFiles = new List<string>();
        
        foreach (ModelInfo model in modelList)
        {
            // 用于构建过程的基本名称
            string basicBundleName = $"{model.modelName}_{model.parentTag}";
            string basicBundlePath = Path.Combine(outputPath, basicBundleName);
            
            // 最终文件名（可能包含组名）
            string finalFileName = GetFileName(model);
            string finalFilePath = Path.Combine(outputPath, finalFileName);
            
            // 如果目标文件已存在，先删除
            if (File.Exists(finalFilePath))
            {
                File.Delete(finalFilePath);
            }
            
            // 重命名文件
            if (File.Exists(basicBundlePath))
            {
                File.Move(basicBundlePath, finalFilePath);
                builtFiles.Add(finalFilePath);
            }
        }
        
        // 显示成功消息
        EditorUtility.DisplayDialog("Success", 
            $"Built {builtFiles.Count} AssetBundles to: {outputPath}\n\n" +
            "Please copy these files to the MOD's Decorations directory", 
            "OK");
        
        // 在文件资源管理器中显示
        EditorUtility.RevealInFinder(outputPath);
    }
}
