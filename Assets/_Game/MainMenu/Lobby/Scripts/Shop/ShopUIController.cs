using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Drives the Shop UI Toolkit document.
/// Attach to the same GameObject as UIDocument.
/// Replaces GameShop.cs — wire this to your "Open Shop" button instead.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ShopUIController : MonoBehaviour
{
    // ── Scene references ─────────────────────────────────────────────────────
    [Header("Scene")]
    public GameObject normalCamera;
    public GameObject shopCamera;
    public CinemachineOrbitalFollow cameraOrbit;
    public MenuPlayerMove player;
    public GameObject house;
    public Transform normalPos;
    public Transform shopPos;
    public GameObject controls;

    [Tooltip("Optional: assign the HUD Canvas/root so it hides while the shop is open.")]
    public GameObject hud;

    [Header("Preview animator")]
    public Animator previewAnim;

    // fightStyleClips: one entry per fight style — used for tile count only (values unused)
    [Header("Fight Style count")]
    public string[] fightStyleClips;

    // ── State ────────────────────────────────────────────────────────────────
    private enum Screen  { CharacterPicker, Customization }
    private enum Tab     { Skin, FightStyle, Intro, HardPunch, HardKick, Celebration }

    private Screen _screen;
    private Tab    _tab;
    private string _currentAnimState;   // animator state name to watch for completion
    private int    _currentAnimLayer;   // layer the state lives on

    private int _charIndex;
    private int _skinIndex;
    private int _fightStyleIndex;
    private int _introIndex;
    private int _hardPunchIndex;
    private int _hardKickIndex;
    private int _celebrationIndex;

    // ── Data ─────────────────────────────────────────────────────────────────
    private AllPlayerData    _playerData;
    private AllFightShopData _fightData;

    // ── Cached UI elements ────────────────────────────────────────────────────
    private VisualElement _root;
    private Label         _cashLabel;
    private Label         _gemsLabel;

    private VisualElement _charPickerScreen;
    private VisualElement _customizationScreen;

    // Character picker
    private Button        _btnPrevChar;
    private Button        _btnNextChar;
    private Label         _charNameLabel;
    private Label         _charMetaLabel;
    private Button        _btnBuyChar;
    private Button        _btnSelectChar;
    private Button        _btnCustomizeChar;

    // Bottom nav
    private Button        _btnGoCharacters;
    private Button        _btnGoCustomize;

    // Nav tabs
    private Button _tabSkin, _tabFightStyle, _tabIntro,
                   _tabHardPunch, _tabHardKick, _tabCelebration;

    // Tab panels
    private VisualElement _panelSkin, _panelFightStyle, _panelIntro,
                          _panelHardPunch, _panelHardKick, _panelCelebration;

    // Item grids (contentContainer inside each ScrollView)
    private VisualElement _skinGrid, _fightStyleGrid, _introGrid,
                          _hardPunchGrid, _hardKickGrid, _celebrationGrid;

    // Action buttons per panel
    private Button _btnBuySkin,        _btnSelectSkin;
    private Button _btnBuyFightStyle,  _btnSelectFightStyle;
    private Button _btnBuyIntro,       _btnSelectIntro;
    private Button _btnBuyHardPunch,   _btnSelectHardPunch;
    private Button _btnBuyHardKick,    _btnSelectHardKick;
    private Button _btnBuyCelebration, _btnSelectCelebration;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public static ShopUIController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        var doc = GetComponent<UIDocument>();
        _root = doc.rootVisualElement;
        BindElements(_root);
        BindEvents();

        // Shop is hidden until OpenShop() is called
        _root.style.display = DisplayStyle.None;
    }

    private void Start()
    {
        _playerData = ResourceManager.Instance.playerData;
        _fightData  = AllFightShopData.Instance;
    }

    private void Update()
    {
        // Only update labels when the shop is visible
        if (_root.style.display == DisplayStyle.None) return;

        var save = SaveDataLocal.Instance;
        _cashLabel.text = "$ " + save.cash.ToString("N0");
        _gemsLabel.text = "◆ " + save.gems.ToString("N0");

        // Reset one-shot animations when they finish
        if (previewAnim && !string.IsNullOrEmpty(_currentAnimState))
        {
            var state = previewAnim.GetCurrentAnimatorStateInfo(_currentAnimLayer);
            if (state.IsName(_currentAnimState) && state.normalizedTime >= 1f)
            {
                previewAnim.SetBool("IsAttacking",    false);
                previewAnim.SetBool("ShowCelebration", false);
                previewAnim.SetBool("ShowIntro",       false);
                _currentAnimState = "";
            }
        }
    }

    // ── Element binding ───────────────────────────────────────────────────────
    private void BindElements(VisualElement root)
    {
        _cashLabel = root.Q<Label>("CashLabel");
        _gemsLabel = root.Q<Label>("GemsLabel");

        _charPickerScreen    = root.Q("CharacterPickerScreen");
        _customizationScreen = root.Q("CustomizationScreen");

        // Picker
        _btnPrevChar      = root.Q<Button>("BtnPrevChar");
        _btnNextChar      = root.Q<Button>("BtnNextChar");
        _charNameLabel    = root.Q<Label>("CharacterName");
        _charMetaLabel    = root.Q<Label>("CharacterMeta");
        _btnBuyChar       = root.Q<Button>("BtnBuyChar");
        _btnSelectChar    = root.Q<Button>("BtnSelectChar");
        _btnCustomizeChar = root.Q<Button>("BtnCustomizeChar");

        // Bottom nav
        _btnGoCharacters = root.Q<Button>("BtnGoCharacters");
        _btnGoCustomize  = root.Q<Button>("BtnGoCustomize");

        // Nav tabs
        _tabSkin        = root.Q<Button>("TabSkin");
        _tabFightStyle  = root.Q<Button>("TabFightStyle");
        _tabIntro       = root.Q<Button>("TabIntro");
        _tabHardPunch   = root.Q<Button>("TabHardPunch");
        _tabHardKick    = root.Q<Button>("TabHardKick");
        _tabCelebration = root.Q<Button>("TabCelebration");

        // Content panels
        _panelSkin        = root.Q("PanelSkin");
        _panelFightStyle  = root.Q("PanelFightStyle");
        _panelIntro       = root.Q("PanelIntro");
        _panelHardPunch   = root.Q("PanelHardPunch");
        _panelHardKick    = root.Q("PanelHardKick");
        _panelCelebration = root.Q("PanelCelebration");

        // Item grids — target the named VisualElement inside the ScrollView
        _skinGrid        = root.Q("SkinGrid");
        _fightStyleGrid  = root.Q("FightStyleGrid");
        _introGrid       = root.Q("IntroGrid");
        _hardPunchGrid   = root.Q("HardPunchGrid");
        _hardKickGrid    = root.Q("HardKickGrid");
        _celebrationGrid = root.Q("CelebrationGrid");

        // Action buttons
        _btnBuySkin           = root.Q<Button>("BtnBuySkin");
        _btnSelectSkin        = root.Q<Button>("BtnSelectSkin");
        _btnBuyFightStyle     = root.Q<Button>("BtnBuyFightStyle");
        _btnSelectFightStyle  = root.Q<Button>("BtnSelectFightStyle");
        _btnBuyIntro          = root.Q<Button>("BtnBuyIntro");
        _btnSelectIntro       = root.Q<Button>("BtnSelectIntro");
        _btnBuyHardPunch      = root.Q<Button>("BtnBuyHardPunch");
        _btnSelectHardPunch   = root.Q<Button>("BtnSelectHardPunch");
        _btnBuyHardKick       = root.Q<Button>("BtnBuyHardKick");
        _btnSelectHardKick    = root.Q<Button>("BtnSelectHardKick");
        _btnBuyCelebration    = root.Q<Button>("BtnBuyCelebration");
        _btnSelectCelebration = root.Q<Button>("BtnSelectCelebration");
    }

    private void BindEvents()
    {
        // Top bar
        GetComponent<UIDocument>().rootVisualElement
            .Q<Button>("BtnClose").clicked += OnClose;

        // Bottom nav
        _btnGoCharacters.clicked += () => ShowScreen(Screen.CharacterPicker);
        _btnGoCustomize.clicked  += () => ShowScreen(Screen.Customization);

        // Character picker
        _btnPrevChar.clicked      += OnPrevChar;
        _btnNextChar.clicked      += OnNextChar;
        _btnBuyChar.clicked       += OnBuyChar;
        _btnSelectChar.clicked    += OnSelectChar;
        _btnCustomizeChar.clicked += () => ShowScreen(Screen.Customization);

        // Nav tabs
        _tabSkin.clicked        += () => ShowTab(Tab.Skin);
        _tabFightStyle.clicked  += () => ShowTab(Tab.FightStyle);
        _tabIntro.clicked       += () => ShowTab(Tab.Intro);
        _tabHardPunch.clicked   += () => ShowTab(Tab.HardPunch);
        _tabHardKick.clicked    += () => ShowTab(Tab.HardKick);
        _tabCelebration.clicked += () => ShowTab(Tab.Celebration);

        // Action buttons
        _btnBuySkin.clicked           += () => OnBuy(Tab.Skin);
        _btnSelectSkin.clicked        += () => OnSelect(Tab.Skin);
        _btnBuyFightStyle.clicked     += () => OnBuy(Tab.FightStyle);
        _btnSelectFightStyle.clicked  += () => OnSelect(Tab.FightStyle);
        _btnBuyIntro.clicked          += () => OnBuy(Tab.Intro);
        _btnSelectIntro.clicked       += () => OnSelect(Tab.Intro);
        _btnBuyHardPunch.clicked      += () => OnBuy(Tab.HardPunch);
        _btnSelectHardPunch.clicked   += () => OnSelect(Tab.HardPunch);
        _btnBuyHardKick.clicked       += () => OnBuy(Tab.HardKick);
        _btnSelectHardKick.clicked    += () => OnSelect(Tab.HardKick);
        _btnBuyCelebration.clicked    += () => OnBuy(Tab.Celebration);
        _btnSelectCelebration.clicked += () => OnSelect(Tab.Celebration);
    }

    // ── Screen switching ──────────────────────────────────────────────────────
    private void ShowScreen(Screen screen)
    {
        _screen = screen;
        _charPickerScreen.style.display    = screen == Screen.CharacterPicker ? DisplayStyle.Flex : DisplayStyle.None;
        _customizationScreen.style.display = screen == Screen.Customization   ? DisplayStyle.Flex : DisplayStyle.None;

        _btnGoCharacters.EnableInClassList("shop-bottom-tab--active", screen == Screen.CharacterPicker);
        _btnGoCustomize.EnableInClassList("shop-bottom-tab--active",  screen == Screen.Customization);

        if (screen == Screen.CharacterPicker)
            RefreshCharacterPicker();
        else
            BuildSkinGrid(); // Rebuild skin grid when switching to customization (char may have changed)
    }

    // ── Tab switching ─────────────────────────────────────────────────────────
    private void ShowTab(Tab tab)
    {
        _tab = tab;

        // Hide all panels, deactivate all tabs
        SetTabActive(_panelSkin,        _tabSkin,        tab == Tab.Skin);
        SetTabActive(_panelFightStyle,  _tabFightStyle,  tab == Tab.FightStyle);
        SetTabActive(_panelIntro,       _tabIntro,       tab == Tab.Intro);
        SetTabActive(_panelHardPunch,   _tabHardPunch,   tab == Tab.HardPunch);
        SetTabActive(_panelHardKick,    _tabHardKick,    tab == Tab.HardKick);
        SetTabActive(_panelCelebration, _tabCelebration, tab == Tab.Celebration);

        RefreshGridSelection(tab);
        RefreshActionButtons(tab);
    }

    private static void SetTabActive(VisualElement panel, Button tab, bool active)
    {
        panel.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        tab.EnableInClassList("shop-nav-tab--active", active);
    }

    // ── Character picker ──────────────────────────────────────────────────────
    private void RefreshCharacterPicker()
    {
        var item = _playerData.player[_charIndex];

        _charNameLabel.text = item.playerName.ToUpper();
        _charMetaLabel.text = ""; // Populate when tier/class data is available

        bool owned = item.status == "owned";
        _btnBuyChar.style.display       = owned ? DisplayStyle.None : DisplayStyle.Flex;
        _btnSelectChar.style.display    = owned ? DisplayStyle.Flex : DisplayStyle.None;
        _btnCustomizeChar.style.display = owned ? DisplayStyle.Flex : DisplayStyle.None;

        if (!owned)
            _btnBuyChar.text = $"BUY · {item.cost}";

        _btnPrevChar.style.display = _charIndex > 0                               ? DisplayStyle.Flex : DisplayStyle.None;
        _btnNextChar.style.display = _charIndex < _playerData.player.Length - 1  ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateCharacterVisuals.Instance.ChangeCharacter(_charIndex, 0);
        RefreshPreviewAnimator();
    }

    private void OnPrevChar()
    {
        if (_charIndex > 0) { _charIndex--; RefreshCharacterPicker(); }
    }

    private void OnNextChar()
    {
        if (_charIndex < _playerData.player.Length - 1) { _charIndex++; RefreshCharacterPicker(); }
    }

    private void OnBuyChar()
    {
        var item = _playerData.player[_charIndex];
        if (item.cost > SaveDataLocal.Instance.cash) return;

        item.status = "owned";
        SaveDataLocal.Instance.cash -= item.cost;
        SaveDataLocal.Instance.SaveGame();
        RefreshCharacterPicker();
    }

    private void OnSelectChar()
    {
        var save = SaveDataLocal.Instance;
        save.currentPlayerIndex = _charIndex;
        save.currentSkinIndex   = 0;
        save.currentMovementType = _playerData.player[_charIndex].playerMovementType;
        save.SaveGame();

        _skinIndex = 0;
        BuildSkinGrid();
        ShowScreen(Screen.Customization);
        ShowTab(Tab.Skin);
    }

    // ── Grid builders ─────────────────────────────────────────────────────────
    private void BuildAllGrids()
    {
        BuildSkinGrid();
        BuildFightStyleGrid();
        BuildIntroGrid();
        BuildHardPunchGrid();
        BuildHardKickGrid();
        BuildCelebrationGrid();
    }

    private void BuildSkinGrid()
    {
        _skinGrid.Clear();
        var charItem = _playerData.player[SaveDataLocal.Instance.currentPlayerIndex];

        if (charItem.skins == null || charItem.skins.Length == 0)
        {
            // No purchasable skins yet — show owned base skin as a single tile
            var tile = MakeTile("BASE", "owned", 0, 0);
            tile.AddToClassList("shop-item-tile--selected");
            _skinGrid.Add(tile);
            return;
        }

        for (int i = 0; i < charItem.skins.Length; i++)
        {
            int idx = i;
            var skin = charItem.skins[i];
            string name = !string.IsNullOrEmpty(skin.displayName) ? skin.displayName : (i == 0 ? "BASE" : $"SKIN {i}");
            var tile = MakeTile(name, idx == 0 ? "owned" : skin.currentStatus, skin.itemCost, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.Skin, idx));
            _skinGrid.Add(tile);
        }
    }

    private void BuildFightStyleGrid()
    {
        _fightStyleGrid.Clear();
        // Fight styles are always unlocked — driven by clip count
        for (int i = 0; i < fightStyleClips.Length; i++)
        {
            int idx = i;
            var tile = MakeTile($"STYLE {i + 1}", "owned", 0, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.FightStyle, idx));
            _fightStyleGrid.Add(tile);
        }
    }

    private void BuildIntroGrid()
    {
        _introGrid.Clear();
        if (_fightData?.introItems == null) return;
        for (int i = 0; i < _fightData.introItems.Length; i++)
        {
            int idx = i;
            var d = _fightData.introItems[i];
            var tile = MakeTile($"INTRO {i + 1}", d.currentStatus, d.itemCost, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.Intro, idx));
            _introGrid.Add(tile);
        }
    }

    private void BuildHardPunchGrid()
    {
        _hardPunchGrid.Clear();
        if (_fightData?.hardPunchItems == null) return;
        for (int i = 0; i < _fightData.hardPunchItems.Length; i++)
        {
            int idx = i;
            var d = _fightData.hardPunchItems[i];
            var tile = MakeTile($"PUNCH {i + 1}", d.currentStatus, d.itemCost, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.HardPunch, idx));
            _hardPunchGrid.Add(tile);
        }
    }

    private void BuildHardKickGrid()
    {
        _hardKickGrid.Clear();
        if (_fightData?.hardKickItems == null) return;
        for (int i = 0; i < _fightData.hardKickItems.Length; i++)
        {
            int idx = i;
            var d = _fightData.hardKickItems[i];
            var tile = MakeTile($"KICK {i + 1}", d.currentStatus, d.itemCost, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.HardKick, idx));
            _hardKickGrid.Add(tile);
        }
    }

    private void BuildCelebrationGrid()
    {
        _celebrationGrid.Clear();
        if (_fightData?.celebrateItems == null) return;
        for (int i = 0; i < _fightData.celebrateItems.Length; i++)
        {
            int idx = i;
            var d = _fightData.celebrateItems[i];
            var tile = MakeTile($"CELEBRATE {i + 1}", d.currentStatus, d.itemCost, idx);
            tile.RegisterCallback<ClickEvent>(_ => OnTileClicked(Tab.Celebration, idx));
            _celebrationGrid.Add(tile);
        }
    }

    // ── Tile factory ──────────────────────────────────────────────────────────
    private static VisualElement MakeTile(string label, string status, int cost, int index)
    {
        var tile = new VisualElement();
        tile.AddToClassList("shop-item-tile");

        // Thumbnail placeholder
        var thumb = new VisualElement();
        thumb.AddToClassList("shop-item-thumb");
        tile.Add(thumb);

        // Name
        var nameEl = new Label(label.ToUpper());
        nameEl.AddToClassList("shop-item-name");
        tile.Add(nameEl);

        // Status / price
        bool owned = status == "owned";
        bool free  = cost == 0 && !owned;
        var  statusEl = new Label(owned ? "OWNED" : (free ? "FREE" : $"$ {cost}"));
        statusEl.AddToClassList(owned ? "shop-item-status--owned" : (free ? "shop-item-status--free" : "shop-item-status--price"));
        tile.Add(statusEl);

        if (owned) tile.AddToClassList("shop-item-tile--owned");

        return tile;
    }

    // ── Tile click / selection ────────────────────────────────────────────────
    private void OnTileClicked(Tab tab, int index)
    {
        switch (tab)
        {
            case Tab.Skin:
                _skinIndex = index;
                SaveDataLocal.Instance.currentSkinIndex = index;
                UpdateCharacterVisuals.Instance.UpdateVisuals();
                RefreshPreviewAnimator();
                break;
            case Tab.FightStyle:
                _fightStyleIndex = index;
                PlayAnimPreview(Tab.FightStyle, index);
                break;
            case Tab.Intro:
                _introIndex = index;
                PlayAnimPreview(Tab.Intro, index);
                break;
            case Tab.HardPunch:
                _hardPunchIndex = index;
                PlayAnimPreview(Tab.HardPunch, index);
                break;
            case Tab.HardKick:
                _hardKickIndex = index;
                PlayAnimPreview(Tab.HardKick, index);
                break;
            case Tab.Celebration:
                _celebrationIndex = index;
                PlayAnimPreview(Tab.Celebration, index);
                break;
        }

        RefreshGridSelection(tab);
        RefreshActionButtons(tab);
    }

    private void RefreshGridSelection(Tab tab)
    {
        VisualElement grid;
        int selected;
        switch (tab)
        {
            default:
            case Tab.Skin:        grid = _skinGrid;        selected = _skinIndex;        break;
            case Tab.FightStyle:  grid = _fightStyleGrid;  selected = _fightStyleIndex;  break;
            case Tab.Intro:       grid = _introGrid;       selected = _introIndex;       break;
            case Tab.HardPunch:   grid = _hardPunchGrid;   selected = _hardPunchIndex;   break;
            case Tab.HardKick:    grid = _hardKickGrid;    selected = _hardKickIndex;    break;
            case Tab.Celebration: grid = _celebrationGrid; selected = _celebrationIndex; break;
        }

        for (int i = 0; i < grid.childCount; i++)
            grid[i].EnableInClassList("shop-item-tile--selected", i == selected);
    }

    // ── Action buttons ────────────────────────────────────────────────────────
    private void RefreshActionButtons(Tab tab)
    {
        switch (tab)
        {
            case Tab.Skin:
            {
                var charItem = _playerData.player[SaveDataLocal.Instance.currentPlayerIndex];
                bool noSkins = charItem.skins == null || charItem.skins.Length == 0;
                bool owned   = noSkins || _skinIndex == 0 ||
                               (_skinIndex < charItem.skins.Length && charItem.skins[_skinIndex].currentStatus == "owned");
                int  cost    = (!noSkins && _skinIndex < charItem.skins.Length) ? charItem.skins[_skinIndex].itemCost : 0;
                SetActions(_btnBuySkin, _btnSelectSkin, owned, cost);
                break;
            }
            case Tab.FightStyle:
                // Fight styles always free/owned
                SetActions(_btnBuyFightStyle, _btnSelectFightStyle, true, 0);
                break;
            case Tab.Intro:
                if (_fightData?.introItems == null || _introIndex >= _fightData.introItems.Length) break;
                SetActions(_btnBuyIntro, _btnSelectIntro,
                    _fightData.introItems[_introIndex].currentStatus == "owned",
                    _fightData.introItems[_introIndex].itemCost);
                break;
            case Tab.HardPunch:
                if (_fightData?.hardPunchItems == null || _hardPunchIndex >= _fightData.hardPunchItems.Length) break;
                SetActions(_btnBuyHardPunch, _btnSelectHardPunch,
                    _fightData.hardPunchItems[_hardPunchIndex].currentStatus == "owned",
                    _fightData.hardPunchItems[_hardPunchIndex].itemCost);
                break;
            case Tab.HardKick:
                if (_fightData?.hardKickItems == null || _hardKickIndex >= _fightData.hardKickItems.Length) break;
                SetActions(_btnBuyHardKick, _btnSelectHardKick,
                    _fightData.hardKickItems[_hardKickIndex].currentStatus == "owned",
                    _fightData.hardKickItems[_hardKickIndex].itemCost);
                break;
            case Tab.Celebration:
                if (_fightData?.celebrateItems == null || _celebrationIndex >= _fightData.celebrateItems.Length) break;
                SetActions(_btnBuyCelebration, _btnSelectCelebration,
                    _fightData.celebrateItems[_celebrationIndex].currentStatus == "owned",
                    _fightData.celebrateItems[_celebrationIndex].itemCost);
                break;
        }
    }

    private static void SetActions(Button buyBtn, Button selectBtn, bool owned, int cost)
    {
        buyBtn.style.display    = owned ? DisplayStyle.None : DisplayStyle.Flex;
        selectBtn.style.display = owned ? DisplayStyle.Flex : DisplayStyle.None;
        if (!owned) buyBtn.text = cost > 0 ? $"BUY · {cost}" : "EQUIP";
    }

    // ── Buy ───────────────────────────────────────────────────────────────────
    private void OnBuy(Tab tab)
    {
        var save = SaveDataLocal.Instance;

        switch (tab)
        {
            case Tab.Skin:
            {
                var skins = _playerData.player[save.currentPlayerIndex].skins;
                if (skins == null || _skinIndex >= skins.Length) break;
                if (skins[_skinIndex].itemCost > save.cash) break;
                skins[_skinIndex].currentStatus = "owned";
                save.cash -= skins[_skinIndex].itemCost;
                save.SaveGame();
                BuildSkinGrid();
                break;
            }
            case Tab.Intro:
            {
                if (_fightData?.introItems == null || _introIndex >= _fightData.introItems.Length) break;
                if (BuyFightItem(_fightData.introItems[_introIndex])) BuildIntroGrid();
                break;
            }
            case Tab.HardPunch:
            {
                if (_fightData?.hardPunchItems == null || _hardPunchIndex >= _fightData.hardPunchItems.Length) break;
                if (BuyFightItem(_fightData.hardPunchItems[_hardPunchIndex])) BuildHardPunchGrid();
                break;
            }
            case Tab.HardKick:
            {
                if (_fightData?.hardKickItems == null || _hardKickIndex >= _fightData.hardKickItems.Length) break;
                if (BuyFightItem(_fightData.hardKickItems[_hardKickIndex])) BuildHardKickGrid();
                break;
            }
            case Tab.Celebration:
            {
                if (_fightData?.celebrateItems == null || _celebrationIndex >= _fightData.celebrateItems.Length) break;
                if (BuyFightItem(_fightData.celebrateItems[_celebrationIndex])) BuildCelebrationGrid();
                break;
            }
        }

        RefreshActionButtons(tab);
    }

    // Returns true if purchase succeeded
    private bool BuyFightItem<T>(T item) where T : class
    {
        // Use reflection to access currentStatus / itemCost (matches all four fight item types)
        var type   = item.GetType();
        var costF  = type.GetField("itemCost");
        var statF  = type.GetField("currentStatus");
        if (costF == null || statF == null) return false;

        int cost = (int)costF.GetValue(item);
        if (cost > SaveDataLocal.Instance.cash) return false;

        statF.SetValue(item, "owned");
        SaveDataLocal.Instance.cash -= cost;
        SaveDataLocal.Instance.SaveGame();
        _fightData.SaveData();
        return true;
    }

    // ── Select / Equip ────────────────────────────────────────────────────────
    private void OnSelect(Tab tab)
    {
        var save = SaveDataLocal.Instance;

        switch (tab)
        {
            case Tab.Skin:
                save.currentSkinIndex = _skinIndex;
                UpdateCharacterVisuals.Instance.UpdateVisuals();
                break;
            case Tab.FightStyle:
                save.currentIdleType = _fightStyleIndex;
                break;
            case Tab.Intro:
                // TODO: add save.currentIntroType when field is added to SaveDataLocal
                break;
            case Tab.HardPunch:
                save.currentHardPunchType = _hardPunchIndex;
                break;
            case Tab.HardKick:
                save.currentHardKickType = _hardKickIndex;
                break;
            case Tab.Celebration:
                save.currentCelebrationType = _celebrationIndex;
                break;
        }

        save.SaveGame();
        _fightData.SaveData();
        RefreshGridSelection(tab);
        RefreshActionButtons(tab);
    }

    // ── Preview animation ─────────────────────────────────────────────────────

    // After ChangeCharacter the old prefab is destroyed and a new one is spawned.
    // Re-grab the Animator from the freshly instantiated child.
    private void RefreshPreviewAnimator()
    {
        // Use lastSpawnedCharacter so we never accidentally grab the old prefab
        // that is queued for Destroy but not yet removed from the hierarchy.
        var spawned = UpdateCharacterVisuals.Instance?.lastSpawnedCharacter;
        if (spawned != null)
            previewAnim = spawned.GetComponentInChildren<Animator>(true);
    }

    /// <summary>
    /// Triggers an animation preview using the PlayerMovement animator parameters —
    /// NOT direct Play(clipName) calls, which bypass the state machine.
    /// </summary>
    private void PlayAnimPreview(Tab tab, int index)
    {
        if (previewAnim == null) return;

        // Clear any in-progress one-shot first
        previewAnim.SetBool("IsAttacking",     false);
        previewAnim.SetBool("ShowCelebration", false);
        previewAnim.SetBool("ShowIntro",       false);
        _currentAnimState = "";

        switch (tab)
        {
            case Tab.FightStyle:
                // Idle-pose swap — just update the blend tree parameter
                previewAnim.SetFloat("IdleType", (float)index);
                break;

            case Tab.HardPunch:
            {
                // BackUp controller has no HardPunchType parameter — drive by direct Play only.
                // States are StylishPunch1–StylishPunch4 on the FullBody layer (index 2).
                int punchNum   = Mathf.Clamp(index + 1, 1, 4);
                var punchState = "StylishPunch" + punchNum;
                previewAnim.SetBool("IsAttacking", true);
                previewAnim.Play(punchState, 2, 0f);
                _currentAnimState = punchState;
                _currentAnimLayer = 2;
                break;
            }

            case Tab.HardKick:
            {
                // BackUp controller has no HardKickType parameter — drive by direct Play only.
                // States are StylishKick1–StylishKick4 on the FullBody layer (index 2).
                int kickNum   = Mathf.Clamp(index + 1, 1, 4);
                var kickState = "StylishKick" + kickNum;
                previewAnim.SetBool("IsAttacking", true);
                previewAnim.Play(kickState, 2, 0f);
                _currentAnimState = kickState;
                _currentAnimLayer = 2;
                break;
            }

            case Tab.Celebration:
                // CelebrateType and ShowCelebration exist in BackUp controller.
                previewAnim.SetInteger("CelebrateType", index);
                previewAnim.SetBool("ShowCelebration", true);
                previewAnim.Play("Celebration", 0, 0f);
                _currentAnimState = "Celebration";
                _currentAnimLayer = 0;
                break;

            case Tab.Intro:
                // IntroType and ShowIntro exist in BackUp controller.
                previewAnim.SetInteger("IntroType", index);
                previewAnim.SetBool("ShowIntro", true);
                previewAnim.Play("Intro", 0, 0f);
                _currentAnimState = "Intro";
                _currentAnimLayer = 0;
                break;
        }
    }

    // ── Open / Close ──────────────────────────────────────────────────────────
    public void OpenShop()
    {
        _root.style.display = DisplayStyle.Flex;

        _charIndex        = SaveDataLocal.Instance.currentPlayerIndex;
        _skinIndex        = SaveDataLocal.Instance.currentSkinIndex;
        _fightStyleIndex  = SaveDataLocal.Instance.currentIdleType;
        _hardPunchIndex   = SaveDataLocal.Instance.currentHardPunchType;
        _hardKickIndex    = SaveDataLocal.Instance.currentHardKickType;
        _celebrationIndex = SaveDataLocal.Instance.currentCelebrationType;

        RefreshPreviewAnimator();   // grab fresh Animator reference on every open
        BuildAllGrids();
        ShowScreen(Screen.CharacterPicker);
        ShowTab(Tab.Skin);

        if (hud != null) hud.SetActive(false);

        normalCamera.SetActive(false);
        shopCamera.SetActive(true);
        cameraOrbit.HorizontalAxis.Value = 180;
        house.SetActive(false);
        controls.SetActive(false);

        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(normalPos.position);
        rb.MoveRotation(normalPos.rotation);
    }

    private void OnClose()
    {
        _root.style.display = DisplayStyle.None;

        SaveDataLocal.Instance.LoadGame();
        SavePlayerDataLocal.Instance.UpdateLoadData();
        _fightData.LoadData();
        UpdateCharacterVisuals.Instance.UpdateVisuals();

        if (hud != null) hud.SetActive(true);

        normalCamera.SetActive(true);
        shopCamera.SetActive(false);
        house.SetActive(true);
        controls.SetActive(true);

        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(shopPos.position);
        rb.MoveRotation(shopPos.rotation);
    }
}
