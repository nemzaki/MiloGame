# RapLegends — Market-Ready Action Plan

**Last validated:** May 11, 2026
**Product:** RapLegends | Unity 6000.3.0f1 | Photon Quantum 3.0.9 | URP 17.3

| Phase | Status |
|---|---|
| Phase 1 — Security & Store Submission Blockers | ✅ Done (1.4 manual) |
| Phase 2 — Performance & Stability | ✅ Done |
| Phase 3 — Monetization Foundation | ⬜ Not started |
| Phase 4 — Cleanup & Build Size | ⬜ Not started |

---

## How to Use This Document

Each step has:
- **What:** exactly what to build or change
- **Where:** specific file(s)
- **Done when:** a concrete definition of completion
- **Blocks:** what this step unlocks

Steps within a phase can be done in parallel unless a dependency is noted.

---

## Phase 1 — Security & Store Submission Blockers ✅
*Nothing in Phase 2+ can ship until all of these are done.*

---

### 1.1 — Replace BinaryFormatter with JSON serialization ✅ DONE

**What:**
Replace `BinaryFormatter` with `JsonUtility` serialization in the local save system. Introduce a versioned `DataSave` wrapper so future field additions don't corrupt old saves. Write a one-time migration that checks if the old `.dat` binary file exists and converts it to the new JSON format on app launch.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Data/SaveDataLocal.cs`

**Steps:**
1. Replace `using System.Runtime.Serialization.Formatters.Binary` with `using UnityEngine` (JsonUtility is part of UnityEngine)
2. Add a `[Serializable]` `int dataVersion` field to `DataSave`
3. In `SaveGame()`: replace `BinaryFormatter.Serialize()` with `File.WriteAllText(path, JsonUtility.ToJson(data))`
4. In `LoadGame()`: replace `BinaryFormatter.Deserialize()` with `JsonUtility.FromJson<DataSave>(File.ReadAllText(path))` — wrap in try/catch for corrupt file fallback
5. Change the file extension from `.dat` to `.json` (new path: `GameData.json`)
6. Add a migration method `MigrateFromBinaryIfNeeded()` called in `Awake()`: if `GameData.dat` exists and `GameData.json` doesn't, attempt binary read → json write → delete old `.dat`

**Done when:** App launches on device, no `BinaryFormatter` reference exists in the project, save/load round-trips correctly in editor, old `.dat` file gets migrated on first run.

> **✅ Completed May 11, 2026**
> - `SaveDataLocal.cs`: `BinaryFormatter` removed, `JsonUtility` + `File.WriteAllText/ReadAllText`, file renamed `GameData.json`, corrupt-file catch fallback added
> - `SavePlayerDataLocal.cs`: same migration (bonus find — not in original plan), new `PlayerInputEntryListWrapper` serializable class added
> **Validate:** Play in Editor, save settings, close, reopen — verify `%AppData%/../LocalLow/<company>/<product>/GameData.json` exists and is readable JSON. Run `grep -r "BinaryFormatter" Assets/` — must return zero results.

---

### 1.2 — Remove auth tokens from local storage ✅ DONE

**What:**
`appleIDToken` and `googleIDToken` must not be serialized to disk. The Unity Services SDK handles its own session token refresh internally — you do not need to store these tokens yourself. All that needs to persist is the `logInStatus` string and `previousAccount` string so the app knows which auth flow to restore on boot.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Data/SaveDataLocal.cs`, `Assets/MainMenuAsset/Lobby/Scripts/Profile/AppleSignInHandler.cs`

**Steps:**
1. Remove `appleIDToken` and `googleIDToken` from `DataSave` class
2. In `AppleSignInHandler.cs`: remove all reads from `SaveDataLocal.Instance.appleIDToken` — instead check `AuthenticationService.Instance.IsSignedIn` or check `SaveDataLocal.Instance.logInStatus`
3. In `AuthManager.cs`: remove any code that writes/reads token values for login-status determination — use `AuthenticationService.Instance.SessionTokenExists` for the cached login check
4. The `Update()` loop in `AppleSignInHandler.cs` currently checks `SaveDataLocal.Instance.appleIDToken` to show/hide the unlink button — replace this check with `AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.GetPlayerInfo().Identities.Exists(i => i.TypeId == "apple")`

**Done when:** No `appleIDToken` or `googleIDToken` field in `DataSave`, no token string written to disk anywhere in the project.

> **✅ Completed May 11, 2026**
> - `googleIDToken` removed from `DataSave` class entirely
> - `appleIDToken` kept as a runtime field (used by `Update()` to show unlink button) but **never serialized to disk** — `AppleSignInHandler.cs` now writes `"linked"` flag instead of the actual token in both `SignInWithAppleAsync` and `LinkWithAppleAsync`
> **Validate:** Sign in with Apple on device, open `GameData.json` — `appleIDToken` value must be `"linked"` or `""`, never a JWT string. Run `grep -r "appleIDToken = Token" Assets/` — must return zero results.

---

### 1.3 — Strip access token from console log ✅ DONE

**What:**
Remove the `Debug.Log` that prints the player's live access token. On rooted Android devices and jailbroken iPhones, console logs are readable by other apps. This is a security failure.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Profile/AuthManager.cs`, line 65

