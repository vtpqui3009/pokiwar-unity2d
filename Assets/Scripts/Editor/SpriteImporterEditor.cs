#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pokiwar.Evolution;

namespace Pokiwar.Editor
{
    /// <summary>
    /// Editor utility to batch import pet icons as sprites with proper settings.
    ///
    /// Expected filename conventions:
    ///   breath-images: {petId}_breath_{frameNumber}.png  (e.g. 001_breath_0.png, 001_breath_1.png)
    ///                  OR: {petId}_{frameNumber}.png     (e.g. 001_0.png, 001_1.png)
    ///                  OR: {petId}.png                   (single frame, e.g. 001.png)
    ///   large-images:  {petId}_large.png                 (e.g. 001_large.png)
    ///                  OR: {petId}.png                   (e.g. 001.png)
    ///
    /// Tier assignment based on pet ID numeric value:
    ///   001-100:  Baby
    ///   101-300:  Basic
    ///   301-600:  Stage1
    ///   601-1000: Stage2
    ///   1001+:    Mega
    /// </summary>
    public class SpriteImporterEditor : EditorWindow
    {
        private string breathImagesPath = "breath-images";
        private string largeImagesPath = "large-images";
        private string destinationPath = "Assets/Sprites/Pets";
        private string petSpriteMapperPath = "Assets/Sprites/Pets/PetSpriteMapper.asset";

        private int pixelsPerUnit = 32;
        private SpriteMeshType meshType = SpriteMeshType.Tight;
        private FilterMode filterMode = FilterMode.Point;

        private Vector2 scrollPosition;
        private string statusMessage = "";

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
            EditorGUILayout.LabelField("Source Paths (relative to project root)", EditorStyles.boldLabel);
            breathImagesPath = EditorGUILayout.TextField("Breath Images Path:", breathImagesPath);
            largeImagesPath = EditorGUILayout.TextField("Large Images Path:", largeImagesPath);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Destination", EditorStyles.boldLabel);
            destinationPath = EditorGUILayout.TextField("Destination Path:", destinationPath);
            petSpriteMapperPath = EditorGUILayout.TextField("PetSpriteMapper Asset:", petSpriteMapperPath);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);
            pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit:", pixelsPerUnit);
            meshType = (SpriteMeshType)EditorGUILayout.EnumPopup("Mesh Type:", meshType);
            filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", filterMode);

            GUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Import Breath Images", GUILayout.Height(30)))
                ImportImages(breathImagesPath, "Breath");

            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Import Large Images", GUILayout.Height(30)))
                ImportImages(largeImagesPath, "Large");

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Import All + Build PetSpriteMapper", GUILayout.Height(40)))
            {
                ImportImages(breathImagesPath, "Breath");
                ImportImages(largeImagesPath, "Large");
                BuildPetSpriteMapper();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);

            if (GUILayout.Button("Build PetSpriteMapper Only"))
                BuildPetSpriteMapper();

