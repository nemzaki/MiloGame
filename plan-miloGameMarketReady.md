# MiloGame ("RapLegends") — Complete Architecture Analysis & Market-Ready Roadmap

---

## What This Game Is

A **1v1 deterministic fighting game** (product name: RapLegends) built on Unity 6 (6000.3.0f1) with URP 17.3, targeting Android, iOS, and Windows. The core loop is: main menu → matchmaking → 2D/3D arena fight (best-of-3 rounds) → results. Think: combo-system brawler with an online competitive mode, cosmetic shop, and AI opponents as fill-in players.

---

## Tech Stack Overview

| Layer | Technology | Version |
|---|---|---|
| Engine | Unity | 6000.3.0f1 |
| Rendering | URP | 17.3.0 |
| Multiplayer | Photon Quantum (deterministic ECS) | 3.0.9 |
| Transport | Photon Realtime | 5.1.5 |
| Character Control | Quantum KCC Addon | - |
| IK & Ragdoll | RootMotion FinalIK | - |
| Camera | Cinemachine | 3.1.3 |
| Input | ControlFreak2 + Unity InputSystem | 1.16.0 |
| Touch | ControlFreak2 (CF2Input) | - |
| Feedback | MoreMountains Feel | - |
| Haptics | Lofelt NiceVibrations | - |
| Tweening | DOTween | - |
| Auth | Unity Services Auth 3.6.0 + AppleAuth SDK | - |
| Cloud | Unity CloudSave 3.2.2 (installed, not implemented) | - |
| Object Pooling | Custom LocalPoolManager | - |

---

## What's Good

**1. Quantum is the right foundation.** Using Photon Quantum 3.x for a fighting game is the correct call. Deterministic lockstep with rollback gives you lag compensation without server-authoritative simulation costs. The 60 FPS tick rate, 60-frame rollback window, and 3x input redundancy are well-configured for competitive play.

**2. Solid simulation architecture.** The ECS design in `QuantumUser/Simulation/Scripts/` is clean — systems are separated (movement, attack, hitbox, gameplay, AI each in their own folder), components map cleanly to fighter concepts, and the frame-based attack windowing (combo queuing, priority windows, hit-stop) is production-quality combat logic.

**3. 25 well-named Quantum events.** Hit reactions, dodge directions, round transitions, connection warnings — all routed through Quantum events keeps simulation and view layers properly decoupled.

**4. AI is configurable and non-trivial.** The AI has 22 states, probability-weighted decision trees loaded from editor assets, hyper-awareness counter logic, and stamina-based behavior modifiers. Solid enough to pair against in offline/training modes.

**5. Input compression is correct.** 14 action flags in 16 bits, encoded movement/look direction in 1 byte each, with hold-detection, double-tap, and ping monitoring — the network input payload is appropriately minimal.

**6. Matchmaking has AI fill logic.** Auto-filling rooms with AI after 20 seconds if only 1 human player prevents failed matches. This matters for a game that won't have huge day-1 MAU.

**7. Object pooling exists.** `LocalPoolManager` covers hit effects. Not comprehensive, but it's there.

**8. Platform authentication is started.** Apple Sign-In with account linking is functional. Unity Services Auth is initialized correctly with cached token refresh.

---

## What's Bad / What Will Hurt You

### CRITICAL — Security

**1. BinaryFormatter on save data — this is a blocker.**
`Assets/MainMenuAsset/Lobby/Scripts/Data/SaveDataLocal.cs` uses `BinaryFormatter` to serialize and deserialize `GameData.dat`. Microsoft banned this API in .NET 5+ for good reason — it allows arbitrary code execution through deserialized payloads. It is also stripped by IL2CPP in some configurations, which will cause silent data corruption on device. Additionally, Apple's App Store review has been flagging BinaryFormatter usage as non-compliant in .NET assemblies. This must be replaced before submitting to either store.

