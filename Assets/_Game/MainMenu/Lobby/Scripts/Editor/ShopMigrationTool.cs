// ── ShopMigrationTool.cs ─────────────────────────────────────────────────────
// One-click tool: reads every wired field from GameShop and copies them into
// ShopUIController.  Run it once, then delete (or keep) GameShop from the scene.
//
// Menu:  RapLegends ▸ Migrate GameShop → ShopUIController
// ─────────────────────────────────────────────────────────────────────────────
using UnityEngine;
using UnityEditor;

public static class ShopMigrationTool
{
    [MenuItem("RapLegends/Migrate GameShop → ShopUIController")]
    private static void Migrate()
    {
        // ── Find source and target (includeInactive: true handles disabled GameObjects) ──
        var source = Object.FindObjectOfType<GameShop>(true);
        if (source == null) { Debug.LogError("[ShopMigration] GameShop not found in scene."); return; }

        var target = Object.FindObjectOfType<ShopUIController>(true);
        if (target == null) { Debug.LogError("[ShopMigration] ShopUIController not found in scene."); return; }

        var so = new SerializedObject(target);
        int ok = 0, skip = 0;

        // ── Helper: copy a value and log the result ───────────────────────────
        void Copy(string label, string targetProp, object value)
        {
            var prop = so.FindProperty(targetProp);
            if (prop == null) { Debug.LogWarning($"[ShopMigration] SKIP  {label}  →  '{targetProp}' not found on ShopUIController"); skip++; return; }

            switch (value)
            {
                case Object obj:
                    prop.objectReferenceValue = obj;
                    break;
                case string[] arr:
                    prop.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++)
                        prop.GetArrayElementAtIndex(i).stringValue = arr[i];
                    break;
                case string s:
                    prop.stringValue = s;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case float f:
                    prop.floatValue = f;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
                default:
                    Debug.LogWarning($"[ShopMigration] SKIP  {label}  — unsupported type {value?.GetType()}");
                    skip++;
                    return;
            }

            Debug.Log($"[ShopMigration] OK    {label}  →  {targetProp}");
            ok++;
        }

        // ── Direct name matches ───────────────────────────────────────────────
        Copy("normalCamera",  "normalCamera",  source.normalCamera);
        Copy("shopCamera",    "shopCamera",    source.shopCamera);
        Copy("cameraOrbit",   "cameraOrbit",   source.cameraOrbit);
        Copy("player",        "player",        source.player);
        Copy("house",         "house",         source.house);
        Copy("normalPos",     "normalPos",     source.normalPos);
        Copy("shopPos",       "shopPos",       source.shopPos);
        Copy("controls",      "controls",      source.controls);

        // ── Renamed fields ────────────────────────────────────────────────────
        // GameShop.anim  →  ShopUIController.previewAnim
        Copy("anim (→ previewAnim)",               "previewAnim",     source.anim);

        // GameShop.hardPunchClipName  →  ShopUIController.hardPunchClips
        Copy("hardPunchClipName (→ hardPunchClips)", "hardPunchClips", source.hardPunchClipName);

        // GameShop.hardKickClipName   →  ShopUIController.hardKickClips
        Copy("hardKickClipName (→ hardKickClips)",   "hardKickClips",  source.hardKickClipName);

        // GameShop.celebrateClipName  →  ShopUIController.celebrateClips
        Copy("celebrateClipName (→ celebrateClips)", "celebrateClips", source.celebrateClipName);

        // NOTE: introClipName has no match — intro animations play in-match, not in shop preview.
        // NOTE: fightStyleClips has no source — fight styles have no preview clip in GameShop.
        //       Assign GameShop.introClipName to ShopUIController.fightStyleClips manually
        //       if those clips double as fight-style previews, otherwise leave empty.

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);

        // ── Summary ───────────────────────────────────────────────────────────
        Debug.Log($"[ShopMigration] Done. {ok} fields copied, {skip} skipped. " +
                  $"Check the ShopUIController Inspector — assign fightStyleClips manually if needed.");

        // ── Unmapped GameShop fields (kept for reference) ─────────────────────
        // The following GameShop fields are intentionally NOT migrated because
        // they belong to the old uGUI hierarchy that ShopUIController replaces:
        //
        //   cashText                  → driven by UI Toolkit CashLabel
        //   mainScreen                → replaced by UXML CharacterPickerScreen
        //   playerSelectScreen        → replaced by UXML CharacterPickerScreen
        //   playerCustomizationScreen → replaced by UXML CustomizationScreen
        //   panels / activePanelText  → replaced by UXML NavSidebar tabs
        //   normalColor / selectedColor → replaced by Shop.uss tab states
        //   selectionButtons          → replaced by UXML shop-action-bar
        //   buyButton / selectButton  → replaced by UXML BtnBuy* / BtnSelect*
        //   itemCostText              → driven by ShopUIController.SetActions()
        //   fightStyleButtons / introButtons / hardPunchButtons / etc.
        //                             → replaced by dynamic shop-item-grid tiles
        //   selectingPlayer / selectingHat / etc. → replaced by Tab enum
        //   currentCharacterIndex / currentHatIndex → private in controller
        //   duration / easeType       → DOTween for old buttons, not needed
        //   openPortalButton / openShopButton → external scene buttons, not shop internals
        //   screens (TV screens)      → scene dressing, not part of shop logic
    }

    // Validates that both components exist before enabling the menu item
    [MenuItem("RapLegends/Migrate GameShop → ShopUIController", true)]
    private static bool ValidateMigrate()
    {
        return Object.FindObjectOfType<GameShop>(true) != null
            && Object.FindObjectOfType<ShopUIController>(true) != null;
    }
}
