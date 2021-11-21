using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UltimateSurvival.StandardAssets;
using UnityEngine;
using UnityEngine.UI;

namespace StarsandHitpoints
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StarsandHitpoints : BaseUnityPlugin
    {
        public new static ManualLogSource Logger;
        // private static float lastPixelHeight = -1;
        private void Awake()
        {
            Logger = new ManualLogSource("Hitpoints");
            BepInEx.Logging.Logger.Sources.Add(Logger);
            Logger.LogInfo("Starsand Hitpoints Mod Loading");
            Harmony.CreateAndPatchAll(typeof(StarsandHitpoints));
        }

        public static Dictionary<int, GameObject> dHitpointCounters = new Dictionary<int, GameObject>();
        public static Dictionary<int, RectTransform> dHitpointBars = new Dictionary<int, RectTransform>();
        private static void Setup(EnemyAI instance)
        {
            GameObject dFeedCounter = new GameObject();

            dFeedCounter.transform.SetParent(instance.transform, false);
            

            // MeshRenderer dFeedCounterMR = dFeedCounter.AddComponent<MeshRenderer>();
            // TextMesh dFeedCounterTM = dFeedCounter.AddComponent<TextMesh>();
            Canvas dCanvas = dFeedCounter.AddComponent<Canvas>();
            dCanvas.GetComponent<RectTransform>().localPosition += Vector3.up * 4f;
            Logger.LogInfo(dCanvas.GetComponent<RectTransform>().position);
            GameObject BackgroundObj = new GameObject();
            BackgroundObj.name = "Background";
            BackgroundObj.transform.SetParent(dCanvas.transform, false);
            GameObject ForegroundObj = new GameObject();
            ForegroundObj.name = "Foreground";
            ForegroundObj.transform.SetParent(BackgroundObj.transform, false);
            Image BG = BackgroundObj.AddComponent<Image>();
            BG.color = Color.black;
            RectTransform BGR = BackgroundObj.GetComponent<RectTransform>();
            BGR.sizeDelta = new Vector2(100f, 5f);
            BGR.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Image FG = ForegroundObj.AddComponent<Image>();
            FG.color = Color.red;
            RectTransform FGR = ForegroundObj.GetComponent<RectTransform>();
            FGR.sizeDelta = new Vector2(100f, 4f);
            FGR.anchoredPosition = new Vector2(1f,0f);
            FGR.pivot = Vector2.zero;
            FGR.anchorMax = FGR.anchorMin = Vector2.zero;
            FGR.anchoredPosition3D = Vector3.one;
            dHitpointBars.Add(instance.GetInstanceID(), FGR);
            // FGR.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            
            // if(ab != null)
            // {
            //     font = ab.LoadAsset<Font>("Assets/Assets/Font/Komon.ttf");
            //     if(font != null)
            //     {
            //         dFeedCounterTM.font = font;
            //     }
            //     else
            //     {
            //         Debug.Log("FAILED TO LOAD FONT FROM ASSET BUNDLE");
            //     }
            // }
            // else
            // {
            //     Debug.Log("FAILED TO LOAD ASSET BUNDLE");
            // }

            // dFeedCounterTM.text = "Loading...";
            // dFeedCounterTM.anchor = TextAnchor.UpperCenter;

            dFeedCounter.SetActive(true);

            dHitpointCounters.Add(instance.GetInstanceID(), dFeedCounter);
            // lastPixelHeight = -1; // trigger resize of all instances
        }
             
        // private static void Resize()
        // {
        //     float ph = Camera.main.pixelHeight;
        //     float ch = Camera.main.orthographicSize;
        //     float pixelRatio = (ch * 0.7f) / ph;
        //     float targetRes = 128f;
        //     // foreach(KeyValuePair<int, GameObject> entry in dHitpointCounters)
        //     // {
        //     //     // TextMesh dHitpointCounterTM = entry.Value.GetComponent<TextMesh>();
        //     //     //
        //     //     // dHitpointCounterTM.characterSize = pixelRatio * Camera.main.orthographicSize / Math.Max(entry.Value.transform.localScale.x, entry.Value.transform.localScale.y);
        //     //     // dHitpointCounterTM.fontSize = (int)Math.Round(targetRes / dHitpointCounterTM.characterSize);
        //     // }
        //     lastPixelHeight = ph;
        // }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoaderButton), "StartLoad")]
        public static void StartLoad()
        {
            dHitpointCounters.Clear();
            dHitpointBars.Clear();
        }        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoaderButton), "StartNew")]
        public static void StartNew()
        {
            dHitpointCounters.Clear();
            dHitpointBars.Clear();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "HideInGround")]
        public static void HideInGround(EnemyAI __instance)
        {
            dHitpointCounters[__instance.GetInstanceID()].SetActive(false);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "onDeath")]
        public static void onDeath(EnemyAI __instance)
        {
            GameObject.Destroy(dHitpointCounters[__instance.GetInstanceID()]);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), "Update")]
        public static bool Update_Prefix(EnemyAI __instance)
        {
            if (!dHitpointCounters.ContainsKey(__instance.GetInstanceID()) && __instance.aiSettings.enabled)
            {
                Setup(__instance);
            }
            else if (__instance.aiSettings.enabled && dHitpointCounters.TryGetValue(__instance.GetInstanceID(), out GameObject dHitPointCounter))
            {
                dHitPointCounter.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
                // if(Camera.main.pixelHeight != lastPixelHeight)
                // {
                //     Resize();
                // }

                // TextMesh dFeedCounterTM = dFeedCounter.GetComponent<TextMesh>();
                RectTransform FGR = dHitpointBars[__instance.GetInstanceID()];
                // if (di.gameObject.GetComponentInChildren<Image>() != null) di = di.gameObject.GetComponentInChildren<Image>();
                FGR.sizeDelta = new Vector2(__instance.aiSettings.AI.Health.m_CurrentValue, 4f);
                dHitPointCounter.SetActive(__instance.agent.enabled);
                // var s = "";
                // var n = (int)(__instance.aiSettings.AI.Health.m_CurrentValue/13.5f) +1;
                // if (n > 0 && __instance.agent.enabled)
                // {
                //     for (int i = 0; i < n; i++)
                //     {
                //         s += "|";
                //
                //     }
                //
                //     dFeedCounterTM.text = s;
                // }
                // else 
                // dFeedCounterTM.text = "";
                HideOnDistance(dHitPointCounter);
            }

            return true;
        }
        
        private static void HideOnDistance(GameObject dHitpointCounter)
        {
            float distanceFromCamera = Vector3.Distance(dHitpointCounter.transform.position, EnviroSky.instance.PlayerCamera.transform.position);
            if(distanceFromCamera > 5f)
            {
                dHitpointCounter.SetActive(false);
            }
            else
            {
                dHitpointCounter.SetActive(true);
            }
        }
       

    }
}