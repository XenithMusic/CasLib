using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CasLib
{
    public class CasLibItem
    {
        public ItemInfo vanillaItem;
        private readonly int debugId = UnityEngine.Random.Range(0, int.MaxValue);
        public string id;
        /// <summary>
        /// A function that should generate a GameObject for this item, and return it.
        /// It should use the following header:
        /// <code>
        /// GameObject handler(string itemID)
        /// </code>
        /// </summary>
        public Func<string,GameObject> assetLoader;
        public class DeathWitness : MonoBehaviour
        {
            private void OnDestroy()
            {
                Debug.LogError(
                    $"DESTROYED: {gameObject.name}\n" +
                    Environment.StackTrace
                );
            }
            private void OnDisable()
            {
                Debug.LogWarning(
                    $"DISABLED: {gameObject.name}\n" +
                    Environment.StackTrace
                );
            }
        }
        public GameObject LoadAsset()
        {
            GameObject go = assetLoader(id);
            go.transform.position = new Vector3(100000,100000,100000);
            if (go == null) Plugin.Logger.LogWarning("it's null in the loadasset somehow??");
            Item item = go.GetComponent<Item>();
            item.id = id;
            item.name = id;

            // HACK: prevent 1 error per frame per item in the main
            //       menu due to KrokMP not checking if there's a world
            //       in their patch. this is quite possibly my fault,
            //       (possibly related to me faking prefabs for easy
            //       development) but i have not found any other
            //       fix/workaround.
            //
            //       and no, ignoring the errors is not an option, with
            //       17 items, this bug causes a framerate of 7 fps.
            go.SetActive(WorldGeneration.world != null && WorldGeneration.world.worldExists);
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
        }
        /// <param name="id">The ID for the item.</param>
        /// <param name="vanillaItem">The ItemInfo that should be passed to Casualties: Unknown</param>
        /// <param name="assetLoader">The assetLoader function for this. Refer to <see cref="assetLoader"/></param>
        public CasLibItem(string id, ItemInfo vanillaItem, Func<string,GameObject> assetLoader)
        {
            this.id = id;
            this.vanillaItem = vanillaItem;
            this.assetLoader = assetLoader;
        }
    }
}