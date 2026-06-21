using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace CasLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;
    public static bool LOADED = false;
    public static bool CAN_LOAD = true;
    public static new ManualLogSource Logger;
    public static Harmony harmony;
    public static CasLibRegistries REGISTRIES;
    public static System.Random globrng = new System.Random();
        
    private void Awake()
    {
        instance = this;
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Library {MyPluginInfo.PLUGIN_GUID} loading!");
        DoActualLoad();
        Logger.LogInfo($"Library {MyPluginInfo.PLUGIN_GUID} loaded!");
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }
    private void DoActualLoad()
    {
        REGISTRIES = new CasLibRegistries();
        Plugin.Logger.LogWarning("Debug item is registered!");
        REGISTRIES.RegisterItem(new CasLibItem("caslib_demoItem",new ItemInfo
        {
            category = "container",
            slotRotation = 0f,
            usable = true,
            usableOnLimb = false,
            destroyAtZeroCondition = true,
            // wearable = true,
            desiredWearLimb = "UpTorso",
            wearSlotId = "back",
            decayMinutes = 60f,
            // decayInfo = FlagsDecayInfo.DECAY_WHEN_FILLED,
            weight = 10000f,
            // wearableIsolation = 0.05f,
            // wearableVisualOffset = 5,
            value = 60,
            qualities = new List<CraftingQuality>
            {
                new CraftingQuality(CraftingQualitys.RIPPABLE,2f)
            },
            rec = new Recognition(5),
            combineable = false,
            scaleWeightWithCondition = false,
            useAction = delegate(Body body, Item item)
            {
                item.condition -= 0.25f;
                body.talker.Talk("testing text");
            }
        },(string id) => {
            GameObject go = (GameObject)Resources.Load("duffelbag");
            if (go.GetComponent<Item>() == null) go.AddComponent<Item>();
            go.GetComponent<Item>().id = id;
            go.GetComponent<Item>().name = id;
            go.AddComponent<Container>();
            go.GetComponent<Container>().maxWeight = 50;
            return go;
        }));
    }
}