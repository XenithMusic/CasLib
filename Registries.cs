using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace CasLib
{
    public static class CasLibItemPool
    {
        public static void AddToLootPool(CasLibItem item)
        {
            Dictionary<string,List<string>> pool = ItemLootPool.pool;
            if (pool == null) Plugin.Logger.LogWarning("UH OH! WHY IS THAT THE CASE???");
            if (item.vanillaItem.category == null) return;
            if (!pool.ContainsKey(item.vanillaItem.category))
            {
                pool.Add(item.vanillaItem.category, new List<string>());
            }
            pool[item.vanillaItem.category].Add(item.id);
            if (item.referenceObject == null) Plugin.Logger.LogWarning("it is null in the lootpool");
        }
        public static void AddItemInternal(CasLibItem item)
        {
            Item.GlobalItems.Add(item.id,item.vanillaItem);
            if (item.referenceObject == null) Plugin.Logger.LogWarning("it is null in the internal adder");
        }
    }
    public class CasLibRegistries
    {
        public Dictionary<string,CasLibItem> items;
        public CasLibRegistries()
        {
            items = new Dictionary<string,CasLibItem>();
        }
        public void RegisterItem(CasLibItem item)
        {
            Plugin.Logger.LogInfo($"Registering CasLibItem {item.id}...");
            items.Add(item.id,item);
            if (item.referenceObject == null) Plugin.Logger.LogWarning("it is null when i register it");
        }
    }
    [HarmonyPatch(typeof(ItemLootPool),nameof(ItemLootPool.InitializePool))]
    public class ItemLootPoolInitializeRegistryPatch
    {
        public static void Postfix()
        {
            Plugin.Logger.LogMessage("CasLib Registry updating loot pool!");
            foreach (CasLibItem item in Plugin.REGISTRIES.items.Values)
            {
                if (item.referenceObject == null) Plugin.Logger.LogWarning("null before itemlootpool");
                CasLibItemPool.AddToLootPool(item);
                if (item.referenceObject == null) Plugin.Logger.LogWarning("null after itemlootpool");
            }
        }
    }
    [HarmonyPatch(typeof(Item),nameof(Item.SetupItems))]
    public class GlobalItemsSetupRegistryPatch
    {
        public static void Postfix()
        {
            Plugin.Logger.LogMessage("CasLib Registry updating global items!");
            foreach(CasLibItem item in Plugin.REGISTRIES.items.Values)
            {
                if (item.referenceObject == null) Plugin.Logger.LogWarning("null before additeminternal");
                CasLibItemPool.AddItemInternal(item);
                if (item.referenceObject == null) Plugin.Logger.LogWarning("null after additeminternal");
            }
        }
    }
    // NOTE: i don't want to patch unity but i sorta had no choice;
    // Resources.Load and Resources.LoadAll are used so extensively that
    // if i don't patch them, it will take several hundred more patches
    // than just doing this.
    [HarmonyPatch(typeof(Resources))]
    [HarmonyPatch(nameof(Resources.Load),[typeof(string), typeof(Type)])]
    public class UnityResourcesLoadPatch
    {
        public static bool Prefix(string path, Type systemTypeInstance, ref UnityEngine.Object __result)
        {
            if (Plugin.REGISTRIES.items.ContainsKey(path))
            {
                __result = Plugin.REGISTRIES.items[path].referenceObject;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Resources))]
    [HarmonyPatch(nameof(Resources.LoadAll),[typeof(string), typeof(Type)])]
    public class UnityResourcesLoadAllPatch
    {
        public static void Postfix(string path,ref UnityEngine.Object[] __result)
        {
            Plugin.Logger.LogInfo(
                $"Object LoadAll hit: {path}, length={__result.Length}"
            );
            if (path == "") {
                int oldlen = __result.Length;
                List<UnityEngine.Object> list = __result.ToList();
                Plugin.Logger.LogInfo("Path is empty string, registry should be appended!");
                if (Plugin.REGISTRIES.items["collar"].referenceObject == null) Plugin.Logger.LogWarning("it is null in the registry");
                foreach (CasLibItem item in Plugin.REGISTRIES.items.Values)
                {
                    if (item.referenceObject == null) Plugin.Logger.LogWarning("item.referenceObject == null before i add it to the list");
                    list.Add(item.referenceObject);
                    if (item == null) Plugin.Logger.LogWarning("item == null");
                    if (item.referenceObject == null) Plugin.Logger.LogWarning("item.referenceObject == null");
                }
                __result = list.ToArray();
                Plugin.Logger.LogInfo($"Loaded all! (length {oldlen} => {__result.Length}, expected (length {oldlen} => {oldlen + Plugin.REGISTRIES.items.Count}))");
            }
        }
    }
}