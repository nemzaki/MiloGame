# Rap Legends — Design System Spec

> **Version 1.0 · May 2026 · Phase II Build**
> Source of truth for all UI, HUD, FX, and brand surfaces.
> Pair with: `Rap Legends Brandbook.html` (visual reference).

This file is consumed by Claude Code / engineers / AI agents to generate Unity prefabs, UI Toolkit (UXML/USS), Unity UGUI panels, TextMeshPro presets, shader graphs, and gameplay FX. All hex values, sizes, easings, and component recipes are normative — match them unless explicitly told otherwise.

---

## 0. How to read this doc

- All hex values are sRGB. For Unity, paste hex directly into `Color` fields.
- Sizes are given at **1920×1080 base** (landscape). Multiply by `screenWidth / 1920` for arbitrary resolutions, or use Unity's Canvas Scaler `Scale With Screen Size` (reference 1920×1080, Match=0.5).
- Type sizes are pixels at base resolution; in Unity TMP, set Font Size to the same number and let auto-scaling Canvas handle the rest.
- "Bone" = off-white, not pure `#FFF`. Never use `#000` or pure `#FFF` anywhere.
- All buttons and cards have a **4px hard-cut bottom shadow** (border-bottom in CSS, image-9-slice or `Outline` + offset in Unity). They feel like physical plastic, not flat web UI.

---

## 1. Brand identity

### 1.1 Pillars (use these to QA every screen)

| # | Pillar | Means | Anti-pattern |
|---|---|---|---|
| 01 | **Sfacciato** (bold) | UI yells. Big type, hard shadows, saturated color. | Neutral grays. Whisper voice. Polite spacing. |
| 02 | **Vernacolo** | Bilingual IT × EN. Italian rap slang as flavor. | Translating every UI label to Italian only. Generic English. |
| 03 | **Drip Forward** | Cosmetics are status. Skins, rooms, walkouts are the meta. | Free-to-play stinginess. Hiding cosmetics in submenus. |
| 04 | **Impatto** | Every hit feels heavy. Combat is the chorus drop. | Tiny damage numbers. Flat hit FX. Silent KOs. |

### 1.2 Voice

| Do | Don't |
|---|---|
| "Vai forte" | "You have been defeated." |
| "Round Two. Spacca tutto." | "Purchase failed. Try again." |
| "#tropparoba" | "Welcome, valued user." |
| "K.O. Hai sentito quel beat?" | "Tap to continue your journey." |
| "Drip secured. +120 XP." | "An error has occurred." |

**Slang dictionary** (use as flavor, never as gatekeeping; always subtitle):

- `tropparoba` — too much / great
- `spacca` — destroy / kill it
- `frate` — bro
- `bomba` — bomb / awesome
- `fra'` — bro (short)
- `scialla` — chill
- `boom` — boom (impact)
- `k.o.` — knockout
- `vai vai vai` — go go go

### 1.3 Logo

- **Wordmark**: "RAP" flat-cut bone on top (left-aligned, slightly inset), "LEGENDS" italic outline-stroked magenta with cobalt drop on bottom (extends further right). Two stacked lines.
- **Monogram "RL"**: Bagel Fat One, sheared −8°. Use for app icon, watermark, social avatar.
- **Clear space**: 1 cap-height of "L" minimum on all sides.
- **Min size**: 96px wide digital / 18mm print for wordmark; 64×64 for monogram.

Never: recolor the magenta fill, alter italic angle, stack horizontally, drop the cobalt shadow.

---

## 2. Color tokens

### 2.1 Core palette

| Token | Hex | RGB | Role |
|---|---|---|---|
| `color/ink` | `#0B0A14` | 11,10,20 | Primary ground. App background. |
| `color/ink-2` | `#14112A` | 20,17,42 | L1 surface (cards). |
| `color/ink-3` | `#1F1A3D` | 31,26,61 | L2 raised surface. |
| `color/ink-line` | `#2A2350` | 42,35,80 | Dividers, borders. |
| `color/bone` | `#F5F0E1` | 245,240,225 | Primary text, neutrals. NEVER #FFF. |
| `color/bone-mute` | `#C9C2AE` | 201,194,174 | Muted text, captions. |
| `color/magenta` | `#FF2D7A` | 255,45,122 | Primary brand. CTAs, accents. |
| `color/magenta-deep` | `#B81658` | 184,22,88 | Magenta bottom-shadow. |
| `color/cobalt` | `#2EE5FF` | 46,229,255 | Secondary. Tags, hashtags. |
| `color/cobalt-deep` | `#1A8AA8` | 26,138,168 | Cobalt bottom-shadow. |
| `color/plum` | `#6B2BD9` | 107,43,217 | Tonal accent. |
| `color/ember` | `#FF7A2D` | 255,122,45 | Heat accent. |