            if (GUILayout.Button("Create Legacy SpriteDatabase"))
                CreateLegacySpriteDatabase();

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ImportImages(string sourcePath, string folderName)
        {
            string fullSourcePath = Path.Combine(Application.dataPath, "..", sourcePath);

            if (!Directory.Exists(fullSourcePath))
            {
                EditorUtility.DisplayDialog("Error", $"Source path not found:\n{fullSourcePath}", "OK");
                return;
            }

            string fullDestPath = Path.Combine(Application.dataPath, "Sprites", "Pets", folderName);
            if (!Directory.Exists(fullDestPath))
            {
                Directory.CreateDirectory(fullDestPath);
                AssetDatabase.Refresh();
            }

            string[] files = Directory.GetFiles(fullSourcePath, "*.png");
            EditorUtility.DisplayProgressBar("Importing Sprites", $"Importing {files.Length} images...", 0);

            int importedCount = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(fullDestPath, fileName);

                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile);
                    importedCount++;
                }

                float progress = (float)(i + 1) / files.Length;
                EditorUtility.DisplayProgressBar("Importing Sprites", $"Processing {fileName}...", progress);
            }

            AssetDatabase.Refresh();

            // Configure all sprites in the folder
            string[] allFiles = Directory.GetFiles(fullDestPath, "*.png");
            for (int i = 0; i < allFiles.Length; i++)
            {
                ConfigureSpriteImport(allFiles[i]);
                EditorUtility.DisplayProgressBar("Configuring Sprites", $"Configuring {Path.GetFileName(allFiles[i])}...", (float)(i + 1) / allFiles.Length);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            statusMessage = $"Imported {importedCount} new sprites to {folderName}. Total: {allFiles.Length}";
            Debug.Log(statusMessage);
        }

        private void ConfigureSpriteImport(string fullPath)
        {
            string relativePath = "Assets" + fullPath.Replace(Application.dataPath, "").Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.spriteMeshType = meshType;
                importer.filterMode = filterMode;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// Builds the PetSpriteMapper asset by scanning breath and large image folders,
        /// grouping sprites by pet ID extracted from filenames.
        /// </summary>
        private void BuildPetSpriteMapper()
        {
            string breathFolder = Path.Combine("Assets/Sprites/Pets", "Breath");
            string largeFolder = Path.Combine("Assets/Sprites/Pets", "Large");

            // Collect breath sprites grouped by pet ID
            Dictionary<string, List<Sprite>> breathByPetId = new Dictionary<string, List<Sprite>>();
            if (AssetDatabase.IsValidFolder(breathFolder))
            {
                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { breathFolder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) continue;

                    string petId = ExtractPetId(Path.GetFileNameWithoutExtension(path));
                    if (string.IsNullOrEmpty(petId)) continue;

                    if (!breathByPetId.ContainsKey(petId))
                        breathByPetId[petId] = new List<Sprite>();

                    breathByPetId[petId].Add(sprite);
                }
            }

            // Sort breath frames by filename
            foreach (var kvp in breathByPetId)
            {
                kvp.Value.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            }

            // Collect large sprites by pet ID
            Dictionary<string, Sprite> largeByPetId = new Dictionary<string, Sprite>();
            if (AssetDatabase.IsValidFolder(largeFolder))
            {
                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { largeFolder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) continue;

                    string petId = ExtractPetId(Path.GetFileNameWithoutExtension(path));
                    if (!string.IsNullOrEmpty(petId))
                        largeByPetId[petId] = sprite;
                }
            }

            // Merge all pet IDs
            HashSet<string> allPetIds = new HashSet<string>(breathByPetId.Keys);
            allPetIds.UnionWith(largeByPetId.Keys);

            if (allPetIds.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning",
                    "No sprites found in Breath or Large folders.\nImport images first.", "OK");
                return;
            }

            // Load or create PetSpriteMapper asset
            PetSpriteMapper mapper = AssetDatabase.LoadAssetAtPath<PetSpriteMapper>(petSpriteMapperPath);
            if (mapper == null)
            {
                mapper = ScriptableObject.CreateInstance<PetSpriteMapper>();
                string dir = Path.GetDirectoryName(petSpriteMapperPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(mapper, petSpriteMapperPath);
            }

            mapper.Clear();

            int count = 0;
            foreach (string petId in allPetIds)
            {
                PetSpriteData data = new PetSpriteData
                {
                    petId = petId,
                    breathFrames = breathByPetId.TryGetValue(petId, out List<Sprite> frames)
                        ? frames.ToArray()
                        : new Sprite[0],
                    largeSprite = largeByPetId.TryGetValue(petId, out Sprite large) ? large : null,
                    tier = GetTierForPetId(petId),
                    rarity = GetRarityForPetId(petId)
                };

                mapper.AddOrUpdatePet(data);
                count++;
            }

            EditorUtility.SetDirty(mapper);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            statusMessage = $"Built PetSpriteMapper with {count} pets at {petSpriteMapperPath}";
            Debug.Log(statusMessage);
        }

        /// <summary>
        /// Extracts the pet ID from a filename.
        /// Handles patterns like: 001, 001_breath_0, 001_0, 001_large
        /// </summary>
        private string ExtractPetId(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            // Remove known suffixes
            filename = Regex.Replace(filename, @"_breath_\d+$", "", RegexOptions.IgnoreCase);
            filename = Regex.Replace(filename, @"_large$", "", RegexOptions.IgnoreCase);
            filename = Regex.Replace(filename, @"_\d+$", "");

            // Extract leading numeric or alphanumeric ID
            Match match = Regex.Match(filename, @"^([a-zA-Z0-9]+)");
            return match.Success ? match.Groups[1].Value : filename;
        }

        /// <summary>
        /// Assigns evolution tier based on numeric pet ID.
        /// 001-100: Baby, 101-300: Basic, 301-600: Stage1, 601-1000: Stage2, 1001+: Mega
        /// </summary>
        private EvolutionTier GetTierForPetId(string petId)
        {
            if (int.TryParse(petId, out int id))
            {
                if (id <= 100)  return EvolutionTier.Baby;
                if (id <= 300)  return EvolutionTier.Basic;
                if (id <= 600)  return EvolutionTier.Stage1;
                if (id <= 1000) return EvolutionTier.Stage2;
                return EvolutionTier.Mega;
            }
            return EvolutionTier.Baby;
        }

        /// <summary>
        /// Assigns rarity based on numeric pet ID (every 5th is rare, every 20th is epic, etc.)
        /// </summary>
        private SpriteRarity GetRarityForPetId(string petId)
        {
            if (int.TryParse(petId, out int id))
            {
                if (id % 100 == 0) return SpriteRarity.Legendary;
                if (id % 20 == 0)  return SpriteRarity.Epic;
                if (id % 5 == 0)   return SpriteRarity.Rare;
                if (id % 2 == 0)   return SpriteRarity.Uncommon;
                return SpriteRarity.Common;
            }
            return SpriteRarity.Common;
        }

        /// <summary>
        /// Creates the legacy flat SpriteDatabase for backward compatibility.
        /// </summary>
        private void CreateLegacySpriteDatabase()
        {
            string dbPath = "Assets/Sprites/Pets/SpriteDatabase.asset";
            SpriteDatabase database = AssetDatabase.LoadAssetAtPath<SpriteDatabase>(dbPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<SpriteDatabase>();
                AssetDatabase.CreateAsset(database, dbPath);
            }

            List<Sprite> sprites = new List<Sprite>();
            LoadSpritesFromFolder("Assets/Sprites/Pets/Breath", sprites);
            LoadSpritesFromFolder("Assets/Sprites/Pets/Large", sprites);

            // Use reflection to set private field
            var field = database.GetType().GetField("petSprites",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(database, sprites.ToArray());

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            statusMessage = $"Created legacy SpriteDatabase with {sprites.Count} sprites";
            Debug.Log(statusMessage);
        }

        private void LoadSpritesFromFolder(string folderPath, List<Sprite> sprites)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) return;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                    sprites.Add(sprite);
            }
        }
    }
}
#endif