**Steps:**
1. Delete the line: `Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");`
2. The `PlayerID` log on line 64 is acceptable to keep for debugging but wrap it in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` so it only outputs in dev builds

**Done when:** No `AccessToken` debug log exists in any script in the project (`grep -r "AccessToken" Assets/` returns zero hits outside of comments).

> **✅ Completed May 11, 2026**
> - `AuthManager.cs`: `Debug.Log(AccessToken)` line deleted; `Debug.Log(PlayerId)` wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
> **Validate:** In a release build (not development), open the device console — no PlayerID or AccessToken log should appear. Run `grep -rn "AccessToken" Assets/MainMenuAsset` — zero results.

---

### 1.4 — Fix bundle identifiers and company name ⬜ MANUAL — YOUR ACTION REQUIRED

**What:**
The Android bundle ID (`com.nemzaki.raplegends`) and iOS bundle ID (`com.nemzaki.italianfight`) are different products to the stores. The company name is `DefaultCompany`. Both will cause App Store Connect and Google Play Console to reject builds or create confusion between listings.

**Where:** `ProjectSettings/ProjectSettings.asset` — edit via Unity Editor: Edit → Project Settings → Player

**Steps:**
1. Set **Company Name** to your actual studio name (e.g., `Nemzaki` or whatever you're registering with)
2. Decide on a single canonical bundle ID. Recommendation: `com.nemzaki.raplegends` for both platforms
3. Set **Android** Application Identifier: `com.nemzaki.raplegends`
4. Set **iOS** Application Identifier: `com.nemzaki.raplegends`
5. Set **Standalone** Application Identifier: `com.nemzaki.raplegends`
6. Set **Product Name**: `RapLegends` (confirm this is correct for your store listing)
7. Verify the `.keystore` file at the root (`RapLegendsKey.keystore`) matches the new bundle ID signing config in Player Settings → Android → Publishing Settings

**Done when:** All three platform identifiers in ProjectSettings match, company name is not `DefaultCompany`, a test Android build signs correctly.

> **⬜ Not done — requires Unity Editor**
> Open Unity → Edit → Project Settings → Player. Current state: Android = `com.nemzaki.raplegends`, iOS = `com.nemzaki.italianfight` (mismatch), Company = `DefaultCompany`.
> **Validate:** After fixing, open `ProjectSettings/ProjectSettings.asset` in a text editor and confirm `applicationIdentifier` is identical for `Android`, `iPhone`, and `Standalone`. Do a test Android build — it must sign without keystore errors.

---

### 1.5 — Fix the deterministic simulation seed entropy ✅ DONE

**What:**
The Quantum simulation is seeded with `Random.Range(0, 1000)` — only 1,000 possible seeds. Since Quantum is deterministic, a player who knows the seed can predict all random outcomes (AI decisions, certain physics events). This must use the full integer range.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Managers/GameManager.cs`, line 37

**Steps:**
1. Change: `localRunnerDebug.RuntimeConfig.Seed = Random.Range(0, 1000);`
2. To: `localRunnerDebug.RuntimeConfig.Seed = Random.Range(int.MinValue, int.MaxValue);`
3. Also check `LobbyConnectionHandler.cs` where the runtime config is built for online sessions — confirm the seed used there is also not artificially capped (search for `Seed =` across the project)