### 2.2 Status palette

| Token | Hex | Role |
|---|---|---|
| `color/lime` | `#B6FF3C` | XP, progress, friendly HP, level-up confetti. |
| `color/gold` | `#FFD23F` | Currency, crits, finishers, K.O. word. |
| `color/siren` | `#FF3B30` | Damage taken, low HP (<25%), destructive actions. |

### 2.3 HP gradient (3-stage)

| HP range | Fill gradient |
|---|---|
| 100% → 50% | `linear-gradient(180deg, #B6FF3C 0%, #6BB519 100%)` (lime) |
| 50% → 25% | `linear-gradient(180deg, #FFD23F 0%, #FF7A2D 100%)` (gold→ember) |
| 25% → 0% | `linear-gradient(180deg, #FF3B30 0%, #B81818 100%)` (siren) |

### 2.4 Rarity tiers (cosmetics, skins, fighters, room items)

| Tier | Name (IT/EN) | Top hex | Bottom hex | Text |
|---|---|---|---|---|
| T1 | Comune / Common | `#2A2A3A` | `#1A1A28` | `#B5B0C8` |
| T2 | Solido / Uncommon | `#00B0A8` | `#006B68` | `#F5F0E1` |
| T3 | Hype / Rare | `#2EE5FF` | `#1E7BD8` | `#0B0A14` |
| T4 | Cult / Epic | `#FF2D7A` | `#6B2BD9` | `#F5F0E1` |
| T5 | Leggenda / Legendary | `#FFD23F` | `#FF7A2D` | `#0B0A14` |

Each tier drives: card border, drop animation intensity, confetti color, walkout glow, KO-banner shadow color.

### 2.5 Color rules

