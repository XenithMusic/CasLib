# Changelog
```diff
Key:
+ Addition
- Removal
*** Note
! Bugfix
- Known Bug
@@ Section
 @ Subsection

--- *Unreleased* 0.2.0
+ Hooks into some functions
+   Footstep hook
+   Collision hook
+ Added Registries
+   Audio Clip Registry (invoke using CasLibPrefabLoader.LoadAudioClipCoroutine)
@@ Bug Fixes
- KrokMP nolonger causes 1 error per frame per registered item.
- Item.Start or whatever it was nolonger errors in game init.
@@ Breaking Changes
 @
! CasLibItem.referenceObject is nolonger destroyed
- CasLibItem.referenceObject
*** Use CasLibItem.LoadAsset() instead.

--- *Unreleased* 0.1.0
+ Added Registries
+   Item Registry
+ Added Default Prefab Loader
. Many a fucking bug.

```