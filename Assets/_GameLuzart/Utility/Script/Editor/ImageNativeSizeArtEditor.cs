#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(Image)), CanEditMultipleObjects]
public class ImageArtNativeSizeEditor : ImageEditor
{
    private static readonly Vector2 artReferenceResolution = new Vector2(1920, 1080);

    public override void OnInspectorGUI()
    {
        // 👇 Vẽ lại toàn bộ Inspector gốc của Unity
        base.OnInspectorGUI();

        // 🔽 Thêm phần tùy chỉnh bên dưới
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎨 Art Layout Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Set Native Size Match Art Resolution"))
        {
            foreach (var t in targets)
            {
                SetNativeSizeToMatchArt(t as Image);
            }
        }
    }

    private void SetNativeSizeToMatchArt(Image img)
    {
        if (img == null || img.sprite == null)
        {
            Debug.LogWarning($"❌ Skipped: {img?.name} (No sprite)");
            return;
        }

        CanvasScaler scaler = img.GetComponentInParent<CanvasScaler>();
        if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            Debug.LogWarning($"❌ Skipped: {img.name} (Missing CanvasScaler or wrong mode)");
            return;
        }

        Vector2 currentRef = scaler.referenceResolution;
        float widthRatio = currentRef.x / artReferenceResolution.x;
        float heightRatio = currentRef.y / artReferenceResolution.y;

        float compensateScale = 1f;
        switch (scaler.screenMatchMode)
        {
            case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                compensateScale = Mathf.Lerp(widthRatio, heightRatio, scaler.matchWidthOrHeight);
                break;
            case CanvasScaler.ScreenMatchMode.Expand:
                compensateScale = Mathf.Max(widthRatio, heightRatio);
                break;
            case CanvasScaler.ScreenMatchMode.Shrink:
                compensateScale = Mathf.Min(widthRatio, heightRatio);
                break;
        }

        Vector2 spriteSize = img.sprite.rect.size;
        Vector2 finalSize = spriteSize * compensateScale;

        Undo.RecordObject(img.rectTransform, "Set Native Size Match Art");
        img.rectTransform.sizeDelta = finalSize;

        Debug.Log($"✅ {img.name} sizeDelta = {finalSize} (scale = {compensateScale})");
    }
}
#endif