- **Max 3 accent colors per screen** (ink + bone don't count). Pick magenta OR cobalt as dominant, never both equal.
- **Never use pure black or pure white.** Substitute `ink` (`#0B0A14`) and `bone` (`#F5F0E1`).
- **Gradients only on featured/hero surfaces** — buttons get hard colors with a 4px hard-cut darker bottom border, not vertical gradients.
- **Status colors are reserved** — lime is XP/heal only, gold is crit/currency only, siren is danger only. Don't use them decoratively.

---

## 3. Typography

### 3.1 Type families

| Role | Family | Weight | Source | Use |
|---|---|---|---|---|
| Display / Holler | **Anton** | 400 | Google Fonts | Headlines, names, K.O., menus, arena titles. ALL CAPS, never lowercase. |
| Sticker / Graffiti | **Bagel Fat One** | 400 | Google Fonts | Hashtags, adlibs, combo callouts, monograms. Always lowercase. Skew −4° to −8°. Only at 24px+. |
| Body / UI | **Space Grotesk** | 400/500/600/700 | Google Fonts | All running text, buttons, descriptions, lore, dialog. |
| Data / HUD | **Space Mono** | 400/700 | Google Fonts | Stats, timers, damage tickers, scoreboards, system labels. ALL CAPS, +10% tracking. |

### 3.2 Scale (1920×1080 base)

| Token | Family | Size | Line | Tracking | Use |
|---|---|---|---|---|---|
| `type/display-hero` | Anton | 120 | 0.86 | −0.5% | K.O. moment, splash. |
| `type/display-title` | Anton | 72 | 0.92 | −0.5% | Section headers (Round Two, Roster). |
| `type/display-header` | Anton | 42 | 1.0 | 0% | Card titles, screen names. |
| `type/display-button` | Anton | 22 | 1.0 | +0.2% | Button labels. |
| `type/graffiti-xl` | Bagel Fat One | 96 | 0.9 | −2% | Big sticker callouts, hashtag splashes. |
| `type/graffiti-m` | Bagel Fat One | 48 | 1.0 | −2% | Combo counter, walkout tag. |
| `type/body-l` | Space Grotesk 600 | 36 | 1.25 | 0% | Important UI labels, modal h2. |
| `type/body-m` | Space Grotesk 400 | 18 | 1.55 | 0% | Default body. |
| `type/body-s` | Space Grotesk 400 | 14 | 1.5 | +1% | Secondary text. |
| `type/mono-m` | Space Mono 700 | 14 | 1.2 | +14% | HUD stats, timers, prices. |
| `type/mono-s` | Space Mono 400 | 11 | 1.2 | +18% | Eyebrows, labels, captions. |
| `type/mono-xs` | Space Mono 400 | 9 | 1.2 | +20% | Tiny micro-labels (badge captions). |

### 3.3 TextMeshPro presets (Unity)

For each role above, create a TMP_FontAsset + Style:

```
TMP/Display-Hero      Anton SDF        FaceSize 90  PaddingX 8  Outline OFF
TMP/Display-KO        Anton SDF        FaceSize 90  Outline 4px (Ink) Shadow (Magenta 6,6)
TMP/Display-Italic    Anton SDF        Skew 12°    Outline 3px (Bone) DropShadow (Cobalt 6,6)
TMP/Graffiti-Combo    BagelFatOne SDF  Outline 2px (Ink) DropShadow (Magenta 3,3) Skew −4°
TMP/Body              SpaceGrotesk SDF FaceSize 64  WordSpacing 1
TMP/Mono-HUD          SpaceMono SDF    UpperCase    Tracking 140 (TMP units)
```

Generate SDF with **Padding 9, Atlas 4096, Render Mode SDFAA**.

### 3.4 Rules

- All `display` roles are ALWAYS uppercase. If passed a lowercase string, force `text-transform: uppercase` or `string.ToUpper()`.
- All `graffiti` roles are ALWAYS lowercase. Force `.ToLower()`.
- All `mono` roles are ALWAYS uppercase + tracked-out.
- Body is sentence-case.
- Never mix two display roles in one block. Never use display for body copy.

---

## 4. Spacing, radii, shadows

### 4.1 Spacing scale (multiples of 4px)

`4 · 8 · 12 · 16 · 24 · 32 · 48 · 64 · 96 · 128`

Don't use values outside this scale.

### 4.2 Radii

| Token | px | Use |
|---|---|---|
| `radius/sharp` | 0 | Tags, ribbons, sticker labels. |
| `radius/xs` | 2 | Pills (mono labels), hashtag boxes. |
| `radius/s` | 4 | Buttons, small cards. |
| `radius/m` | 8 | Default cards, item tiles. |
| `radius/l` | 12 | Featured banners, large modals. |
| `radius/xl` | 16 | Modal dialogs. |
| `radius/round` | 50% | Circular action buttons, portraits in HUD. |

### 4.3 Shadows / depth

Components use a **hard-cut bottom shadow** (NOT a CSS box-shadow blur — a solid darker block underneath).

| Token | Recipe |
|---|---|
| `depth/button` | 4px tall darker color underneath (e.g. magenta-deep `#B81658` under magenta). Implement as `border-bottom: 4px solid X` or a 9-slice image. |
| `depth/card` | `0 12px 0 rgba(0,0,0,0.5)` (hard, no blur). |
| `depth/modal` | `0 16px 0 rgba(0,0,0,0.6)` + 2px ink-line border. |
| `depth/glow-mag` | `0 0 16px rgba(255,45,122,0.5)` — selected state only. |
| `depth/glow-gold` | `0 0 16px rgba(255,210,63,0.6)` — crit / legendary only. |

In Unity:
- For buttons: nest a second darker `Image` 4px below, OR use Sprite 9-slice with bottom edge baked.
- For glows: use UI Particle, additive shader, or `Outline` + `OutlineGlow` script.

### 4.4 Borders

| Token | Width | Color |
|---|---|---|
| `border/default` | 1px | `#2A2350` (ink-line) |
| `border/divider` | 1px | `#2A2350` |
| `border/selected` | 2px | `#FF2D7A` (magenta) + 2px ink-spacer + 2px magenta double-stroke for emphasis |
| `border/legendary` | 2px | `#FFD23F` (gold) |

---

## 5. Iconography

8 core gameplay icons. SVG strokes, 4px width, rounded caps, 64×64 viewbox.

| Icon | Use | Stroke color |
|---|---|---|
| `icon/microphone` | Special / Voice / Walkout intro | bone |
| `icon/special` | Special meter, lightning special | gold |
| `icon/ko` | K.O. marker, defeat banner | siren |
| `icon/combo` | Combo counter glyph | magenta |
| `icon/crown` | Ranked, leaderboard, season winner | cobalt |
| `icon/coin` | Currency (soft coins, not gems) | gold |
| `icon/timer` | Round timer, countdowns | bone |
| `icon/studio` | Player room, customization | lime |

Rules:
- Drawn as if vinyl-cut decals: chunky 3–4px strokes, rounded terminals, no fine detail.
- Single-color fills only. No gradients on icons.
- Min display size 32×32. Below that, swap to a simpler glyph.

---

## 6. Components

### 6.1 Buttons

| Variant | Background | Text color | Bottom shadow | Border | Use |
|---|---|---|---|---|---|
| `btn/primary` | `#FF2D7A` magenta | `#0B0A14` ink | `#B81658` 4px | none | Main CTA. Fight, Buy, Confirm. |
| `btn/cobalt` | `#2EE5FF` cobalt | `#0B0A14` ink | `#1A8AA8` 4px | none | Secondary action. Equip, Detail. |
| `btn/gold` | `linear-gradient(135deg, #FFD23F, #FF7A2D)` | `#0B0A14` ink | `#A85A1A` 4px | 2px ink | Special, Premium, Open box. |
| `btn/ghost` | transparent | `#F5F0E1` bone | none | 2px bone | Cancel, Back. |
| `btn/disabled` | `#1F1A3D` ink-3 | `#C9C2AE` bone-mute | `#2A2350` 4px | none | Locked content. |
| `btn/danger` | `#FF3B30` siren | `#F5F0E1` bone | `#9C0000` 4px | none | Quit match, Delete. |

**Spec:**
- Font: `type/display-button` (Anton 22px ALL CAPS).
- Padding: 14px vertical, 24px horizontal.
- Radius: 4px.
- Active state: translate-y +2px and reduce bottom shadow to 2px (gives a "press" feel).
- Hover (desktop): brighten background +6% lightness.
- Disabled: 60% opacity, no hover/press states.

**Unity prefab**: `UI/Prefabs/Buttons/Btn_Primary.prefab`. Children: `Background` (Image, 9-slice), `Shadow` (Image, magenta-deep, anchored below), `Label` (TMP).

### 6.2 Cards

| Type | Use | Size |
|---|---|---|
| `card/fighter` | Roster slot, character preview | 240×320 |
| `card/item` | Skin, KO FX, walkout, currency pack | 240×260 |
| `card/featured` | Shop hero, season banner | full-width × 240 |

**Anatomy** (card/fighter):
```
┌─────────────────┐
│  [Tier ribbon]  │ ← absolute top-right, sticker-rotated 4°
│ ┌─────────────┐ │
│ │             │ │
│ │   Portrait  │ │ ← square aspect, radius 8px
│ │             │ │
│ └─────────────┘ │
│ FIGHTER NAME    │ ← Anton 22 ALL CAPS
│ City · Class    │ ← Mono 10 cobalt
│ ▰▰▰▰▱           │ ← stat pips, lime-on / ink-line-off
└─────────────────┘
```

Padding 16px. Background `linear-gradient(180deg, ink-3 0%, ink-2 100%)`. Border 1px ink-line. Radius 12px.

### 6.3 Modal dialog

- 320×auto, radius 16px, background `ink-2`, border 2px `ink-line`.
- Title crest: magenta plate, Anton 22, sits centered, overlapping top edge by 50%.
- Body copy: Space Grotesk 14, bone-mute, text-align center.
- Buttons: ghost (Back) + primary (Confirm), gap 12px, justify-center.
- Shadow: `depth/modal`.

### 6.4 HUD elements

#### Health bar (player nameplate)

- Width 220×12px @ 1920 base. Skewed −12° for fighter-game energy.
- Border 2px ink-line, background ink, overflow hidden.
- Fill: HP gradient (see §2.3). Drains right-to-left for P1, left-to-right for P2 (mirrored).
- Drain anim: linear, 400ms ease-out per damage tick.

#### Stamina bar (under HP)

- Width matches HP, height 5px, skewed −12°.
- Background ink + 1px ink-line border. Fill solid cobalt `#2EE5FF`.
- Regens passively; drains on dodge/block.

#### Round-win pips

- Two diamond (rotated 45°) 12px squares.
- Empty: border 2px bone-mute, background ink.
- Won: filled gold, glow `depth/glow-gold`.

#### Round timer (center HUD)

- 64×64 disc, background ink, border 4px gold, radius 50%.
- Number: Anton 28, bone.
- Below: "ROUND 1/2/3" in mono-xs bone-mute.

#### Virtual joystick

- Outer ring 78×78, transparent black 35%, 2px white-translucent border.
- Inner knob 40×40, radial bone gradient, 2px ink border, 3px hard shadow.

#### Action button cluster (bottom-right)

5 buttons arranged in two rows + special:

```
  [L]  [B]  ╔══════╗
  [H]  [D]  ║  SP  ║
            ╚══════╝
```

| Button | Diameter | Background | Label | Function |
|---|---|---|---|---|
| L (Light) | 38px | cobalt | "L" | Light attack |
| H (Heavy) | 38px | magenta | "H" | Heavy attack |
| B (Block) | 38px | bone | "B" | Block / parry |
| D (Dodge) | 38px | lime | "D" | Dodge / dash |
| SP (Special) | 50px | gold→ember gradient | "SP" | Special / cinematic move |

Border 2px bone (ink on light buttons). Hard shadow 3px black 40%.

---

## 7. Screens (canonical layouts, landscape 19.5:9)

### 7.1 Combat HUD

Mobile fighting-game layout, best-of-3 rounds.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ ●32ms                                                                    ✕  │
│ ┌──────────────────┐         ┌─────┐         ┌──────────────────────────┐  │
│ │ [P1] FRATE   ◆◇  │         │  42 │         │  ◇◆ PANTERA [P2]         │  │
│ │ ▰▰▰▰▰▰▰▰░░  HP   │         │R 2/3│         │       HP  ▰▰░░░░░░░░░    │  │
│ │ ▰▰▰▰▰░░░░░ STAM  │         └─────┘         │       STAM ▰▰▰▰▰▰▰▰░░    │  │
│ └──────────────────┘                          └──────────────────────────┘  │
│                                                                             │
│                                                                             │
│        [P1 fighter]              ◯◯◯ BOOM!              [P2 fighter]        │
│                                                                             │
│   ×17                                                                       │
│   combo                                                                     │
│                                                                             │
│   ╭───╮                                              [L] [B]                │
│   │ ◯ │                                              [H] [D]    [SP]       │
│   ╰───╯                                                                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Z-order (front to back):**
1. Action buttons + joystick (z=10)
2. Combo counter (z=8)
3. Impact FX (rings, sparks, words) (z=7)
4. Fighters (z=5)
5. Floor glow (z=4)
6. Top HUD strip (pings, names, timer) (z=15)
7. Background (z=0)

**Behaviour:**
- Top HUD persists; impact FX spawn at contact point and despawn after 240ms.
- Combo counter appears on first hit, increments per hit, fades + slides down after 1.2s idle.
- KO/PERFECT/DOUBLE banner appears center-screen for 1.8s before round-end transition.

### 7.2 Character Select

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ ● Choose Your Fighter     [Lock in 00:28]            ◉ 1,240 GEMS           │
├──────────────────────────────────────┬──────────────────────────────────────┤
│ ┌──────────────────────────────────┐ │ Roster · 6 owned          All ▾     │
│ │                                  │ │ ┌────┬────┬────┬────┐               │
│ │      [Full-body render]          │ │ │T4  │T3  │T5  │T2  │               │
│ │                                  │ │ │Fra │Pan │ReM │Ves │               │
│ │                                  │ │ ├────┼────┼────┼────┤               │
│ │            FRATE                 │ │ │T4  │T3  │ ?  │ ?  │               │
│ │   Roma · Punchline · T4 Cult     │ │ │Tra │Bos │lock│lock│               │
│ │                                  │ │ └────┴────┴────┴────┘               │
│ │ Win 64%  Matches 142  KOs 87     │ │                                     │
│ │ PWR 82 SPD 76 FLO 91 DRIP 68     │ │ Map · Custom room        Host picks │
│ │ Skins: ▣ ▣ ▣ ▣ ▣                 │ │ ┌────┬────┬────┬────┐               │
│ └──────────────────────────────────┘ │ │Rome│Mila│Napl│Sici│               │
│                                      │ └────┴────┴────┴────┘               │
│                                      │                                      │
│                                      │      [   Confirm · Go!   ]          │
└──────────────────────────────────────┴──────────────────────────────────────┘
```

**Behaviour:**
- 30s lock-in timer (ranked only); custom rooms have no timer.
- Skin carousel persists per-fighter selection.
- Locked roster slots show a `?` and tier `?`, fade to 50% opacity.
- Map row: visible in custom-room mode; hidden in ranked (random pick).

### 7.3 Shop

Four tabs: **Featured · Fighters · Cosmetics · Currency**. Persistent coin balance top-right.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ SHOP   [Featured][Fighters][Cosmetics][Currency]            ◉ 1,240         │
├──────────────────────────────────────────┬──────────────────────────────────┤
│ Limited · Drop 02      Ends 02:14:32     │ ┌────────────────────────────┐  │
│                                          │ │ ◆ FIGHTER  Re Mida     ▲   │  │
│   MILANO                                 │ │   T5 · Legendary · New     │  │
│   COUTURE                                │ │            ◆ 4,800         │  │
│   Pantera + skin + walkout + KO FX       │ └────────────────────────────┘  │
│                                          │ ┌────────────────────────────┐  │
│   [◆ 1,250 gems]  [Buy bundle]           │ │ ▼ SKIN     Cobalt Trackst..│  │
│                                          │ │   T3 · Frate skin          │  │
│                                          │ │            ◆ 320           │  │
│                                          │ └────────────────────────────┘  │
│                                          │ ┌────────────────────────────┐  │
│                                          │ │ ▒ KO FX    Gold Chain Snap │  │
│                                          │ │   T5 · Finisher animation  │  │
│                                          │ │            ◆ 600           │  │
│                                          │ └────────────────────────────┘  │
├──────────────────────────────────────────┴──────────────────────────────────┤
│ [PASS] Season 02 Battle Pass · Tier 17/50 · 33d left   ▰▰▰░░░░░  17/50  →   │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Behaviour:**
- Featured banner cycles 4 hero offers, 8s each (auto-advance, swipe to control).
- Battle Pass dock is sticky bottom across ALL shop tabs.
- Currency tab uses gold-tinted backgrounds; Cosmetics tab uses tier-color borders on items.

---

## 8. Combat FX language

Borrowed from arcade fighters (Street Fighter / Tekken), adapted for mobile screens.

### 8.1 Impact stack

Every successful hit spawns this stack at contact point, **z-ordered**, **timed**:

| Layer | Anim | Duration | Color |
|---|---|---|---|
| 1. Shock rings (3 concentric) | Scale 0 → 1.4, opacity 1 → 0 | 240ms ease-out | Outer magenta, mid cobalt, inner gold |
| 2. Core flash | Scale 0 → 1 → 0, full opacity | 120ms | Bone → gold radial |
| 3. Spark slashes (3–5 triangles) | Translate outward, fade out | 160ms ease-out | Gold + bone + magenta mix, angles 20°/45°/80° |
| 4. Impact word | Drop from above, rotate −4° to −8°, scale 1.2 → 1, fade after 800ms | 800ms | Gold fill, ink stroke 4px, magenta drop-shadow 6,6 |
| 5. Damage number | Float up 40px, fade after 600ms | 600ms | See §8.3 |
| 6. Screen shake | Trauma 0.6 for normal hit, 1.0 for crit | 200ms | — |

### 8.2 Impact words (per attack archetype)

| Attack | Word | Onomatopoeia |
|---|---|---|
| Light hit | "TAP!" / "POW!" | TMP/Display-KO |
| Heavy hit | "BOOM!" / "BANG!" | TMP/Display-KO |
| Special | "VAI!" / "GO!" | TMP/Display-KO + cobalt drop |
| Critical | "SPACCA!" / "BOMBA!" | TMP/Display-KO + gold glow |
| KO | "K.O." | Bigger, rotated −4°, screen freeze 400ms |
| Counter | "STOP!" | Magenta fill, bone stroke |
| Block break | "SNAP!" | Siren |
| Perfect round | "PERFETTO!" | gold + lime confetti |

### 8.3 Damage numerals

| Type | Style | Color | Size |
|---|---|---|---|
| Standard | Anton 56, no rotation | bone fill, 2px ink stroke | 56px |
| Critical | Anton 84, rotate −4°, +scale 1.1 | gold fill, 3px ink stroke, 4px magenta drop | 84px |
| Heal / pickup | Anton 52, prefix "+" | lime fill, 2px ink stroke | 52px |
| K.O. | Anton 96, rotate −4°, full HP-remaining as negative | siren fill, 3px ink stroke, 6px ink drop | 96px |
| Counter | Anton 64, italic, prefix "×N" | magenta fill, 2px ink stroke | 64px |

### 8.4 Combo counter

- Position: bottom-left of stage, z=8.
- Style: graffiti-xl (Bagel Fat One) lowercase "×N", cobalt fill, 2px ink stroke, 3px magenta drop.
- Sub-label "combo" in mono-xs underneath.
- Skews −4° on every +5 combo milestone (mini juice frame).
- Fades + slides down 24px after 1.2s of no hits.

### 8.5 Camera / screen effects

| Event | Effect |
|---|---|
| Normal hit | Trauma 0.4, 150ms decay |
| Heavy hit | Trauma 0.6, 200ms decay, 60ms freeze |
| Critical | Trauma 0.8, 300ms decay, 80ms freeze, radial chromatic-aberration pulse |
| KO | Trauma 1.0, 400ms decay, 400ms freeze, slow-mo to 0.3× for 800ms |
| Special trigger | Zoom in 1.15× for 500ms, magenta vignette pulse |
| Round end | Screen pulse white (bone) 1 frame, then fade ink |

Use Cinemachine Impulse Source on each hit. Vignette + chromatic aberration via PostProcessing volume keyed to player damage state.

### 8.6 Walkouts / intros

Each fighter has a 3-second walkout when match starts:

1. Fighter slides in from off-screen on a Vespa / skateboard / smoke cloud (signature prop).
2. Hashtag tag stickers onto screen (e.g. `#tropparoba` for Frate).
3. BPM-synced beat-drop on round-start, then HUD fades in.

---

## 9. Arenas

Four maps, each with a dominant palette and signature hazard.

| Arena | ID | Dominant | Hazards | Music BPM |
|---|---|---|---|---|
| Rome — Subway Lines | `arena/rome` | Ember `#FF7A2D` | Train pass-by every 30s; graffiti walls breakable | 90–110 |
| Milan — Fashion Show | `arena/milan` | Magenta `#FF2D7A` | Flash cameras (stun); runway models cross stage | 120–140 |
| Naples — Dock Brawl | `arena/naples` | Cobalt `#2EE5FF` | Scooter zooms; crate stacks topple | 100–130 |
| Sicily — Rooftop Sunset | `arena/sicily` | Gold `#FFD23F` | Tile sections collapse; rooftop edge KO ring-out | 80–100 |

When a player is on arena X, the win-screen, post-round VFX, and any podium use that arena's dominant color.

---

## 10. Unity implementation notes

### 10.1 Recommended folder structure

```
Assets/
  Art/
    Fonts/                  Anton.ttf, BagelFatOne.ttf, SpaceGrotesk-{wght}.ttf, SpaceMono-{wght}.ttf
    Fonts/TMP/              *.asset (SDF font assets)
    UI/
      Buttons/              btn_primary.png (9-slice), btn_primary_pressed.png ...
      Cards/
      HUD/                  hpbar_skewed.png, stamina.png, timer_ring.png, joystick.png
      Icons/                icon_microphone.png, icon_ko.png ...
      FX/                   impact_ring.png, spark_triangle.png, confetti_*.png
  Prefabs/
    UI/
      Buttons/              Btn_Primary.prefab, Btn_Cobalt.prefab ...
      Cards/                Card_Fighter.prefab, Card_Item.prefab ...
      HUD/                  Nameplate.prefab, RoundTimer.prefab, ActionCluster.prefab
      FX/                   ImpactBurst.prefab, ComboCounter.prefab, KoBanner.prefab
      Screens/              HUD.prefab, CharacterSelect.prefab, Shop.prefab
  Scripts/
    UI/
      Theme.cs              Static class: Colors.Magenta, Colors.Ink, etc.
      ButtonHardShadow.cs   Translates RectTransform on press
      HpBar.cs              Drives gradient + skew
      ImpactFx.cs           Spawns rings, sparks, word, dmg number from one call
      ComboCounter.cs
  Settings/
    UI/
      TMP_Style_Sheet.asset Reference to all type/* presets
      Theme.asset           ScriptableObject mirror of this MD (Colors, Sizes)
```

### 10.2 `Theme.cs` template

```csharp
// Generated from RAP_LEGENDS_DESIGN_SYSTEM.md §2
public static class RLColors {
    public static readonly Color Ink      = HexToColor("#0B0A14");
    public static readonly Color Ink2     = HexToColor("#14112A");
    public static readonly Color Ink3     = HexToColor("#1F1A3D");
    public static readonly Color InkLine  = HexToColor("#2A2350");
    public static readonly Color Bone     = HexToColor("#F5F0E1");
    public static readonly Color BoneMute = HexToColor("#C9C2AE");
    public static readonly Color Magenta  = HexToColor("#FF2D7A");
    public static readonly Color MagentaDeep = HexToColor("#B81658");
    public static readonly Color Cobalt   = HexToColor("#2EE5FF");
    public static readonly Color CobaltDeep = HexToColor("#1A8AA8");
    public static readonly Color Plum     = HexToColor("#6B2BD9");
    public static readonly Color Ember    = HexToColor("#FF7A2D");
    public static readonly Color Lime     = HexToColor("#B6FF3C");
    public static readonly Color Gold     = HexToColor("#FFD23F");
    public static readonly Color Siren    = HexToColor("#FF3B30");

    static Color HexToColor(string h) {
        ColorUtility.TryParseHtmlString(h, out var c); return c;
    }
}

public static class RLRarity {
    public static (Color top, Color bottom, Color text) Get(int tier) => tier switch {
        1 => (HexToColor("#2A2A3A"), HexToColor("#1A1A28"), HexToColor("#B5B0C8")),
        2 => (HexToColor("#00B0A8"), HexToColor("#006B68"), RLColors.Bone),
        3 => (HexToColor("#2EE5FF"), HexToColor("#1E7BD8"), RLColors.Ink),
        4 => (HexToColor("#FF2D7A"), HexToColor("#6B2BD9"), RLColors.Bone),
        5 => (HexToColor("#FFD23F"), HexToColor("#FF7A2D"), RLColors.Ink),
        _ => (RLColors.Ink2, RLColors.Ink, RLColors.Bone),
    };
}

public static class RLSpacing {
    public const float Xs = 4, S = 8, M = 12, L = 16, Xl = 24, Xxl = 32, Xxxl = 48, Huge = 64;
}
```

### 10.3 Canvas setup

- **Canvas Scaler**: Scale With Screen Size, reference 1920×1080, Match 0.5.
- **Pixel-perfect**: OFF (FX need sub-pixel motion).
- **Sort layers**: `Background (0) → Stage (5) → FX (10) → HUD (20) → Modal (30) → Toast (40)`.
- **Safe area**: Inset by `Screen.safeArea` on a top-level `SafeArea` RectTransform; iOS notch / Android cutout aware.

### 10.4 Animation timing reference

| Action | Curve | Duration |
|---|---|---|
| Button press | EaseOutCubic | 80ms in, 120ms out |
| Modal open | EaseOutBack (overshoot 1.1) | 240ms |
| Modal close | EaseInCubic | 160ms |
| HP drain | EaseOutQuad | 400ms |
| Combo +1 punch-scale | Spring (stiffness 700, damping 12) | 220ms |
| Impact ring expand | EaseOutQuart | 240ms |
| KO banner drop | EaseOutBack | 400ms in, hold 1.0s, EaseInBack 300ms out |
| Tab switch | EaseOutCubic | 200ms |
| Walkout intro | scripted, ~3.0s | — |

### 10.5 Audio cues (parallel UI/UX language)

Always pair UI events with sound. Empty silence is off-brand.

| Event | Audio family |
|---|---|
| Button press | Short kick + tape stop |
| Tab switch | Vinyl scratch (200ms) |
| Modal open | Sub-bass drop + crisp tick |
| Card flip / reveal (cosmetic) | DJ scratch + airhorn (rarity-tiered: T5 = airhorn cascade) |
| HP low (<25%) | Heartbeat sub at 60bpm |
| Round start | Beat drop (arena-specific BPM) |
| KO | Record scratch + crowd roar + bass hit |
| Special trigger | Voice tag ("ay!", "scialla!", "yeah!") |
| Purchase confirmed | Coin shower + air horn |

---

## 11. Do / Don't quick reference

### Do
- Use ALL CAPS for display, lowercase for graffiti, sentence-case for body, ALL CAPS for mono.
- Push every animation 10% further than feels comfortable. Bigger overshoot, harder shake.
- Subtitle every Italian word in EN.
- Keep the HUD readable at thumb distance — minimum tap target 44×44.
- Persist the magenta + cobalt + gold trio.
- Let cosmetics dominate the visual hierarchy in roster/shop.

### Don't
- Don't use pure black (`#000`) or pure white (`#FFF`). Use `#0B0A14` and `#F5F0E1`.
- Don't blur shadows. Hard cuts only.
- Don't add new colors without updating §2 first.
- Don't use Italian as a gate (no slang without subtitle).
- Don't mix display + graffiti in the same line.
- Don't let any text in HUD drop below 24px equivalent.
- Don't introduce emojis as UI elements (they're not part of the design DNA).
- Don't use 8 different border radii on one screen — pick from the scale.

---

## 12. Glossary

| Term | Meaning |
|---|---|
| **Drip** | Cosmetic / fashion stat. Drives DRIP attribute on fighter card. |
| **Walkout** | Pre-round intro animation + voice tag. |
| **KO FX** | Custom victory finisher animation (cosmetic, purchasable). |
| **Punchline / Bombarolo / Flowmaster** | Fighter classes (TBD final naming). |
| **Crew** | Future feature: 3v3 team mode. Naming reserved. |
| **Room** | Player home / studio for customization. |
| **Drop** | Limited-time seasonal cosmetic release. |
| **Pass** | Battle Pass / Season Pass. |
| **Cult / Hype / Leggenda** | Rarity tier names (T4 / T3 / T5 EN-Italian). |

---

## 13. Changelog

| Version | Date | Notes |
|---|---|---|
| 1.0 | May 2026 | Initial system. Phase II launch baseline. |

---

*If something here conflicts with `Rap Legends Brandbook.html`, the brandbook wins for visual reference but this file wins for token values and naming. Update both when you change either.*
