// System
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// BepInEx
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;

// Unity
using UnityEngine.SceneManagement;

// Game specific
using ADV;
using Illusion.Unity;
using HDataClass;
using System.Linq;

namespace RG_TextDump
{

    [BepInProcess("RoomGirl")]
    [BepInPlugin(GUID, PluginName, Version)]
    public class RG_TextDump : BasePlugin
    {
        // Plugin consts
        public const string GUID = "SpockBauru.RG_TextDump";
        public const string PluginName = "RG_TextDump";
        public const string Version = "0.1";

        internal static new ManualLogSource Log;

        static HashSet<string> subtitleHash = new HashSet<string>();
        static HashSet<string> advHash = new HashSet<string>();
        static bool isAdvDumped = false;
        static bool isSubtitleDumped = false;

        string[] header = {"//",
                           "// Dumped With RG_TextDump v" + Version,
                           "//" };

        public override void Load()
        {
            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(StartDump));
            Log = base.Log;
        }

        private void StartDump(Scene scene, LoadSceneMode lsm)
        {
            if (scene.name != "Title") return;



            Log.LogMessage("Dumping ADV");
            DumpADV();
            Log.LogMessage("Dumping Subtitles");
            DumpSubtitles();
            Log.LogMessage("Dump Finalized");
        }

        private void DumpADV()
        {
            {
                if (isAdvDumped) return;
                //ReadTranslation(advFile, advHash);

                var bundleList = CommonLib.GetAssetBundleNameListFromPath("adv\\scenario", subdirCheck: true);
                string currentText;
                bool hasText = false;

                foreach (string bundle in bundleList)
                {
                    hasText = false;
                    ScenarioData[] allAssets = AssetBundleManager.LoadAllAsset(bundle, UnhollowerRuntimeLib.Il2CppType.Of<ScenarioData>()).GetAllAssets<ScenarioData>();
                    foreach (ScenarioData scenarioData in allAssets)
                    {
                        hasText = false;
                        foreach (ScenarioData.Param param in scenarioData.list)
                        {
                            var args = param.Args;
                            for (int j = 0; j < args.Length; j++)
                            {
                                // If this line is [H] and the following line contains japanese characters
                                if (args[j].StartsWith("[H]"))
                                {
                                    currentText = args[j + 1];
                                    if (Regex.IsMatch(currentText, "[一-龠]+|[ぁ-ゔ]+|[ァ-ヴー]+|[々〆〤ヶ]+"))
                                    {
                                        hasText = true;
                                        if (!advHash.Contains(currentText))
                                        {
                                            advHash.Add(currentText);
                                        }

                                    }
                                }
                            }
                        }
                        // One folder per asset
                        if (hasText)
                        {
                            string path = "TextDump\\" + Path.GetDirectoryName(bundle) + "\\" + Path.GetFileNameWithoutExtension(bundle) + "\\" + scenarioData.name;
                            path = path + "\\translation.txt";
                            WriteFile(path, advHash);
                            advHash.Clear();
                        }
                    }
                    // One folder per bundle
                    //if (hasText)
                    //{
                    //    string path = "TextDump\\" + Path.GetDirectoryName(bundle) + "\\" + Path.GetFileNameWithoutExtension(bundle);
                    //    Debug.Log(path);
                    //    path = path + "\\translation.txt";
                    //    WriteFile(path, advHash);
                    //    advHash.Clear();
                    //}
                    AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
                }

                // One big file for everything
                //WriteFile(advFile, advHash);
                isAdvDumped = true;
            }
        }

        private void DumpSubtitles()
        {
            if (isSubtitleDumped) return;
            //ReadTranslation(advFile, advHash);

            // .unity3d files
            var bundleList = CommonLib.GetAssetBundleNameListFromPath("..\\abdata\\list\\h\\sound\\voice", subdirCheck: true);
            string currentText;
            bool hasText;

            foreach (string bundle in bundleList)
            {
                VoiceData[] allAssets = AssetBundleManager.LoadAllAsset(bundle, UnhollowerRuntimeLib.Il2CppType.Of<VoiceData>()).GetAllAssets<VoiceData>();

                foreach (VoiceData voiceData in allAssets)
                {
                    hasText = false;
                    foreach (VoiceData.Param param in voiceData.Params)
                        foreach (VoiceData.Info info in param.Infos)
                            foreach (VoiceData.InfoDetail infoDetail in info.Details)
                            {
                                currentText = infoDetail.Word;
                                hasText = true;
                                if (!subtitleHash.Contains(currentText))
                                {
                                    subtitleHash.Add(currentText);
                                }
                            }
                    // One folder per asset
                    if (hasText)
                    {
                        string filePath = Regex.Replace(bundle, "^\\.\\.", "");
                        filePath = "TextDump" + Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "\\" + voiceData.name;
                        //Directory.CreateDirectory(path);

                        filePath = filePath + "\\translation.txt";
                        WriteFile(filePath, subtitleHash);
                        subtitleHash.Clear();
                    }
                }
                AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
            }
            // One big file for everything
            //WriteFile(subtitleFile, subtitleHash);
            isSubtitleDumped = true;
        }

        void ReadTranslation(string path, HashSet<string> hashSet)
        {
            if (!File.Exists(path)) return;
            string[] allLines = File.ReadAllLines(path);
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = Regex.Replace(allLines[i], "^//", "");
                allLines[i] = Regex.Replace(allLines[i], "=$", "");
            }
            hashSet.Clear();
            hashSet.UnionWith(allLines);
        }

        void WriteFile(string path, HashSet<string> hashSet)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string[] stringArray = new string[hashSet.Count];
            hashSet.CopyTo(stringArray);
            for (int i = 0; i < stringArray.Length; i++)
            {
                stringArray[i] = stringArray[i].Replace("\n", "\\n");
                stringArray[i] = "//" + stringArray[i] + "=";
            }

            string[] fileContent = header.Concat(stringArray).ToArray();

            File.WriteAllLines(path, fileContent, Encoding.UTF8);
        }
    }
}
