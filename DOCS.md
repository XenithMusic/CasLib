# Documentation
## File Formats
### `CasLibModelFormat`
A JSON file that describes how a prefab should be constructed.

It consists of a list of `Instruction`s.

An `Instruction` is of the following format:
`{"func":"Function", ...further arguments...}`

The available `"func"`s are:
#### `CasLibModelFormatVersion`
`"version"`: The version of CasLibModelFormat that this file is described with. Should be `1`.
#### `AddComponent`
`"type"`: The type of component that should be added. Should be of form `Unambiguous.Reference.To.Class, AssemblyThatTheClassIsIn`. For example, `UnityEngine.Sprite, UnityEngine.CoreModule`.
#### `Construct`
`"type"`: The type of component that should be constructed.

`"op"`: How the object should be constructed.
##### `"LoadSprite"`: Loads a sprite.
##### `"GenericConstructor"`: Calls the constructor for the type, with no arguments.
#### `SetComponentField`
`"type"`: The type of component that should be modified.

`"field"`: The field that should be modified.

`"value"`: The new value for the field.

Alternatively, you may pass `constructionID` instead of `value` to refer to an object constructed using `Construct`. 1 refers to the result of the first `Construct` call, 2 to the second, 3 to the third.
#### `ResetFieldParent`, `SetFieldParent`, `SetField`, `GetField`
currently too lazy to do documentation for this.
please refer to my code!

also, `GetField` is not implemented.
<!-- TODO: implement GetField. -->
## Classes
### `CasLibItem`
A class representing a custom item.

The constructor is of format `(string itemID, ItemInfo itemInfo, Func assetLoader)`

`itemID` is the internal ID used to spawn the item. Reference the strings used in the `spawn [id]` command.

`itemInfo` is an internal Casualties Unknown object, which is used to specify details about an item.

`assetLoader` is a function of format `GameObject assetLoader(string itemID)`, which should return the `GameObject` to be generated when this item is instantiated.
#### `CasLibItem.referenceObject`
This is the object that should be cloned to create a new instance of this item.
#### `CasLibItem.vanillaItem`
This is a `ItemInfo` describing this item.
#### `CasLibItem.id`
This is the id of the item.

### `FlagsDecayInfo`
Contains flags to be used for ItemInfo.decayInfo.

`DECAY_WHEN_FILLED` makes the item only decay when it's not empty.

`DECAY_WHEN_MAIN_SLOTS` makes the item only decay when it's not in a container.

`DECAY_WHEN_MOVING` makes the item only decay when moving.

### `CraftingQualitys`
Contains strings referring to vanilla crafting qualities.

These are as follows:
- `CUTTING`
- `HAMMERING`
- `NAILS`
- `DRESSING`
- `FOLIAGE`
- `FLOUR`
- `PRODUCE`
- `RIPPABLE`
- `WATER`
- `DISINFECTANT`
- `BLOOD`
- `FAT`
- `OPIATE`
- `CONDIMENT`
- `HEATSOURCE`
- `FIRESTARTER`
- `FLAMMABLE`

### `CasLibItemPool`
#### `void CasLibItemPool.AddToLootPool(CasLibItem item)`
> [!WARNING]
> This is an internal function, and should not be called directly if you don't know what you're doing. Review the source code of Registries.cs first.

Adds an item to the loot pool. This uses item.vanillaItem.category in order to populate the loot pool.
#### `void CasLibItemPool.AddItemInternal(CasLibItem item)`
> [!WARNING]
> This is an internal function, and should not be called directly if you don't know what you're doing. Review the source code of Registries.cs first.

Adds an item to the internal array of all items.
### `CasLibRegistries`
This should not be instantiated; use CasLib.REGISTRIES as a singleton reference to this class.
#### `void CasLibRegistries.RegisterItem(CasLibItem item)`
Registers an item with CasLib. This should only ever be called on plugin startup, as calling this late may result in uninitialized data.
### `CasLibPrefabLoader`
This is kinda shit, though it is necessary.
Only `LoadPrefab` is implemented, as the others are sorta difficult to use. This will change, as it is quite preferable to be able to load stuff. Sorry, you'll have to handle most resource loading yourself.
<!-- TODO: make other Load functions better. -->
#### `GameObject CasLibPrefabLoader.LoadPrefab(object startingPoint, string PluginFolder, string path)`
Loads a prefab using CasLibModelFormat style JSON. `startingPoint` indicates the base object that the prefab should be constructed from. `PluginFolder` indicates the name of the folder your plugin is in. `path` indicates the path relative to your plugin's folder, where the prefab resides.
#### `GameObject CasLibPrefabLoader.LoadPrefab(string PluginFolder, string path)`
Same as the above function, but passes a default `new GameObject()` to `startingPoint`.