**2. Auth tokens stored in plaintext binary file.**
`appleIDToken` and `googleIDToken` are public fields on the `DataSave` class, serialized directly to disk with no encryption. If a device is even lightly compromised, these tokens can be extracted and used to hijack accounts. Auth tokens should never be persisted to disk directly — use the OS keychain on iOS (via Unity's `SecurePlayerPrefs` or native plugin) and Android Keystore on Android.

**3. Access token logged to console.**
`Assets/MainMenuAsset/Lobby/Scripts/Profile/AuthManager.cs` logs `AuthenticationService.Instance.AccessToken` to `Debug.Log`. On any rooted device or jailbroken phone, console logs are readable by other apps. Strip this before release.

**4. Low-entropy deterministic seed.**
`Assets/MainMenuAsset/Lobby/Scripts/Managers/GameManager.cs` seeds the Quantum simulation with `Random.Range(0, 1000)`. That's 1,000 possible seeds. A motivated cheater can enumerate them. Use `Random.Range(0, int.MaxValue)` at minimum.

---

### HIGH — Architecture Gaps

**5. No IAP implementation — zero revenue path.**
`Assets/MainMenuAsset/Lobby/Scripts/Shop/GameShop.cs` has `//IAP Panel` comments in 5+ places where real-money purchase flows should be. The entire currency system is virtual cash only — there is no way for players to spend real money. For a market-ready release with any monetization model (battle pass, cosmetics, currency bundles), Unity IAP (`com.unity.purchasing`) needs to be integrated, product catalogs defined, receipt validation added (server-side via Unity Cloud Code for anti-fraud), and Google Play Billing / Apple StoreKit wired up.

**6. Cloud save is a stub.**
`Assets/MainMenuAsset/Lobby/Scripts/Profile/CloudSave.cs` is an empty singleton. `com.unity.services.cloudsave` is installed in the manifest but completely unused. This means if a player uninstalls the app, wipes their phone, or switches devices — all progress, purchased cosmetics, and stats are lost. This is a retention and monetization killer.

**7. Google Play Games auth is entirely commented out.**
`Assets/MainMenuAsset/Lobby/Scripts/Profile/GoogleSignInHandler.cs` — the entire implementation is dead code. Android users can only log in anonymously, which means no persistent identity across device reinstalls on Android.

**8. Bundle ID inconsistency.**
Android bundle ID is `com.nemzaki.raplegends`, iOS bundle ID is `com.nemzaki.italianfight`. These are different products to the stores. They need to match (or be intentionally different if publishing separate titles, which seems unintentional). Company name is still `DefaultCompany` — the stores will reject this.

---

### HIGH — Stability / Performance (The Crash Evidence)

**9. The mono_crash files are serious.**
The crash dumps show: **1.2 GB RAM consumed**, **272 seconds spent in garbage collection**, **80 major GC cycles**. That's not a minor performance issue — at those numbers the game is effectively unplayable on mid-range mobile hardware before it crashes. Mobile devices have 2-4 GB RAM total with the OS taking 1-1.5 GB. The culprits are almost certainly:
- String allocations in hot paths (`EventAttackName` passing strings through events per frame)
- Coroutine/IEnumerator allocations in `PoolableObject`, `PlayerSoundEffectsManager`, and `UpdatePlayerMovementAnimator`
- Animator parameter setting via string keys (`_anim.SetBool("paramName", ...)`) — 40+ parameters every frame
- Lists being created in update loops

The `unsafe` pointer access in `PlayerStatsManager` and `PlayerEffectsHandler` is correct for hot paths. But the animator string-based parameters need to be converted to integer hash IDs via `Animator.StringToHash()`.

**10. No frame budget management.**
The game targets 60 FPS but there's no adaptive quality system, no frame rate cap fallback, no thermal warning handling. On an iPhone 12 or a mid-range Android this will thermal throttle within 5 minutes of gameplay.

---

### MEDIUM — Code Quality

**11. `player.cs` at the root of Assets — lowercase class name, unclear purpose.**
Violates C# naming conventions and suggests abandoned/prototype code.

**12. `SystemSetup.User.cs` and `CommandSetup.User.cs` are empty.** Fine architecturally (systems registered via config asset), but worth knowing.

**13. `3DSceneTest` scene and `Frank_Fighting_Set4` sample scene are in the build.**
Test assets that will bloat the APK/IPA and confuse build configurations.

**14. `ParrelSync` and `QuantumConsole (QFSW)` are in the project at runtime.**
Dev tools only. Neither should ship in production builds.

---

## Data Architecture — Current State and What Needs to Change

Right now data lives in three places:

1. **Local binary file** (`GameData.dat` via BinaryFormatter) — authentication state, player prefs, currency, stats, settings, cosmetic selections. Insecure, platform-fragile, can corrupt.
2. **Local JSON files** (`fightshopdata.json`, `Player_{name}_Data.dat`) — shop unlocks, per-character progression.
3. **Nothing in the cloud** — CloudSave.cs is empty.

For a market-ready game you need:
- Local data migrated from BinaryFormatter → `JsonUtility` or `Newtonsoft.Json` with a migration path for existing users
- Auth tokens moved to platform keychain (not serialized at all)
- Cloud Save implemented for: currency balance, owned cosmetics, stats, character progression — keyed to the Unity Services player ID
- Conflict resolution strategy for cloud vs. local (i.e., which wins if they differ)
- A `DataManager` layer that abstracts local vs. cloud reads/writes so the rest of the code doesn't care where data lives

---

## Matchmaking — What You Have and What You'll Need at Scale

Current matchmaking: Photon Realtime rooms with an auto-join 30-second timer, AI fill after 20 seconds, master client starts the game, room properties sync `StartTime` across players.

This works fine at low player counts. Issues as you scale:
- **Region selection is manual** (in UI) — you need automatic region latency selection (Photon Best Region)
- **No skill-based matchmaking (SBMM)** — the room join logic is purely "first available open room". For competitive integrity you'll want ELO/skill brackets, which requires custom Photon webhooks or a separate matchmaking service (Unity Matchmaker or Photon's Fusion matchmaking)
- **No reconnect flow** — if a player drops from a match, the `EventPlayerLeave` event just triggers a death animation. The player loses the match and there's no rejoin path. This is especially harsh if it was a network glitch rather than intentional quit
- **Room list caching to 20 rooms** — fine for now but needs to be thought through at scale

