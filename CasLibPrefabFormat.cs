using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace CasLib
{
    public class Instruction
    {
        public string op;
        public Dictionary<string,object> args;
        private static object GetMemberValue(object obj,string member)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(member);
            if (field == null)
            {
                PropertyInfo property = type.GetProperty(member);
                return property.GetValue(obj);
            } else
            {
                return field.GetValue(obj);
            }
        }
        private static void SetMemberValue(object obj,string member,object value)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(member);
            if (field == null)
            {
                PropertyInfo property = type.GetProperty(member);
                property.SetValue(obj,value);
            } else
            {
                field.SetValue(obj,value);
            }
        }
        public Instruction(string op, Dictionary<string,object> args)
        {
            this.op = op;
            this.args = args;
        }
        public void Run()
        {
            if (op == "CasLibModelFormatVersion")
            {
                CasLibPrefabLoader.expectedVersion = (long)args.GetValueOrDefault("version",0);
            } else if (op == "AddComponent")
            {
                if (((GameObject)CasLibPrefabLoader.constructedObjects[0]).GetComponent(Type.GetType((string)args["type"]))) return;
                ((GameObject)CasLibPrefabLoader.constructedObjects[0]).AddComponent(Type.GetType((string)args["type"]));
            } else if (op == "SetComponentField")
            {
                if (args.ContainsKey("constructionID"))
                {
                    Plugin.Logger.LogInfo("Blah blah blah, it should be one of my constructed objects, whoa!!!");
                    args["value"] = CasLibPrefabLoader.constructedObjects[(int)(long)args["constructionID"]];
                }
                FieldInfo field = Type.GetType((string)args["type"]).GetField((string)args["field"]);
                // sets the field args["field"] (`field`) in the
                // component (`blah blah.GetComponent(blahblah)`)
                // to "value" `args["value"]`
                if (field == null) Plugin.Logger.LogWarning("AAAAAAAA FIELD NULLLLLL");
                SetMemberValue(
                        ((GameObject)CasLibPrefabLoader.constructedObjects[0])
                        .GetComponent(
                            Type.GetType((string)args["type"])
                            ),
                    (string)args["field"],
                    args["value"]
                    );
            } else if (op == "SetFieldParent")
            {
                string[] split = ((string)args["parent"]).Split(".");
                object newFP = ((GameObject)CasLibPrefabLoader.constructedObjects[0]).GetComponent(Type.GetType((string)args["type"]));
                foreach (string item in split)
                {
                    newFP = GetMemberValue(newFP,item);
                }
                CasLibPrefabLoader.FieldParent = newFP;
            } else if (op == "SetField")
            {
                SetMemberValue(CasLibPrefabLoader.FieldParent,(string)args["field"],args["value"]);
            } else if (op == "Construct")
            {
                CasLibPrefabLoader.Construct(args);
            } else if (op == "ResetFieldParent")
            {
                CasLibPrefabLoader.FieldParent = null;
            }
        }
    }
    public static class CasLibPrefabLoader
    {
        public static long expectedVersion = 0;
        public static List<object> constructedObjects = null;
        public static object FieldParent = null;
        public static string PluginFolder = "CasLib";
        public static string ParseRelativePath(string pluginfolder, string path)
        {
            return Path.Combine(BepInEx.Paths.PluginPath,pluginfolder,"Resources",path);
        }
        public static string ParseRelativePath(string path)
        {
            return ParseRelativePath(CasLibPrefabLoader.PluginFolder,path);
        }
        public static IEnumerator<UnityEngine.Networking.UnityWebRequestAsyncOperation> LoadAudioClipCoroutine(string pluginfolder, string path, bool stream)
        {
            string abspath = ParseRelativePath(pluginfolder,path);
            Plugin.Logger.LogInfo("start request");
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + abspath,AudioType.OGGVORBIS);
            Plugin.Logger.LogInfo("waiting for request to finish");
            yield return request.SendWebRequest();
            Plugin.Logger.LogInfo("it finished");
            if (request.result != UnityWebRequest.Result.Success)
            {
                Plugin.Logger.LogError($"Failed to get audio clip! Path: '{"file://" + abspath}', Error: {request.error}");
                yield break;
            }
            Plugin.Logger.LogInfo("getting audio clip");
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            
            Plugin.Logger.LogInfo("registering audio clip");
            Plugin.REGISTRIES.RegisterAudioClip(Path.GetFileNameWithoutExtension(abspath),clip);
        }
        public static Texture2D LoadTexture(string pluginfolder, string path, Vector2Int texSize)
        {
            CasLibPrefabLoader.PluginFolder = pluginfolder;
            Texture2D tex = new Texture2D(texSize.x,texSize.y);
            tex.LoadImage(File.ReadAllBytes(ParseRelativePath(path)));
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }
        public static Sprite LoadSprite(string pluginfolder, string path, Vector2Int texSize, Rect rect, Vector2 pivot, float pxpu)
        {
            Texture2D tex = LoadTexture(pluginfolder,path,texSize);
            return LoadSprite(tex,rect,pivot,pxpu);
        }
        public static Sprite LoadSprite(Texture2D tex, Rect rect, Vector2 pivot, float pxpu)
        {
            Sprite sprite = Sprite.Create(tex,rect,pivot,pxpu);
            return sprite;
        }
        public static void Construct(Dictionary<string,object> args)
        {
            if ((string)args["op"] == "LoadSprite")
            {
                constructedObjects.Add(LoadSprite(
                    CasLibPrefabLoader.PluginFolder,
                    (string)args["src"],
                    new Vector2Int((int)(long)args["txwidth"],(int)(long)args["txheight"]),
                    new Rect(0,0,(int)(long)args["txwidth"],(int)(long)args["txheight"]),
                    new Vector2(0,0),
                    (float)(double)args["pxpu"]
                ));
            } else if ((string)args["op"] == "LoadSpriteRP")
            {
                // TODO: implement LoadSprite(path,rect,pivot)
            } else if ((string)args["op"] == "LoadSpriteTex")
            {
                // TODO: implement LoadSprite(tex,rect,pivot)
            } else if ((string)args["op"] == "GenericConstructor")
            {
                constructedObjects.Add(Activator.CreateInstance(Type.GetType((string)args["type"])));
            }
        }
        public static GameObject LoadPrefab(object startingPoint, string PluginFolder, string path)
        {
            CasLibPrefabLoader.PluginFolder = PluginFolder;
            constructedObjects = new List<object>();
            constructedObjects.Add(startingPoint);
            Dictionary<string,object>[] instructions = JsonConvert.DeserializeObject<Dictionary<string,object>[]>(File.ReadAllText(path));
            foreach (Dictionary<string,object> inst in instructions)
            {
                Plugin.Logger.LogMessage((string)inst["func"]);
                new Instruction((string)inst["func"],inst).Run();
            }
            if (constructedObjects[0] == null) Plugin.Logger.LogError("THAT'S QUITE BAD");
            return (GameObject)constructedObjects[0];
        }
        public static GameObject LoadPrefab(string PluginFolder, string path)
        {
            GameObject go = new GameObject();
            go.SetActive(false);
            return LoadPrefab(go, PluginFolder, path);
        }
    }
}