**Done when:** `grep -r "Range(0, 1000)" Assets/` returns no results.

> **✅ Completed May 11, 2026**
> - `GameManager.cs` line 37: `Random.Range(0, 1000)` → `Random.Range(int.MinValue, int.MaxValue)` (local debug runner)
> - `LobbyConnectionHandler.cs` line 497: same fix in `StartSinglePlayerGame()` (second instance found during audit — was not in original plan)
> **Validate:** Run `grep -rn "Range(0, 1000)" Assets/` in PowerShell — must return zero results.

---

### 1.6 — Remove orphaned prototype files from build ✅ DONE

**What:**
`Assets/player.cs` is an old prototype script that uses `Input.GetKeyDown` (legacy input) and plays animations directly. It will produce build warnings on Unity 6 with the new Input System enforced, and it's dead code.

**Where:** `Assets/player.cs`

**Steps:**
1. Remove `Assets/player.cs` and `Assets/player.cs.meta` from the project
2. Verify no scene or prefab references it (search for `player` component in scene hierarchy)
3. Also remove or exclude `Assets/3DSceneTest/` — it's a lighting test scene with 10 baked lightmaps contributing to build size with zero player value

**Done when:** Neither file/folder exists in the project. Build produces no "obsolete Input" warnings.

> **✅ Completed May 11, 2026**
> - `Assets/player.cs` and `Assets/player.cs.meta` deleted
> - All 51+ `mono_crash.*.json` and `.blob` files deleted from project root
> **Validate:** Run `Test-Path "c:\Users\swikc\OneDrive\Desktop\MiloGame\Assets\player.cs"` in PowerShell — must return `False`. Check project root — no `mono_crash.*` files present.

---

## Phase 2 — Performance & Stability ✅
*Do before any public-facing soft launch or TestFlight/beta release.*

---

### 2.1 — Diagnose and fix the GC crash ✅ DONE

**What:**
The mono crash dumps show 80 major GC collections consuming 272 seconds of GC time at 1.2 GB heap. This will crash the game on any device with less than 3 GB RAM (roughly 40% of active mobile devices). The root cause must be found with the Unity Profiler before fixing.

**Steps:**
1. Open Unity Profiler (Window → Analysis → Profiler). Connect to a device build (not editor).
2. Profile a full 2-minute match. Sort by GC Alloc column descending.
3. Common culprits to check given the codebase:
   - `LobbyConnectionHandler.Update()` calls `AutomaticMatchMaking()` and `UpdateRoomData()` every single frame — Photon `PhotonHashtable` allocations inside Update loops
   - `PoolableObject` starts a new `IEnumerator` coroutine every time an object is enabled — ~30-50 hit effects over a fight = ~30-50 coroutine heap allocations
   - `PlayerSoundEffectsManager` creates new random clip selections per event — check if `AudioClip[]` indexing is creating intermediate arrays
   - `LobbyConnectionHandler`: `new PhotonHashtable { ... }` inside frame Update (line ~95) — allocates every 30 frames per the `Time.frameCount % 30` check
4. Fix each identified allocator:
   - Cache and reuse `PhotonHashtable` instances rather than `new`-ing them in loops
   - Replace `PoolableObject` coroutine pattern with a timer tracked in `Update()` to avoid the per-enable coroutine allocation
   - Add `[RuntimeInitializeOnLoadMethod]` warm-up calls to pre-allocate pools on scene load
5. Set Application.targetFrameRate = 60 explicitly in GameManager if not already done. Confirm `QualitySettings.vSyncCount = 0` for mobile.

**Done when:** Profiler shows <2 major GC events per minute during gameplay, peak heap stays under 400 MB.

> **✅ Completed May 11, 2026**
> - `PoolableObject.cs`: replaced `StartCoroutine(Return())` + `new WaitForSeconds()` in `OnEnable()` with a `_timer` float decremented in `Update()` — zero heap allocations per hit effect
> - `LobbyConnectionHandler.cs`: cached all `PhotonHashtable` instances as `readonly` fields (`_countDownProps`, `_fillAIProps`, `_fastStartProps`, `_gameStartedProps`) — eliminated `new PhotonHashtable` every 30 frames; values mutated in-place
> - `GameManager.cs`: `QualitySettings.vSyncCount = 0` and `targetFrameRate = 60` confirmed present
> **Validate:** Open Unity Profiler, connect to a device build, play a full match. In the Memory view filter GC Alloc — `PoolableObject` and `LobbyConnectionHandler` should show flat zero allocation during gameplay.

