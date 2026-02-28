#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Pokiwar.Editor
{
    /// <summary>
    /// Editor utility to batch import pet icons as sprites with proper settings.
    /// </summary>
    public class SpriteImporterEditor : EditorWindow
    {
        [Header("Source Paths")]
        private string breathImagesPath = "breath-images";
        private string largeImagesPath = "large-images";

        [Header("Destination")]
        private string destinationPath = "Assets/Sprites/Pets";

        [Header("Import Settings")]
        private int pixelsPerUnit = 32;
        private SpriteMeshType meshType = SpriteMeshType.Tight;
        private FilterMode filterMode = FilterMode.Point;

        private Vector2 scrollPosition;

        [MenuItem("Pokiwar/Sprite Importer")]
        public static void ShowWindow()
        {
            GetWindow<SpriteImporterEditor>("Pokiwar Sprite Importer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Pokiwar Sprite Importer", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Source Paths", EditorStyles.boldLabel);
            breathImagesPath = EditorGUILayout.TextField("Breath Images Path:", breathImagesPath);
            largeImagesPath = EditorGUILayout.TextField("Large Images Path:", largeImagesPath);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Destination", EditorStyles.boldLabel);
            destinationPath = EditorGUILayout.TextField("Destination Path:", destinationPath);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);
            pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit:", pixelsPerUnit);
            meshType = (SpriteMeshType)EditorGUILayout.EnumPopup("Mesh Type:", meshType);
            filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", filterMode);

            GUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Import Breath Images", GUILayout.Height(30)))
            {
                ImportImages(breathImagesPath, "Breath");
            }

            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Import Large Images", GUILayout.Height(30)))
            {
                ImportImages(largeImagesPath, "Large");
            }

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Import All", GUILayout.Height(35)))
            {
                ImportImages(breathImagesPath, "Breath");
                ImportImages(largeImagesPath, "Large");
            }

            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            if (GUILayout.Button("Create Sprite Database"))
            {
                CreateSpriteDatabase();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ImportImages(string sourcePath, string folderName)
        {
            string fullSourcePath = Path.Combine(Application.dataPath, "..", sourcePath);

            if (!Directory.Exists(fullSourcePath))
            {
                EditorUtility.DisplayDialog("Error", $"Source path not found: {fullSourcePath}", "OK");
                return;
            }

            string destPath = Path.Combine(destinationPath, folderName);
            string fullDestPath = Path.Combine(Application.dataPath, "Sprites", "Pets", folderName);

            if (!Directory.Exists(fullDestPath))
            {
                Directory.CreateDirectory(fullDestPath);
                AssetDatabase.Refresh();
            }

            string[] files = Directory.GetFiles(fullSourcePath, "*.png");

            EditorUtility.DisplayProgressBar("Importing Sprites", $"Importing {files.Length} images...", 0);

            int importedCount = 0;
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(fullDestPath, fileName);

                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile);
                    importedCount++;
                }

                ConfigureSpriteImport(destFile, importedCount);

                float progress = (float)importedCount / files.Length;
                EditorUtility.DisplayProgressBar("Importing Sprites", $"Importing {fileName}...", progress);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            Debug.Log($"Imported {importedCount} sprites from {folderName}");
        }

        private void ConfigureSpriteImport(string assetPath, int index)
        {
            string relativePath = "Assets" + assetPath.Replace(Application.dataPath, "");
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.spriteMeshType = meshType;
                importer.filterMode = filterMode;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveImporter();
            }
        }

        private void CreateSpriteDatabase()
        {
            string databasePath = Path.Combine(destinationPath, "SpriteDatabase.asset");
            string fullPath = Path.Combine(Application.dataPath, "Sprites", "Pets", "SpriteDatabase.asset");

            SpriteDatabase database = ScriptableObject.CreateInstance<SpriteDatabase>();

            List<Sprite> sprites = new List<Sprite>();

            string breathPath = Path.Combine(destinationPath, "Breath");
            string largePath = Path.Combine(destinationPath, "Large");

            LoadSpritesFromFolder(Path.Combine("Assets/Sprites/Pets", "Breath"), sprites);
            LoadSpritesFromFolder(Path.Combine("Assets/Sprites/Pets", "Large"), sprites);

            var fieldType = database.GetType().GetField("petSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldType != null)
            {
                fieldType.SetValue(database, sprites.ToArray());
            }

            if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }

            AssetDatabase.CreateAsset(database, "Assets/Sprites/Pets/SpriteDatabase.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created Sprite Database with {sprites.Count} sprites");
        }

        private void LoadSpritesFromFolder(string folderPath, List<Sprite> sprites)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
        }
    }
}
#endif