---

## In-App Purchase Implications — What You're Building Into

When you add IAP, here's what you're touching and what will break if not planned:

**What needs to change in `SaveDataLocal.cs`:** The `cash` field is an unprotected `int`. Once real money can add currency, this becomes a cheat target. Client-authoritative currency with no server validation is a fraud vector. IAP receipt validation must happen server-side (Unity Cloud Code is already installed — use it). The currency balance must be server-authoritative, not a local file integer.

**What needs to change in `GameShop.cs`:** The buy flow needs to branch: if paying with virtual cash → existing path; if buying a currency bundle or premium item → Unity IAP `IStoreController.InitiatePurchase()` → receipt goes to Cloud Code function → Cloud Code validates with Apple/Google → Cloud Code updates player's cloud-saved balance → local UI refreshes. Every one of those steps needs error handling, retry logic, and a "restore purchases" path for iOS.

**Apple App Store rules:** Any iOS app with in-app content purchases that benefits from identity must offer Sign in with Apple as an authentication option. You have this — keep it. You also need a "Restore Purchases" button that calls `IStoreController.ConfirmPendingPurchase()` for non-consumables, or Apple will reject your app.

**Google Play policy:** Since `GoogleSignIn` is currently commented out, Android users are anonymous. Anonymous users can't associate purchases with a persistent account. If they reinstall the app, their purchases are gone. This is an explicit policy violation for Google Play. You need real Google Play Games sign-in before you can ship IAP on Android.

---

## Package Breakdown — What's Needed, What's Bloat

**Keep — Core to Gameplay:**
- Photon Quantum + Realtime + KCC Addon — the entire multiplayer stack
- ControlFreak2 — mobile virtual controls (touch joystick, buttons)
- RootMotion FinalIK — IK for hit reactions (actively used in `AnimatorEffectorIK.cs`)
- Lofelt NiceVibrations — haptics (actively used in `FightFeedBackControl.cs`)
- MoreMountains Feel — feedback player system (actively used)
- DOTween — tweening (used in camera, FOV, shop animations)
- AppleAuth SDK — iOS Sign-In
- Unity Input System — base input layer
- Cinemachine 3 — camera management
- Unity Services (Auth, CloudSave, CloudCode) — backend

