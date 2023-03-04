// System
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

// BepInEx
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

// Game specific
using RG;
using RG.Scripts;
using ADV;
using Illusion.Unity;
using HDataClass;
using Illusion.Extensions;
using RG.Scene.Action;
using BepInEx.Configuration;

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

        internal static ConfigEntry<bool> EnableConfig;
        internal static new ManualLogSource Log;

        static bool isAdvDumped = false;
        static bool isSubtitleDumped = false;
        static int topicNumber = 0;

        static string[] header = {"//",
                                  "// Dumped With RG_TextDump v" + Version,
                                  "//" };

        public override void Load()
        {
            EnableConfig = Config.Bind("General",
                     "Enable TextDump",
                     false,
                     "Reload the game to Enable/Disable");

            Log = base.Log;

            if (EnableConfig.Value)
            {
                SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(StartDump));
            }
        }

        // Start dumping when Title Scene loads
        private void StartDump(Scene scene, LoadSceneMode lsm)
        {
            if (scene.name != "Title") return;

            Log.LogMessage("Dumping ADV");
            DumpADV();
            
            Log.LogMessage("Dumping H-Scene Subtitles");
            DumpHSubtitles();
            
            Log.LogMessage("Dumping Action Subtitles");
            Harmony.CreateAndPatchAll(typeof(DumpActionSubtitles), GUID);

            // Not finished
            Log.LogMessage("Dumping Topics");
            Harmony.CreateAndPatchAll(typeof(DumpTopics), GUID);
        }

        private void DumpADV()
        {
            {
                if (isAdvDumped) return;

                // Using List instead of HashSet because order matters
                List<string> advList = new List<string>(header);

                // Getting all bundles (.unity3d files) from folder
                var bundleList = CommonLib.GetAssetBundleNameListFromPath("adv\\scenario", subdirCheck: true);
                string currentText;
                bool hasText;

                foreach (string bundle in bundleList)
                {
                    hasText = false;

                    // Getting all assets from bundle that are the type ScenarioData
                    ScenarioData[] allAssets = AssetBundleManager.LoadAllAsset(bundle, UnhollowerRuntimeLib.Il2CppType.Of<ScenarioData>()).GetAllAssets<ScenarioData>();
                    foreach (ScenarioData scenarioData in allAssets)
                    {
                        //hasText = false;
                        foreach (ScenarioData.Param param in scenarioData.list)
                        {
                            var args = param.Args;
                            for (int j = 0; j < args.Length; j++)
                            {
                                // If current line is [H] and the following line contains japanese characters
                                if (args[j].StartsWith("[H]"))
                                {
                                    currentText = args[j + 1];
                                    if (Regex.IsMatch(currentText, "[一-龠]+|[ぁ-ゔ]+|[ァ-ヴー]+|[々〆〤ヶ]+"))
                                    {
                                        hasText = true;

                                        // Fix when Illusion adds newline that don't play well with xUnity Autotranslator.
                                        currentText = currentText.Replace("\n", "\\n");
                                        // add comment in the beginning and = in the end
                                        currentText = "//" + currentText + "=";
                                        if (!advList.Contains(currentText))
                                        {
                                            advList.Add(currentText);
                                        }
                                    }
                                }
                            }
                        }
                        // One folder per asset
                        //if (hasText)
                        //{
                        //    string path = "TextDump\\" + Path.GetDirectoryName(bundle) + "\\" + Path.GetFileNameWithoutExtension(bundle) + "\\" + scenarioData.name;
                        //    path = path + "\\translation.txt";
                        //    WriteFile(path, advList);
                        //    advList.Clear();
                        //}
                    }
                    // One folder per bundle
                    if (hasText)
                    {
                        string path = "TextDump\\" + "ADV\\" + "abdata\\" + Path.GetDirectoryName(bundle) + "\\" + Path.GetFileNameWithoutExtension(bundle);
                        path = path + "\\translation.txt";
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.WriteAllLines(path, advList, Encoding.UTF8);
                        advList.Clear();
                    }
                    AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
                }

                // One big file for everything
                //WriteFile(advFile, advList);
                isAdvDumped = true;
            }
        }

        private void DumpHSubtitles()
        {
            if (isSubtitleDumped) return;

            List<string> hSubtitleList = new List<string>(header);

            // Getting all bundles (.unity3d files) from folder
            var bundleList = CommonLib.GetAssetBundleNameListFromPath("list\\h\\sound\\voice", subdirCheck: true);
            string currentText;
            bool hasText;

            foreach (string bundle in bundleList)
            {
                // Getting all assets from bundle that are the type VoiceData
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

                                // Fix when Illusion adds newline that don't play well with xUnity Autotranslator.
                                currentText = currentText.Replace("\n", "\\n");
                                // add comment in the beginning and = in the end
                                currentText = "//" + currentText + "=";
                                if (!hSubtitleList.Contains(currentText))
                                {
                                    hSubtitleList.Add(currentText);
                                }
                            }
                    // One folder per asset
                    if (hasText)
                    {
                        string filePath = "TextDump\\" + "H_Subtitles\\" + "abdata\\" + Path.GetDirectoryName(bundle) + "\\" + Path.GetFileNameWithoutExtension(bundle) + "\\" + voiceData.name;
                        Directory.CreateDirectory(filePath);

                        filePath = filePath + "\\translation.txt";
                        File.WriteAllLines(filePath, hSubtitleList, Encoding.UTF8);
                        hSubtitleList.Clear();
                    }
                }
                AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
            }
            // One big file for everything
            //WriteFile(subtitleFile, hSubtitleList);
            isSubtitleDumped = true;
        }

        public static class DumpActionSubtitles
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionSubtitleTable), nameof(ActionSubtitleTable.Load))]
            public static void DumpAction(Define.TableData.Category category, ActionSubtitleTable __instance)
            {
                string path = "TextDump\\Action_Subtitles\\translation.txt";

                // Using List instead of HashSet because order matters
                List<string> actionSubtitles;

                if (File.Exists(path))
                {
                    actionSubtitles = File.ReadAllLines(path).ToList();
                }
                else
                {
                    actionSubtitles = new List<string>(header);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                // Including asset name
                actionSubtitles.Add("\r\n//" + category.ToString() + "\r\n");
                Debug.Log("Dumping " + category.ToString());

                var instance = __instance._instance;

                // Behold Illusion recursion insanity: indexes from i to m (5 levels)
                for (int i = 0; i < instance.Count; i++)
                {
                    var dic = instance.GetElement(i).value;
                    for (int j = 0; j < dic.Count; j++)
                    {
                        var element = dic.GetElement(j);

                        // First array
                        var arrayText = element.AryTextA;
                        AddAllArray(arrayText);

                        // Second array
                        arrayText = element.AryTextB;
                        AddAllArray(arrayText);
                    }
                }
                void AddAllArray(UnhollowerBaseLib.Il2CppReferenceArray<UnhollowerBaseLib.Il2CppReferenceArray<UnhollowerBaseLib.Il2CppStringArray>> arrayText)
                {
                    for (int k = 0; k < arrayText.Length; k++)
                    {
                        var first = arrayText[k];
                        for (int l = 0; l < first.Length; l++)
                        {
                            var second = first[l];
                            for (int m = 0; m < second.Length; m++)
                            {
                                string subtitle = second[m];
                                if (string.IsNullOrEmpty(subtitle)) continue;

                                // Fix when Illusion adds newline that don't play well with xUnity Autotranslator.
                                subtitle = subtitle.Replace("\n", "\\n");
                                // add comment in the beginning and = in the end
                                subtitle = "//" + subtitle + "=";
                                if (!actionSubtitles.Contains(subtitle))
                                {
                                    actionSubtitles.Add(subtitle);
                                }
                            }
                        }
                    }
                }
                File.WriteAllLines(path, actionSubtitles);
            }
        }

        public static class DumpTopics
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TopicTableData.TopicTableInfo), nameof(TopicTableData.TopicTableInfo.Set))]
            public static void Topics(TopicTableData.TopicTableInfo __instance)
            {
                List<string> topicsList;
                string path = "TextDump\\Topics\\translation.txt";

                if (File.Exists(path))
                {
                    topicsList = File.ReadAllLines(path).ToList();
                }
                else
                {
                    topicsList = new List<string>(header);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                topicNumber++;
                Debug.Log("Topic number: " + topicNumber);

                var voiceText = __instance.VoiceText;
                if (string.IsNullOrEmpty(voiceText)) return;

                // Fix when Illusion adds newline that don't play well with xUnity Autotranslator.
                voiceText = voiceText.Replace("\n", "\\n");
                // add comment in the beginning and = in the end
                voiceText = "//" + voiceText + "=";
                if (!topicsList.Contains(voiceText))
                {
                    topicsList.Add(voiceText);
                }

                File.WriteAllLines(path, topicsList);
            }
        }
    }
}
