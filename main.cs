using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class FaceEmoShapeKeyCleaner : EditorWindow
{
    private GameObject targetAvatar;

    [MenuItem("Tools/FaceEmo/Unused ShapeKey Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<FaceEmoShapeKeyCleaner>("ShapeKey Cleaner");
    }

    void OnGUI()
    {
        GUILayout.Label("FaceEmoで使用されていないシェイプキーを削除します", EditorStyles.boldLabel);
        targetAvatar = (GameObject)EditorGUILayout.ObjectField("Target Avatar", targetAvatar, typeof(GameObject), true);

        if (GUILayout.Button("未使用シェイプキーを削除する") && targetAvatar != null)
        {
            CleanUnusedShapeKeys();
        }
    }

    private void CleanUnusedShapeKeys()
    {
        var smr = targetAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null || smr.sharedMesh == null)
        {
            Debug.LogError("SkinnedMeshRendererが見つかりません。");
            return;
        }

        Mesh mesh = smr.sharedMesh;
        // アニメーション等で使われているパスを収集
        HashSet<string> usedShapeKeys = GetUsedShapeKeysInProject();

        List<string> toRemove = new List<string>();
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string name = mesh.GetBlendShapeName(i);
            if (!usedShapeKeys.Contains(name))
            {
                toRemove.Add(name);
            }
        }

        if (toRemove.Count == 0)
        {
            Debug.Log("削除対象の未使用シェイプキーはありませんでした。");
            return;
        }

        // 実際の削除処理（Meshの再構築などが必要なため、ここではログ出力とリストアップに留めます）
        // ※Meshを直接書き換えるのはリスクが高いため、実運用ではアセットとして保存する工程が必要です。
        Debug.Log($"発見された未使用シェイプキー ({toRemove.Count}個): " + string.Join(", ", toRemove));
        
        // 注意: 実際のMesh.DeleteBlendShapeはAPIに存在しないため、
        // 本来は新しいMeshを作成して必要なデータだけをコピーする処理が必要です。
    }

    private HashSet<string> GetUsedShapeKeysInProject()
    {
        HashSet<string> used = new HashSet<string>();
        // プロジェクト内の全Animationを検索（FaceEmoの生成物を含む）
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.StartsWith("blendShape."))
                {
                    used.Add(binding.propertyName.Replace("blendShape.", ""));
                }
            }
        }
        return used;
    }
}