**Remove for Production Builds (dev-only):**
- `ParrelSync` — editor-only network testing tool, zero value in a shipped build
- `QuantumConsole (QFSW)` — in-game developer console, 10 .csproj files of dead weight in production
- `com.unity.recorder` — gameplay recording tool, no player value
- `com.unity.test-framework` — unit testing, shouldn't be in runtime
- `com.unity.visualscripting` — not used anywhere in the codebase, pure bloat
- `com.unity.services.deployment` — CI/CD tool, dev environment only
- `3DSceneTest` assets and scene
- `Assets/Samples/` — empty Unity package samples folder
- All `mono_crash.*` files in project root

**Verify Before Removing:**
- `Invector` (footstep system) — verify if footstep audio is actually wired up anywhere
- `Frank_Fighting_Set4` — if this character is in active scenes, keep; if sample-only, remove
- `Synty PolygonCasino` and `PolygonGangWarfare` — verify which map uses which Synty pack; remove unused packs (each is ~100-500 MB of art)
- `com.unity.modules.cloth`, `com.unity.modules.vehicles`, `com.unity.modules.vr`, `com.unity.modules.tilemap` — none of these are relevant to a 3D fighting game

**Why So Many `.csproj` Files in Root:**
Each third-party package with its own Assembly Definition generates a `.csproj`. ControlFreak2 = 1, NiceVibrations = 3, MoreMountains = 2, QFSW = 10, Quantum = 5, AppleAuth = 2, ParrelSync = 1. They are generated by Unity automatically — not files you manage. Removing the dev packages above eliminates those `.csproj` files automatically.

---

## Prioritized Roadmap

### Phase 1 — Blockers (must do before store submission)
1. Replace `BinaryFormatter` with `JsonUtility` or Newtonsoft.Json in `SaveDataLocal.cs`
2. Move auth token storage out of the local data file — use platform keychain
3. Remove `Debug.Log(AccessToken)` from `AuthManager.cs`
4. Fix bundle IDs — align Android/iOS to the same `bundleIdentifier`, set real company name
5. Disable unused modules (cloth, vehicles, vr, tilemap) in ProjectSettings if not used
6. Enable IL2CPP scripting backend and ARM64 target (verify in ProjectSettings)
7. Remove `3DSceneTest`, `Samples/`, and the orphaned root `player.cs`

### Phase 2 — Stability (must fix before scale)
8. Profile and fix the GC thrashing — convert all animator `string` parameters to `Animator.StringToHash()` integer IDs, pool coroutines
9. Implement adaptive frame rate / quality scaling for thermal management on mobile
10. Increase Quantum simulation seed entropy in `GameManager.cs` (`Random.Range(0, int.MaxValue)`)
11. Implement player reconnect flow (store match session, allow rejoin within N seconds)

### Phase 3 — Monetization Foundation
12. Implement `CloudSave.cs` — at minimum: currency balance, owned cosmetics, player stats
13. Re-enable Google Play Games sign-in in `GoogleSignInHandler.cs`
14. Make currency server-authoritative via Cloud Code (not a local int)
15. Implement Unity IAP — product catalog, purchase flow in `GameShop.cs`, receipt validation via Cloud Code, restore purchases button for iOS

### Phase 4 — Polish & Scale
16. Remove dev packages (ParrelSync, QuantumConsole, Recorder, test-framework, visualscripting)
17. Auto-detect best Photon region instead of manual selection (Photon Best Region)
18. Add SBMM foundation (skill tracking in Cloud Save, filter rooms by skill bracket)
19. Implement Analytics (Unity Analytics or Firebase) for funnel and retention data
20. Add Android adaptive difficulty / battery/thermal API handling
21. Full QA pass on all Synty packs — remove unused art packs from builds
