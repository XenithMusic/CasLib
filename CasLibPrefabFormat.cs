using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

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
        public static string ParseRelativePath(string path)
        {
            return Path.Combine(BepInEx.Paths.PluginPath,CasLibPrefabLoader.PluginFolder,"Resources",path);
        }
        public static Texture2D LoadTexture(string path)
        {
            Texture2D tex = new Texture2D(2,2);
            tex.LoadRawTextureData(File.ReadAllBytes(ParseRelativePath(path)));
            return tex;
        }
        public static Sprite LoadSprite(string path)
        {
            Texture2D tex = LoadTexture(path);
            return LoadSprite(tex,new Rect(0,0,tex.width,tex.height),new Vector2(0,0));
        }
        public static Sprite LoadSprite(string path, Rect rect, Vector2 pivot)
        {
            Texture2D tex = LoadTexture(path);
            return LoadSprite(tex,rect,pivot);
        }
        public static Sprite LoadSprite(Texture2D tex, Rect rect, Vector2 pivot)
        {
            Sprite sprite = Sprite.Create(tex,rect,pivot);
            return sprite;
        }
        public static void Construct(Dictionary<string,object> args)
        {
            if ((string)args["op"] == "LoadSprite")
            {
                constructedObjects.Add(LoadSprite((string)args["src"]));
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
            return LoadPrefab(new GameObject(), PluginFolder, path);
        }
    }
}