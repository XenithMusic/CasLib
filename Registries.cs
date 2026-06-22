using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using KrokoshaCasualtiesMP;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        }
        public static void AddItemInternal(CasLibItem item)
        {
            item.InitializeItemCharacteristics();
            Item.GlobalItems.Add(item.id,item.vanillaItem);
            AddToLootPool(item);
        }
    }
    public class CasLibRegistries
    {
        public List<Setting> settings;
        public Dictionary<string,CasLibItem> items;
        public Dictionary<string,Func<float,Body,bool>> hooksFootStep;
        public Dictionary<string,Func<Collision2D,Body,bool>> hooksCollision;
        public Dictionary<string,AudioClip> audioClips;
        public Dictionary<string,Command> commands;
        public CasLibRegistries()
        {
            settings = new List<Setting>();
            items = new Dictionary<string,CasLibItem>();
            hooksFootStep = new Dictionary<string,Func<float,Body,bool>>();
            hooksCollision = new Dictionary<string,Func<Collision2D,Body,bool>>();
            audioClips = new Dictionary<string,AudioClip>();
            commands = new Dictionary<string,Command>();
        }
        public void RegisterSetting(Setting setting)
        {
            Plugin.Logger.LogInfo($"Registering Setting {setting.name}...");
            settings.Add(setting);
        }
        public void RegisterItem(CasLibItem item)
        {
            Plugin.Logger.LogInfo($"Registering CasLibItem {item.id}...");
            items.Add(item.id,item);
        }
        public void RegisterAudioClip(string id, AudioClip audio)
        {
            Plugin.Logger.LogInfo($"Registering AudioClip {id}...");
            audioClips.Add(id,audio);
        }
        /// <summary>
        /// Registers a function to run on footstep.
        /// </summary>
        /// <param name="id">
        /// An identifier to refer to this hook.
        /// </param>
        /// <param name="hook">
        /// Function that takes in two parameters (a float representing the volume of the footstep, and a Body representing the Body instance.)
        /// This function should return a bool, which is `true` if the vanilla code should run.
        /// </param>
        public void RegisterHookFootStep(string id, Func<float,Body,bool> hook)
        {
            Plugin.Logger.LogInfo($"Registering Footstep Hook {id}");
            hooksFootStep.Add(id,hook);
        }
        /// <summary>
        /// Registers a function to run on collision (prefix).
        /// </summary>
        /// <param name="id">
        /// An identifier to refer to this hook.
        /// </param>
        /// <param name="hook">
        /// Function that takes in two parameters (a Collision2D representing the collision, and a Body representing the Body instance.)
        /// This function should return a bool, which is `true` if the vanilla code should run.
        /// </param>
        public void RegisterHookCollision(string id, Func<Collision2D,Body,bool> hook)
        {
            Plugin.Logger.LogInfo($"Registering Footstep Hook {id}");
            hooksCollision.Add(id,hook);
        }
        public void RegisterCommand(Command command)
        {
            Plugin.Logger.LogInfo($"Registering Command {command.name}...");
            commands.Add(command.name,command);
        }

        public AudioClip GetRegisteredAudioClip(string id)
        {
            return audioClips[id];
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
                CasLibItemPool.AddToLootPool(item);
            }
        }
    }
    [HarmonyPatch(typeof(Item),nameof(Item.SetupItems))]
    public class GlobalItemsSetupRegistryPatch
    {
        public static void Postfix()
        {
            Plugin.Logger.LogMessage("CasLib Registry updating global items; it is nolonger safe to update the registry!");
            foreach(CasLibItem item in Plugin.REGISTRIES.items.Values)
            {
                CasLibItemPool.AddItemInternal(item);
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
                __result = Plugin.REGISTRIES.items[path].LoadAsset();
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
            // Plugin.Logger.LogInfo(
            //     $"Object LoadAll hit: {path}, length={__result.Length}, scene={SceneManager.GetActiveScene().GetHashCode()}"
            // );
            if (path == "") {
                int oldlen = __result.Length;
                List<UnityEngine.Object> list = __result.ToList();
                // Plugin.Logger.LogInfo("Path is empty string, registry should be appended!");
                foreach (CasLibItem item in Plugin.REGISTRIES.items.Values)
                {
                    Plugin.Logger.LogInfo($"id={item.GetHashCode()}");
                    list.Add(item.LoadAsset());
                }
                __result = list.ToArray();
                // Plugin.Logger.LogInfo($"Loaded all! (length {oldlen} => {__result.Length}, expected (length {oldlen} => {oldlen + Plugin.REGISTRIES.items.Count}))");
            }
        }
    }
    [HarmonyPatch(typeof(Body),nameof(Body.FootStep))]
    public class FootStepHooksPatch
    {
        public static bool Prefix(float vol, ref Body __instance)
        {
            foreach (Func<float,Body,bool> func in Plugin.REGISTRIES.hooksFootStep.Values)
            {
                if (func == null) continue;
                if (func(vol,__instance) == true) return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Sound))]
    [HarmonyPatch(nameof(Sound.Play),[typeof(string),typeof(Vector2),typeof(bool),typeof(bool),typeof(Transform),typeof(float),typeof(float),typeof(bool),typeof(bool)])]
    public class PlaySoundFromStringPatch
    {
        public static bool Prefix(string clip, Vector2 pos, bool twoDimensional, bool pitchShift, Transform follow, float volume, float pitch, bool noReverb, bool ignoreMixer,ref AudioSource __result)
        {
            if (clip == null) return true;
            if (Plugin.REGISTRIES.audioClips.ContainsKey(clip))
            {
                // Plugin.Logger.LogInfo($"Playing audioclip! ({clip}={Plugin.REGISTRIES.audioClips[clip]})");
                __result = Sound.Play(Plugin.REGISTRIES.audioClips[clip],pos,twoDimensional,pitchShift,follow,volume,pitch,noReverb,ignoreMixer);
                return false;
            }
            // Plugin.Logger.LogInfo($"Not my audioclip! ({clip} not in {string.Join(", ",Plugin.REGISTRIES.audioClips.Keys)})");
            return true;
        }
    }
    [HarmonyPatch(typeof(ConsoleScript),nameof(ConsoleScript.RegisterAllCommands))]
    public class RegisterCommandsPatch
    {
        public static void Postfix()
        {
            foreach (Command cmd in Plugin.REGISTRIES.commands.Values)
            {
                ConsoleScript.Commands.Add(cmd);
            }
        }
    }
    [HarmonyPatch(typeof(Settings),nameof(Settings.DefaultSettings))]
    public class RegisterSettingsPatch
    {
        public static void Postfix(ref List<Setting> __result)
        {
            foreach(Setting setting in Plugin.REGISTRIES.settings)
            {
                __result.Add(setting);
            }
        }
    }
}