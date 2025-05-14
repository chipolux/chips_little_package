using UnityEngine;
using UnityEditor;

// 确保这个脚本只在Unity编辑器中编译
#if UNITY_EDITOR
[ExecuteInEditMode]
public class LockTransformPosition : MonoBehaviour
{
    // 存储原始变换值
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    
    private void OnEnable()
    {
        // 记录当前变换值
        StoreCurrentTransform();
        
        // 订阅编辑器更新事件
        EditorApplication.update += CheckTransform;
    }
    
    private void OnDisable()
    {
        // 取消订阅编辑器更新事件
        EditorApplication.update -= CheckTransform;
    }
    
    private void StoreCurrentTransform()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }
    
    private void CheckTransform()
    {
        // 只在编辑模式下执行
        if (!Application.isPlaying)
        {
            bool changed = false;
            
            // 检查并恢复位置
            if (transform.localPosition != originalPosition)
            {
                transform.localPosition = originalPosition;
                changed = true;
            }
            
            // 检查并恢复旋转
            if (transform.localRotation != originalRotation)
            {
                transform.localRotation = originalRotation;
                changed = true;
            }
            
            // 检查并恢复缩放
            if (transform.localScale != originalScale)
            {
                transform.localScale = originalScale;
                changed = true;
            }
            
            // 如果有变化，通知编辑器刷新
            if (changed)
            {
                EditorUtility.SetDirty(this);
            }
        }
    }
}
#endif