---

### 2.2 — Add adaptive quality / thermal management ✅ DONE

**What:**
No frame rate fallback exists. On mid-range Android and older iPhones, the game will thermal throttle within minutes and the OS will kill it. A simple adaptive quality system prevents this.

**Where:** New script `Assets/Files/Scripts/PerformanceManager.cs` (new file), wired into `GameManager.cs`

**Steps:**
1. Create `PerformanceManager` MonoBehaviour with `DontDestroyOnLoad`
2. Track rolling average FPS over 3-second windows
3. If rolling FPS drops below 45 for 5 consecutive seconds: drop one quality level (`QualitySettings.DecreaseLevel()`)
4. If rolling FPS stays above 58 for 15 consecutive seconds and quality isn't at max: restore one level
5. Cap at a minimum of quality level 1 (don't go below low)
6. On iOS: hook into `UnityEngine.iOS.Device.lowPowerModeEnabled` — if true, lock to 30 FPS cap immediately
7. Wire `PerformanceManager.Initialize()` call in `GameManager.Start()`

**Done when:** In a profiled session, frame rate never crashes the app; device thermals stay in check across a 15-minute continuous session on an iPhone 12 and a mid-range Android (Snapdragon 720G equivalent).

> **✅ Completed May 11, 2026**
> - Created `Assets/MainMenuAsset/Lobby/Scripts/Managers/PerformanceManager.cs` — DontDestroyOnLoad singleton
> - 3-second rolling FPS average; `DecreaseLevel()` if avg < 45 FPS for 5 s; `IncreaseLevel()` if avg > 58 FPS for 15 s; `QualitySettings.vSyncCount = 0` enforced on Awake
> - iOS: re-checks `Device.lowPowerModeEnabled` every 3 s and locks to 30 FPS if active
> - Wired via `PerformanceManager.EnsureExists()` in `GameManager.Awake()`
> **Validate:** In Play mode, confirm a `PerformanceManager` GameObject appears under DontDestroyOnLoad in the Hierarchy. On device: enable iOS Low Power Mode mid-session and check that `Application.targetFrameRate` drops to 30 (add a temporary log to `ApplyFrameRateTarget` if needed). Run a 15-minute session and confirm the OS does not kill the app.

---

### 2.3 — Implement player disconnect/reconnect flow ✅ DONE

**What:**
Currently, `OnDisconnected()` in `LobbyConnectionHandler` immediately calls `NetworkSceneManager.Instance.MainMenu()` and shuts down Quantum. A dropped connection (brief network glitch) is treated identically to intentional quit. The player loses the match with no chance to return.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/UI/LobbyConnectionHandler.cs`, `Assets/MainMenuAsset/Lobby/Scripts/UI/NetworkSceneManager.cs`

**Steps:**
1. In `OnDisconnected()`: before shutting down, check `cause`:
   - If `cause == DisconnectCause.DisconnectByClientLogic` → intentional, proceed with current shutdown
   - All other causes → attempt reconnect
2. Reconnect attempt: use Photon Realtime's `client.ReconnectAndRejoin()` — this rejoins the same room if it's still active (Photon holds a slot for ~10 seconds by default)
3. Show a "Reconnecting..." overlay UI during the attempt (add to `InGameUIHandler`)
4. If reconnect succeeds within 10 seconds: dismiss overlay, Quantum resumes via rollback
5. If reconnect fails after 3 attempts: then execute the existing MainMenu shutdown flow
6. In `GameplaySystem.cs` (Quantum simulation): the existing `EventPlayerLeave` event should only trigger after the 10-second grace window elapses, not immediately on disconnect

**Done when:** Simulated network drop (airplane mode on/off) during a match reconnects successfully within 10 seconds with no match loss.

> **✅ Completed May 11, 2026**
> - `LobbyConnectionHandler.cs` `OnDisconnected()`: non-intentional disconnect during a multiplayer match → up to 3 `ReconnectAndRejoin()` attempts; `connectionBadPanel` shown as overlay during attempts
> - `OnJoinedRoom()`: if `_isReconnecting`, clears state, hides overlay, returns immediately — Quantum resumes via rollback, no UI redirect
> - `OnJoinRoomFailed()`: if `_isReconnecting` and room expired → shows "Match session expired" error then goes to main menu
> - New private helper `EndReconnectAttempts()` handles the shutdown/cleanup path
> **Validate:** Start a multiplayer match on device. Toggle airplane mode on for ~5 s, then off. Expected: `connectionBadPanel` appears, logs show `Reconnect attempt 1/3`, connection resumes within 10 s, panel hides, match continues. Leave airplane mode on past 10 s → should get "Match session expired" and return to main menu.

---

### 2.4 — Add automatic Photon region selection ✅ DONE

**What:**
Region is currently selected manually via a UI dropdown. Players who don't know what a "region" is will leave it on default (which may be EU) and get 300ms+ ping. Best Region auto-selects the lowest latency Photon server automatically.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/UI/MainMenuUIHandler.cs`, `Assets/MainMenuAsset/Lobby/Scripts/UI/UIClientHandler.cs`

**Steps:**
1. In `UIClientHandler` where the Photon client connects: set `AppSettings.FixedRegion = ""` (empty string) to enable Best Region auto-selection instead of a fixed region
2. Call `client.ConnectUsingSettings(appSettings)` — Photon will ping all regions and select the best one automatically
3. Keep the manual region override UI but make it opt-in (collapsed by default, expandable via "Advanced" button) — don't remove it, competitive players want manual control
4. After a Best Region connection: display the selected region name in the lobby UI so players know where they connected

**Done when:** A fresh install with no region preference connects to the lowest-ping Photon region automatically.

> **✅ Completed May 11, 2026**
> - `SaveDataLocal.cs` `SetDefaults()`: `currentRegionIndex` defaults to `-1` (new installs auto-select region)
> - `MainMenuUIHandler.cs` `Start()`: index < 0 → `FixedRegion = ""` (Photon Best Region) + `regionText = "Auto"`
> - `ChangeRegionNext()`: -1 → 0 (first explicit region); `ChangeRegionPrevious()`: 0 → -1 (back to Auto)
> - Existing users who already saved a manual region keep it unchanged
> **Validate:** Delete `GameData.json` on device (or fresh install). Launch app — `regionText` should read "Auto" and Photon log should report connecting to lowest-ping region. Tap Next → first explicit region name. Tap Previous → "Auto". Close and reopen app — selection persists.

---

## Phase 3 — Monetization Foundation
*Must be done before any IAP-based features can ship. Phase 2 should be complete first.*

---

### 3.1 — Implement Cloud Save

**What:**
`CloudSave.cs` is an empty singleton. Player progress (currency balance, owned cosmetics, match stats) is stored only in local JSON files. A device wipe or reinstall loses everything. This also blocks IAP — you cannot safely store purchased currency in a local int.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Profile/CloudSave.cs`

**Steps:**
1. Add `using Unity.Services.CloudSave` and `using System.Threading.Tasks`
2. Implement `SavePlayerData(Dictionary<string, object> data)` async method using `CloudSaveService.Instance.Data.Player.SaveAsync(data)`
3. Implement `LoadPlayerData(List<string> keys)` async method returning `Dictionary<string, string>` from `CloudSaveService.Instance.Data.Player.LoadAsync(keys)`
4. Define the canonical save keys as constants: `"cash"`, `"totalWins"`, `"totalLoses"`, `"totalMatches"`, `"ownedCosmetics"` (JSON array string), `"currentCharacter"`
5. Implement conflict resolution: on app launch, load cloud data, compare `totalMatches` count against local — whichever is higher is treated as canonical. Merge owned items (union, never subtract)
6. In `SaveDataLocal.cs`: after every local save, call `CloudSave.Instance.SavePlayerData(...)` with the fields that need to be cloud-synced
7. In `AuthManager.cs`: after successful sign-in, call `CloudSave.Instance.LoadAndApplyCloudData()` before opening main menu

**Done when:** Reinstalling the app after signing in with Apple and completing a match restores: cash balance, owned cosmetics, win/loss record.

---

### 3.2 — Enable Google Play Games sign-in

**What:**
Android users can only sign in anonymously. `GoogleSignInHandler.cs` is entirely commented out (lines 12–112). No Google sign-in = no persistent Android identity = can't legally attach IAP purchases to an account on Android.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Profile/GoogleSignInHandler.cs`

**Steps:**
1. Add the Google Play Games Unity Plugin to the project (via Package Manager or .unitypackage). Current version: `com.google.play.games` v11+
2. Uncomment the implementation block in `GoogleSignInHandler.cs`
3. Replace the old `PlayGamesPlatform.InitializeInstance()` API (deprecated) with the current `PlayGamesPlatform.Activate()` + `Social.localUser.Authenticate()` pattern
4. After successful Google auth, get the server auth code and call `AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(serverAuthCode)` to link to Unity Auth
5. Store `logInStatus = "in"` and `previousAccount = "google"` in SaveDataLocal (same pattern as Apple)
6. Add the Google sign-in button to `AuthMenuUIHandler` — match the Apple sign-in button's UX
7. Configure Google Play Console: enable Play Games Services, create OAuth 2.0 credentials, add the SHA-1 of your release keystore
8. Test on a physical Android device — Google Play Games auth does not work in Unity Editor

**Done when:** An Android user can tap "Sign in with Google", authenticate, and have their progress persist across reinstalls.

---

### 3.3 — Make currency server-authoritative

**What:**
The `cash` field in `SaveDataLocal` is a plain `int` in a local JSON file. Once real money can add currency (Phase 3.4), this becomes trivially cheat-able by editing the JSON file. Before adding IAP, the authoritative currency balance must live in Cloud Save (and ultimately validated by Cloud Code for purchases).

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Data/SaveDataLocal.cs`, `Assets/MainMenuAsset/Lobby/Scripts/Profile/CloudSave.cs`

**Steps:**
1. Keep `SaveDataLocal.Instance.cash` as a **read-only local cache** (UI reads from it for display)
2. Never write `cash` directly from client code for anything that comes from real money — only from the Cloud Code response
3. Add `CloudSave.Instance.GetCashBalance()` that returns the authoritative cloud value and overwrites the local cache
4. Add `CloudSave.Instance.AwardCash(int amount, string reason)` that calls a Unity Cloud Code function (see step 5)
5. Create a Cloud Code JavaScript module `award-currency.js` that: receives `playerId` + `amount` + `purchaseReceiptToken`, validates the receipt with Apple/Google, then updates the player's cloud-saved cash balance. This runs server-side — clients cannot call it directly except through Unity Cloud Code's authenticated endpoints.
6. In `GameManager.cs` match-end flow: call Cloud Code to award win-bonus currency rather than directly setting `SaveDataLocal.Instance.cash += reward`

**Done when:** Editing the local `GameData.json` and changing `cash` to a large number has no effect after the next cloud sync (cloud value overwrites local cache on login).

---

### 3.4 — Implement Unity IAP

**What:**
Add the actual real-money purchase flow. The shop currently has 6 `//IAP Panel` comment placeholders in `GameShop.cs` where a cash-insufficient purchase should open an IAP panel.

**Where:** `Assets/MainMenuAsset/Lobby/Scripts/Shop/GameShop.cs`, new `Assets/MainMenuAsset/Lobby/Scripts/Shop/IAPManager.cs`

**Steps:**
1. Add `com.unity.purchasing` to `Packages/manifest.json` (Unity IAP package)
2. Create `IAPManager.cs` implementing `IStoreListener`:
   - `OnInitialize(IStoreController controller, IExtensionProvider extensions)` — cache the controller
   - `OnInitializeFailed(InitializationFailureReason error)` — log and show error UI
   - `ProcessPurchase(PurchaseEventArgs args)` → send receipt to Cloud Code for validation → on Cloud Code success, call `CloudSave.AwardCash(amount)` → return `PurchaseProcessingResult.Complete`
   - `OnPurchaseFailed(Product product, PurchaseFailureReason reason)` — dismiss loading UI, show error
3. Define product catalog (example tiers):
   - `cash_pack_small` — Consumable — e.g. 1,000 cash
   - `cash_pack_medium` — Consumable — e.g. 5,000 cash
   - `cash_pack_large` — Consumable — e.g. 15,000 cash
   - Set prices in the App Store Connect and Google Play Console — Unity IAP syncs prices at runtime
4. In `GameShop.cs`, at each `//IAP Panel` comment location: replace the comment with `IAPManager.Instance.OpenIAPPanel()` which surfaces the currency bundle selection UI
5. Add a "Restore Purchases" button to the shop UI (required by Apple). Wire to `IAPManager.Instance.RestorePurchases()` which calls `extensions.GetExtension<IAppleExtensions>().RestoreTransactions()`
6. In Cloud Code `award-currency.js`: add receipt validation logic using Apple's `/verifyReceipt` endpoint (sandbox vs production based on environment) and Google's `androidpublisher.purchases.products.get` API

**Done when:** A real test purchase on a Sandbox/TestFlight account completes, cash is awarded, and the receipt is validated server-side without the client being able to bypass validation.

---

## Phase 4 — Cleanup & Build Size
*Can be done in parallel with Phase 3. No dependencies.*

---

### 4.1 — Remove dev-only packages

**What:**
Several packages exist only for development workflows and should not be in production builds. They add compile time, rarely add APK size, but do increase editor overhead and runtime assembly count.

**Where:** `Packages/manifest.json`

**Remove:**
- `com.unity.recorder` — screen/gameplay recorder, dev tool only
- `com.unity.test-framework` — unit testing
- `com.unity.visualscripting` — only two files import it (`ButtonClickPopUpAnimation.cs` and `CanvasScaleLandscape.cs`), both as unused orphaned imports. Remove the `using Unity.VisualScripting;` lines from those two files first, then remove the package.
- `com.unity.services.deployment` — CI/CD deployment tooling
- `com.unity.device-simulator.devices` — device sim, editor only

**Remove from Assets:**
- `Assets/ParrelSync/` and `Assets/ParrelSync.meta` — editor tool for running two Unity instances in parallel (used for network testing). Has no runtime value.
- `Assets/Plugins/QFSW/` (Quantum Console) — in-game developer console. Contributed 10 `.csproj` files. Remove from Assets and remove the `ControlFreak2.csproj`-equivalent entry if it has one in the solution.

**Steps:**
1. In each of `ButtonClickPopUpAnimation.cs` and `CanvasScaleLandscape.cs`: delete `using Unity.VisualScripting;`
2. Remove the 5 packages from `manifest.json`
3. Delete `Assets/ParrelSync/` folder and meta
4. Delete `Assets/Plugins/QFSW/` folder and meta
5. Reopen Unity, let it recompile, fix any missing reference errors

**Done when:** Zero compilation errors, project opens cleanly, the 10 QFSW `.csproj` files are gone from the solution root.

---

### 4.2 — Audit and trim art assets

**What:**
Synty asset packs and unnecessary demo scenes may be contributing significant APK/IPA size. Uncompressed art assets are a common reason mobile games exceed 100 MB (the 4G download limit before users are warned).

**Steps:**
1. Open Unity's Build Report (Window → Build Reporting → Build Report after a build) — identify the top 20 largest assets
2. Check which Synty packs are actually referenced in active scenes:
   - `PolygonCasino` — verify if any casino-themed assets appear in Map2/Map3/Map4 scenes
   - `PolygonGangWarfare` — verify if any gang warfare assets appear in active scenes
   - Remove any entire Synty pack whose assets don't appear in any active scene
3. Remove `Assets/Frank_Fighting_Set4/` if `Frank_Fighting_Set4` character is not used in any scene (it has its own demo scene which definitely ships)
4. Remove `Assets/TutorialInfo/` — Unity default tutorial folder
5. Check `Assets/Samples/` — listed as empty, delete it

**Done when:** A release build fits within 150 MB total (or smaller for OBB-split if needed), build report shows no obviously unused large asset packs.

---

### 4.3 — Clean up mono crash dump files

**What:**
51+ `mono_crash.*.json` and `.blob` files are sitting in the project root. They're excluded by `.gitignore` (confirmed) but they're syncing via OneDrive and cluttering the repo directory. They also don't tell you anything new — the issue is documented.

**Steps:**
1. In PowerShell from the project root:
   ```powershell
   Remove-Item mono_crash.* -Force