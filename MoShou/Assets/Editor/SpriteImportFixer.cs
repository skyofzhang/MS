using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 修复Sprite导入设置的Editor工具
/// 按照策划案要求设置正确的导入参数
/// </summary>
public class SpriteImportFixer : EditorWindow
{
    [MenuItem("MoShou/Fix Sprite Imports")]
    public static void FixAllSpriteImports()
    {
        string[] spriteFolders = new string[]
        {
            "Assets/Resources/Sprites/Generated",
            "Assets/Resources/Sprites/UI"
        };

        int fixedCount = 0;

        foreach (string folder in spriteFolders)
        {
            if (!Directory.Exists(folder))
            {
                Debug.LogWarning($"[SpriteImportFixer] 目录不存在: {folder}");
                continue;
            }

            string[] pngFiles = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);

            foreach (string filePath in pngFiles)
            {
                string assetPath = filePath.Replace("\\", "/");

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool needsReimport = false;

                // 设置为Sprite类型
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    needsReimport = true;
                }

                // 设置Sprite模式为Single
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    needsReimport = true;
                }

                // 设置Pixels Per Unit
                if (importer.spritePixelsPerUnit != 100)
                {
                    importer.spritePixelsPerUnit = 100;
                    needsReimport = true;
                }

                // 设置Filter Mode
                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    needsReimport = true;
                }

                // 设置压缩格式
                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                if (platformSettings.format != TextureImporterFormat.RGBA32)
                {
                    platformSettings.format = TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(platformSettings);
                    needsReimport = true;
                }

                // 启用Alpha透明
                if (importer.alphaIsTransparency != true)
                {
                    importer.alphaIsTransparency = true;
                    needsReimport = true;
                }

                if (needsReimport)
                {
                    importer.SaveAndReimport();
                    fixedCount++;
                    Debug.Log($"[SpriteImportFixer] 修复: {assetPath}");
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImportFixer] 完成! 共修复 {fixedCount} 个Sprite导入设置");
        EditorUtility.DisplayDialog("Sprite Import Fixer", $"修复完成!\n共修复 {fixedCount} 个Sprite", "确定");
    }

    [MenuItem("MoShou/Fix Sprite Imports (Selected)")]
    public static void FixSelectedSpriteImports()
    {
        Object[] selectedObjects = Selection.objects;
        int fixedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!assetPath.EndsWith(".png") && !assetPath.EndsWith(".jpg")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;

            importer.SaveAndReimport();
            fixedCount++;
            Debug.Log($"[SpriteImportFixer] 修复: {assetPath}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImportFixer] 完成! 共修复 {fixedCount} 个选中的Sprite");
    }
}
