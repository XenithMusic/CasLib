using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CasLib
{
    public class CasLibItem
    {
        public ItemInfo vanillaItem;
        public string id;
        /// <summary>
        /// A function that should generate a GameObject for this item, and return it.
        /// It should use the following header:
        /// <code>
        /// GameObject handler(string itemID)
        /// </code>
        /// </summary>
        public Func<string,GameObject> assetLoader;
        public GameObject referenceObject = null;
        public GameObject LoadAsset()
        {
            GameObject go = GameObject.Instantiate(assetLoader(id));
            if (go == null) Plugin.Logger.LogWarning("it's null in the loadasset somehow??");
            Item item = go.GetComponent<Item>();
            item.id = id;
            item.name = id;
            return go;
        }
        /// <summary>
        /// This function is called after the Item is loaded, but before the constructor exits.
        /// This should be used to finish initializing the Item.
        /// </summary>
        public void InitializeItemCharacteristics()
        {
            this.vanillaItem.fullName = Locale.GetItem(this.id);
            this.vanillaItem.description = Locale.GetItem(this.id + "dsc");
            // copied from vanilla code
            this.vanillaItem.SetTags();
            if (this.vanillaItem.decayMinutes > 0f)
            {
                this.vanillaItem.rotSpeed = 1.666f / this.vanillaItem.decayMinutes;
            }
            // nolonger copied from vanilla code
            Plugin.Logger.LogInfo("loadingAsset");
            this.referenceObject = this.LoadAsset();
            Plugin.Logger.LogInfo("loadedAsset");
            if (this.referenceObject == null) Plugin.Logger.LogWarning("referenceObject is null"); 
        }
        /// <param name="id">The ID for the item.</param>
        /// <param name="vanillaItem">The ItemInfo that should be passed to Casualties: Unknown</param>
        /// <param name="assetLoader">The assetLoader function for this. Refer to <see cref="assetLoader"/></param>
        public CasLibItem(string id, ItemInfo vanillaItem, Func<string,GameObject> assetLoader)
        {
            this.id = id;
            this.vanillaItem = vanillaItem;
            this.assetLoader = assetLoader;
            InitializeItemCharacteristics();
            if (this.referenceObject == null) Plugin.Logger.LogWarning("referenceObject is NOW null");
        }
    }
}