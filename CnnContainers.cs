using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;

namespace CnnContainers;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.cannuccia.cnn-containers";
    public override string Name { get; init; } = "CNN-Containers";
    public override string Author { get; init; } = "Cannuccia";
    public override List<string>? Contributors { get; init; }
    // Read from the assembly version (set by <Version> in the csproj) so version lives in ONE place
    // and can't drift out of sync with the release zip. ToString(3) turns 4.4.0.0 into "4.4.0".
    public override SemanticVersioning.Version Version { get; init; } =
        new(Assembly.GetExecutingAssembly().GetName().Version!.ToString(3));
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = true;
    public override string License { get; init; } = "MIT";
}

public record ContainerConfig
{
    [JsonPropertyName("enabled")]     public bool Enabled { get; init; } = true;
    [JsonPropertyName("price")]       public int Price { get; init; }
    [JsonPropertyName("fleaPrice")]   public int FleaPrice { get; init; }
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 1;
    [JsonPropertyName("gridH")]       public int GridH { get; init; }
    [JsonPropertyName("gridV")]       public int GridV { get; init; }
    [JsonPropertyName("extraFilters")]         public string[]? ExtraFilters { get; init; }
    [JsonPropertyName("extraExcludedFilters")] public string[]? ExtraExcludedFilters { get; init; }
    [JsonPropertyName("name")]        public string? Name { get; init; }
    [JsonPropertyName("shortName")]   public string? ShortName { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
}

public record MapbookConfig
{
    [JsonPropertyName("enabled")]     public bool Enabled { get; init; } = true;
    [JsonPropertyName("price")]       public int Price { get; init; } = 65000;
    [JsonPropertyName("fleaPrice")]   public int FleaPrice { get; init; }
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 2;
    [JsonPropertyName("name")]        public string? Name { get; init; }
    [JsonPropertyName("shortName")]   public string? ShortName { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
}

public record OnyxConfig
{
    [JsonPropertyName("enabled")]      public bool Enabled { get; init; } = true;
    [JsonPropertyName("dollarPrice")]  public int DollarPrice { get; init; } = 85000;
    // Optional: price of the dollars-only barter. Left 0 (default) means "not set" -> falls back to
    // OnyxDollarOnlyPrice in code, so admins never have to add this line unless they want to override it.
    [JsonPropertyName("dollarOnlyPrice")] public int DollarOnlyPrice { get; init; }
    [JsonPropertyName("fleaPrice")]    public int FleaPrice { get; init; }
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 4;
    [JsonPropertyName("grid1H")]       public int Grid1H { get; init; } = 2;
    [JsonPropertyName("grid1V")]       public int Grid1V { get; init; } = 3;
    [JsonPropertyName("grid2H")]       public int Grid2H { get; init; } = 3;
    [JsonPropertyName("grid2V")]       public int Grid2V { get; init; } = 4;
    [JsonPropertyName("grid3H")]       public int Grid3H { get; init; } = 1;
    [JsonPropertyName("grid3V")]       public int Grid3V { get; init; } = 2;
    [JsonPropertyName("extraFilters")]         public string[]? ExtraFilters { get; init; }
    [JsonPropertyName("extraExcludedFilters")] public string[]? ExtraExcludedFilters { get; init; }
    [JsonPropertyName("name")]        public string? Name { get; init; }
    [JsonPropertyName("shortName")]   public string? ShortName { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
}

public record ModConfig
{
    [JsonPropertyName("gearBox")]      public ContainerConfig GearBox { get; init; } = new() { Price = 850000, LoyaltyLevel = 3, GridH = 10, GridV = 8 };
    [JsonPropertyName("modCase")]      public ContainerConfig ModCase { get; init; } = new() { Price = 1200, LoyaltyLevel = 2, GridH = 6, GridV = 5 };
    [JsonPropertyName("ammoBag")]      public ContainerConfig AmmoBag { get; init; } = new() { Price = 75000, GridH = 2, GridV = 2 };
    [JsonPropertyName("recycledFak")]  public ContainerConfig RecycledFak { get; init; } = new() { Price = 85000, GridH = 3, GridV = 3 };
    [JsonPropertyName("smallFridge")]  public ContainerConfig SmallFridge { get; init; } = new() { Price = 38000, GridH = 2, GridV = 3 };
    [JsonPropertyName("smallToolbox")] public ContainerConfig SmallToolbox { get; init; } = new() { Price = 500, GridH = 4, GridV = 6 };
    [JsonPropertyName("woodenBox")]    public ContainerConfig WoodenBox { get; init; } = new() { Price = 1000000, LoyaltyLevel = 2, GridH = 8, GridV = 6 };
    [JsonPropertyName("mapbook")]      public MapbookConfig Mapbook { get; init; } = new();
    [JsonPropertyName("onyx")]         public OnyxConfig Onyx { get; init; } = new();
}

// Load order matters for the mapbook: it scans the DB for every map to build one cell per map, so
// all map items must exist when we run. This sets a narrow window:
//   - Lower bound: after map-adding mods. DynamicMaps registers its extra maps (Ground Zero, Streets,
//     Reserve, Labs, Lighthouse, Labyrinth) at PostDBModLoader + 90000, so we sit above that.
//   - Upper bound: before TraderRegistration (PostDBModLoader + 100000, i.e. OnLoadOrder value 500000).
//     The server snapshots trader assorts there; our Therapist mapbook assort must already be in the
//     DB, or it gets wiped on the first trader resupply.
// +99000 sits between the two. A map mod loading above +99000 would still be missed, but that window
// is tiny and no known map mod runs that late.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 99000)]
public class CnnContainersLoader(
    ISptLogger<CnnContainersLoader> logger,
    ModHelper modHelper,
    DatabaseService databaseService,
    CustomItemService customItemService) : IOnLoad
{
    private ModConfig config = new();
    private ContainerDef[] _resolvedContainers = [];

    // Clone base - SICC organizational pouch (SimpleContainer, same parent node as our items)
    private const string CloneBase = "5d235bb686f77443f4331278";

    // Item hierarchy parent node (SimpleContainer)
    private const string ContainerParentId = "5795f317245977243854e041";

    // Handbook category (Containers)
    private const string HandbookParentId = "5b5f6fa186f77409407a7eb7";

    // Secure container parent node - used to find all secure containers to patch
    private const string SecureContainerParentId = "5448bf274bdc2dfc2f8b456a";

    // Vanilla containers whose filters need patching to accept our custom items
    private const string ItemsCaseId      = "59fb042886f7746c5005a7b2";
    private const string ThiccItemsCaseId = "5c0a840b86f7742ffa4f2482";

    // Equipment parent nodes - used to exclude stash-only items from player inventory
    private const string BackpackParentId = "5448e53e4bdc2d60728b4567";
    private const string VestParentId     = "5448e5284bdc2dcb718b4567";
    private const string PocketsParentId  = "557596e64bdc2dc2118b4571";

    // Grid prototype shared by all vanilla simple containers
    private const string GridProto = "55d329c24bdc2d892f8b4567";

    // Currency template IDs
    private const string Roubles = "5449016a4bdc2d6f028b456f";
    private const string Dollars = "5696686a4bdc2da3298b456a";
    private const string Euros   = "569668774bdc2da2298b4568";

    // Custom item IDs (kept the same as v3 so existing profiles/saves stay compatible)
    private const string GearBoxId      = "683d0995deed9b8d4f897ec2";
    private const string ModCaseId      = "683d09aadb9e219d2f7bd6e8";
    private const string AmmoBagId      = "683d09b347163b4e7eaecfb1";
    private const string RecycledFakId  = "683d09bda7c49f4eead357ec";
    private const string SmallFridgeId  = "683d09c63c277dad20e4260b";
    private const string SmallToolboxId = "683d09cd0c8ec927b398b7b7";
    private const string WoodenBoxId   = "683d09e1f5a4b7c8d9e20003";

    // Mapbook item - a container that holds maps, one 1x1 cell per map (cells built dynamically at load).
    private const string MapbookId = "683d09d8a1b2c3d4e5f60001";
    private const string MapbookHandbookParentId = "5b47574386f77428ca22b345";
    private const string MapCategoryId = "567849dd4bdc2d150f8b456e"; // "Map" base class - every map item inherits from this

    // Onyx secure container - multi-grid secure container cloned from Kappa
    private const string OnyxId = "674a33573fef1c2943025680";
    private const string OnyxCloneBase = "5c093ca986f7740a1867ab12";     // Kappa
    private const string DesecratedKappaId = "676008db84e242067d0dc4c9"; // Desecrated Kappa
    private const string OnyxHandbookParentId = "5b5f6fd286f774093f2ecf0d"; // Secure Containers handbook category (matches Kappa's handbook.ParentId)
    private const string OnyxFilterInclude = "54009119af1c881c07000029";    // Item base category
    // Default price of the dollars-only Onyx barter: priced well above the Kappa top-up ($85,259) since
    // no Kappa is sacrificed. This is the fallback used when config's optional "dollarOnlyPrice" is unset,
    // so no config key is required - existing configs stay untouched on update. Admins can override in config.
    private const int OnyxDollarOnlyPrice = 501437;
    private const string OnyxFilterExclude = "5447e1d04bdc2dff2f8b4567";    // Weapons

    // Default display strings for the mapbook and Onyx. Kept as consts so the item's
    // creation code and its locale entries always agree - a single source of truth
    // for the fallback used when the config leaves name/shortName/description unset.
    private const string MapbookDefaultName        = "Secure Mapbook";
    private const string MapbookDefaultShortName   = "Mapbook";
    private const string MapbookDefaultDescription = "A meticulously crafted book designed for storing and organizing maps.";
    private const string OnyxDefaultName        = "Secure Container Onyx";
    private const string OnyxDefaultShortName   = "OnyxSC";
    private const string OnyxDefaultDescription = "A secret Black Division invention for maximum storage - the Onyx secured container.";

    // Items allowed in both stash AND player inventory
    private static readonly string[] PortableItemIds =
    [
        AmmoBagId, RecycledFakId, SmallFridgeId, MapbookId
    ];

    // Items restricted to stash only
    private static readonly string[] StashOnlyItemIds =
    [
        GearBoxId, ModCaseId, SmallToolboxId, WoodenBoxId
    ];

    // Per-container definition: all data needed to create the item and register it
    private record ContainerDef(
        string Id,
        string Name, string ShortName, string Description,
        int Width, int Height,
        int GridH, int GridV,
        string[] FilterIds,
        double Weight,
        string BackgroundColor,
        string BundlePath,
        int HandbookPrice,
        int FleaPrice = 0,
        bool CanRequireOnRagfair = false,
        int DiscardLimit = 0,
        string[]? ExcludedFilterIds = null);

    private static readonly ContainerDef[] Containers =
    [
        new(GearBoxId,
            "Gear Box", "G-BOX", "A gear storage box.",
            5, 3, 10, 8,
            [
                "57bef4c42459772e8d35a53b", // Armored equipment
                "5448e54d4bdc2dcc718b4568", // Armor vests
                "5448e5284bdc2dcb718b4567", // Tactical vests
                "5448e53e4bdc2d60728b4567", // Headwear
                "5645bcb74bdc2ded0b8b4578", // Visors
                "5448e5724bdc2ddf718b4568", // Face cover
                "5a341c4086f77401f2541505", // Headsets
                "5a341c4686f77469e155819e", // Armbands
                "5b3f15d486f77432d0509248"  // Clothing
            ],
            11.4, "grey",
            "assets/content/items/containers/item_container_gearbox.bundle",
            995000, CanRequireOnRagfair: true),

        new(ModCaseId,
            "Mod Case", "M-CASE", "A weapon mod storage case.",
            3, 2, 6, 5,
            ["5448fe124bdc2da5018b4567"], // Weapon mods
            2.7, "grey",
            "assets/content/items/containers/item_container_modbox.bundle",
            197000),

        new(AmmoBagId,
            "Recycled Ammo Bag", "A-BAG", "A recycled IFAK to hold ammunition.",
            1, 1, 2, 2,
            [
                "5485a8684bdc2da71d8b4567", // Ammo packs
                "543be5cb4bdc2deb348b4568"  // Ammo
            ],
            0.4, "grey",
            "assets/content/items/containers/recycled_ammo_bag.bundle",
            88000, DiscardLimit: 1),

        new(RecycledFakId,
            "Recycled First Aid Kit", "R-FAK",
            "A used and dirty first aid kit. It can be reused in a similar way.",
            2, 1, 3, 3,
            [
                "543be5664bdc2dd4348b4569", // Medical supplies
                "57864c8c245977548867e7f1"  // Medical equipment
            ],
            0.4, "orange",
            "assets/content/items/containers/recycled_fak.bundle",
            100000),

        new(SmallFridgeId,
            "Small Portable Fridge", "SP-F", "A small portable fridge.",
            1, 1, 2, 3,
            ["543be6674bdc2df1348b4569"], // Food and drink
            0.5, "black",
            "assets/content/items/containers/small_portable_fridge.bundle",
            45000),

        new(SmallToolboxId,
            "Small Toolbox", "S-TB", "A small toolbox, probably stolen from a Scav.",
            3, 2, 4, 6,
            ["5448eb774bdc2d0a728b4567"], // Tools
            3.6, "blue",
            "assets/content/items/containers/small_toolbox.bundle",
            76000),

        new(WoodenBoxId,
            "Ruined Wooden Box", "RW-BOX",
            "A worn-out wooden crate, still sturdy enough to store just about anything.",
            5, 3, 8, 6,
            ["54009119af1c881c07000029"], // Item (accepts everything)
            11.4, "grey",
            "assets/content/items/containers/ruined_wooden_box.bundle",
            1170000,
            ExcludedFilterIds: [WoodenBoxId]), // Never allow wooden boxes inside wooden boxes
    ];

    // Trader assort entries: which trader sells which item, and in what currency.
    // Price and loyalty level are NOT listed here - they come from config at load time
    // (see AddToTraderAssorts), so this table only needs the parts that never change.
    private static readonly (string TraderId, string ItemId, string CurrencyId)[] TraderAssorts =
    [
        ("54cb50c76803fa8b248b4571", AmmoBagId,      Roubles), // Prapor
        ("54cb57776803fa99248b456e", RecycledFakId,  Roubles), // Therapist
        ("54cb57776803fa99248b456e", MapbookId,      Roubles), // Therapist
        ("5935c25fb3acc3127c3d8cd9", ModCaseId,      Dollars), // Peacekeeper
        ("58330581ace78e27b8b10cee", SmallToolboxId, Euros),   // Skier
        ("5ac3b934156ae10c4430e83c", GearBoxId,      Roubles), // Ragman
        ("5c0647fdd443bc2504c2d371", SmallFridgeId,  Roubles), // Jaeger
        ("5c0647fdd443bc2504c2d371", WoodenBoxId,    Roubles), // Jaeger
    ];

    // Map container IDs to their config entry
    private ContainerConfig GetContainerConfig(string id) => id switch
    {
        GearBoxId      => config.GearBox,
        ModCaseId      => config.ModCase,
        AmmoBagId      => config.AmmoBag,
        RecycledFakId  => config.RecycledFak,
        SmallFridgeId  => config.SmallFridge,
        SmallToolboxId => config.SmallToolbox,
        WoodenBoxId    => config.WoodenBox,
        _ => new ContainerConfig()
    };

    // Apply config overrides (grid size, flea price, extra filters, and optional locale strings)
    private ContainerDef ResolveContainer(ContainerDef def)
    {
        var cc = GetContainerConfig(def.Id);
        var filterIds = cc.ExtraFilters is { Length: > 0 }
            ? [..def.FilterIds, ..cc.ExtraFilters]
            : def.FilterIds;

        // Merge baseline excludes (hard-coded on the ContainerDef) with any user-added excludes from config.
        // This keeps defaults like "wooden box cannot hold wooden boxes" in place even if the user adds their own list.
        var baselineExcluded = def.ExcludedFilterIds ?? Array.Empty<string>();
        var configExcluded = cc.ExtraExcludedFilters ?? Array.Empty<string>();
        var excludedFilterIds = baselineExcluded.Length == 0 && configExcluded.Length == 0
            ? null
            : baselineExcluded.Concat(configExcluded).Distinct().ToArray();

        return def with
        {
            // Guard against a blanked/zeroed grid size in config: if the user commented
            // out or set gridH/gridV to 0, fall back to the container's built-in size
            // (the literal in the Containers table) instead of creating a broken 0-cell grid.
            GridH             = cc.GridH > 0 ? cc.GridH : def.GridH,
            GridV             = cc.GridV > 0 ? cc.GridV : def.GridV,
            FleaPrice         = cc.FleaPrice,
            FilterIds         = filterIds,
            ExcludedFilterIds = excludedFilterIds,
            Name              = cc.Name        ?? def.Name,
            ShortName         = cc.ShortName   ?? def.ShortName,
            Description       = cc.Description ?? def.Description
        };
    }

    public Task OnLoad()
    {
        LoadConfig();
        _resolvedContainers = [..Containers.Select(ResolveContainer)];

        foreach (var def in _resolvedContainers)
        {
            if (!GetContainerConfig(def.Id).Enabled) continue;
            CreateContainer(def);
        }

        if (config.Mapbook.Enabled) CreateMapbook();
        if (config.Onyx.Enabled) CreateOnyx();
        AddLocales();
        AddToTraderAssorts();
        PatchSecureContainers();
        PatchSpecialSlots();
        ExcludeFromEquipment();
        PatchVanillaContainers();

        logger.Success("[CNN-Containers] Loaded successfully.");
        return Task.CompletedTask;
    }

    private void LoadConfig()
    {
        try
        {
            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            var configPath = System.IO.Path.Combine(modPath, "config", "config.jsonc");

            if (!File.Exists(configPath))
            {
                logger.Warning("[CNN-Containers] config/config.jsonc not found, using defaults.");
                return;
            }

            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            config = JsonSerializer.Deserialize<ModConfig>(json, options) ?? new ModConfig();
        }
        catch (Exception ex)
        {
            logger.Error($"[CNN-Containers] Failed to read config: {ex.Message}. Using defaults.");
            config = new ModConfig();
        }
    }

    private void CreateContainer(ContainerDef def)
    {
        var fleaPrice = def.FleaPrice > 0 ? def.FleaPrice : def.HandbookPrice;

        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = CloneBase,
            NewId = def.Id,
            ParentId = ContainerParentId,
            HandbookParentId = HandbookParentId,
            HandbookPriceRoubles = def.HandbookPrice,
            FleaPriceRoubles = fleaPrice,
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = def.Name,
                        ShortName = def.ShortName,
                        Description = def.Description
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                Name = def.Name,
                ShortName = def.ShortName,
                Description = def.Description,
                Width = def.Width,
                Height = def.Height,
                Weight = (float)def.Weight,
                BackgroundColor = def.BackgroundColor,
                CanPutIntoDuringTheRaid = true,
                CanSellOnRagfair = true,
                CanRequireOnRagfair = def.CanRequireOnRagfair,
                ExaminedByDefault = true,
                HideEntrails = true,
                InsuranceDisabled = false,
                ItemSound = "container_metal",
                DiscardLimit = def.DiscardLimit,
                MergesWithChildren = false,
                Prefab = new Prefab { Path = def.BundlePath },
                Grids = new List<Grid>
                {
                    new Grid
                    {
                        Id = new MongoId(),
                        Name = "main",
                        Parent = new MongoId(def.Id),
                        Prototype = new MongoId(GridProto),
                        Properties = new GridProperties
                        {
                            CellsH = def.GridH,
                            CellsV = def.GridV,
                            IsSortingTable = false,
                            MaxCount = 0,
                            MaxWeight = 0,
                            MinCount = 0,
                            Filters = new List<GridFilter>
                            {
                                new GridFilter
                                {
                                    Filter = new HashSet<MongoId>(
                                        def.FilterIds.Select(id => new MongoId(id))),
                                    ExcludedFilter = def.ExcludedFilterIds is { Length: > 0 }
                                        ? new HashSet<MongoId>(def.ExcludedFilterIds.Select(id => new MongoId(id)))
                                        : new HashSet<MongoId>()
                                }
                            }
                        }
                    }
                }
            }
        });
    }

    private void CreateMapbook()
    {
        var items = databaseService.GetItems();
        var grids = new List<Grid>();
        var gridNumber = 1;

        // Build one 1x1 grid per map by discovering every map in the server's item DB (all items
        // whose parent is the "Map" base class). This always uses the correct, current template IDs
        // and auto-includes new or modded maps - no hardcoded list to drift out of date.
        //  - Integer grid names ("1", "2", ...) make the grid contents count as regular container
        //    items - exactly like the pouches in a vanilla chest rig - rather than weapon-style
        //    attachments. This is what fixes the insurance duplication: SPT only duplicates slotted
        //    attachments, never plain container contents.
        //  - A 1x1 cell filtered to a single specific map means each map has exactly one home, so a
        //    second copy of the same map has nowhere to go (prevents duplicates inside the book).
        foreach (var (mapTpl, mapItem) in items)
        {
            if (!string.Equals(mapItem.Parent, MapCategoryId, StringComparison.OrdinalIgnoreCase))
                continue;

            grids.Add(new Grid
            {
                Id = new MongoId(),
                Name = gridNumber.ToString(),
                Parent = new MongoId(MapbookId),
                Prototype = new MongoId(GridProto),
                Properties = new GridProperties
                {
                    CellsH = 1,
                    CellsV = 1,
                    IsSortingTable = false,
                    MaxCount = 0,
                    MaxWeight = 0,
                    MinCount = 0,
                    Filters = new List<GridFilter>
                    {
                        new GridFilter
                        {
                            Filter = new HashSet<MongoId> { mapTpl },
                            ExcludedFilter = new HashSet<MongoId>()
                        }
                    }
                }
            });
            gridNumber++;
        }

        logger.Info($"[CNN-Containers] Mapbook: created {grids.Count} map cells.");

        var mapbookName        = config.Mapbook.Name        ?? MapbookDefaultName;
        var mapbookShortName   = config.Mapbook.ShortName   ?? MapbookDefaultShortName;
        var mapbookDescription = config.Mapbook.Description ?? MapbookDefaultDescription;
        var mapbookFleaPrice   = config.Mapbook.FleaPrice > 0 ? config.Mapbook.FleaPrice : 76000;

        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            // Clone the same simple-container base the other containers use, and parent it to
            // SimpleContainer so the client renders it as an openable grid container. The old
            // weapon-mod parent made the client show "Install" instead of "Open" - it never opened.
            ItemTplToClone = CloneBase,
            NewId = MapbookId,
            ParentId = ContainerParentId,
            HandbookParentId = MapbookHandbookParentId,
            HandbookPriceRoubles = 76000,
            FleaPriceRoubles = mapbookFleaPrice,
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = mapbookName,
                        ShortName = mapbookShortName,
                        Description = mapbookDescription
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                Name = mapbookName,
                ShortName = mapbookShortName,
                Description = mapbookDescription,
                Width = 1,
                Height = 2,
                Weight = 0.5f,
                BackgroundColor = "grey",
                CanPutIntoDuringTheRaid = true,
                CanSellOnRagfair = true,
                CanRequireOnRagfair = false,
                ExaminedByDefault = true,
                InsuranceDisabled = false,
                ItemSound = "item_book",
                MergesWithChildren = false,
                Prefab = new Prefab { Path = "assets/content/items/barter/item_mapbook/mapbook.bundle" },
                Grids = grids,
                Slots = new List<Slot>()
            }
        });
    }

    private void CreateOnyx()
    {
        // Inherit Kappa's exact grid filter (19-entry allow-list + 112-entry exclusion list)
        // so Onyx has the same item restrictions as a normal secure container. Cloning Kappa
        // would inherit these automatically, but OverrideProperties.Grids below replaces the
        // inherited grids with custom-sized ones, so we have to copy the filter explicitly.
        // Copy into new HashSets so ExtraFilters / ExtraExcludedFilters don't mutate Kappa.
        HashSet<MongoId> includeIds;
        HashSet<MongoId> excludeIds;

        var items = databaseService.GetItems();
        var kappaTpl = items.TryGetValue(new MongoId(OnyxCloneBase), out var tpl) ? tpl : null;
        var kappaFilter = kappaTpl?.Properties?.Grids?.FirstOrDefault()?.Properties?.Filters?.FirstOrDefault();

        if (kappaFilter?.Filter is { Count: > 0 } kf)
        {
            includeIds = new HashSet<MongoId>(kf);
            excludeIds = kappaFilter.ExcludedFilter is { } ke
                ? new HashSet<MongoId>(ke)
                : new HashSet<MongoId>();
        }
        else
        {
            // Fallback if Kappa's filter shape ever changes: allow-all minus weapons.
            logger.Warning("[CNN-Containers] Could not read Kappa filter - Onyx falling back to allow-all minus weapons.");
            includeIds = new HashSet<MongoId> { new MongoId(OnyxFilterInclude) };
            excludeIds = new HashSet<MongoId> { new MongoId(OnyxFilterExclude) };
        }

        if (config.Onyx.ExtraFilters is { Length: > 0 })
            foreach (var id in config.Onyx.ExtraFilters)
                includeIds.Add(new MongoId(id));

        if (config.Onyx.ExtraExcludedFilters is { Length: > 0 })
            foreach (var id in config.Onyx.ExtraExcludedFilters)
                excludeIds.Add(new MongoId(id));

        var gridFilter = new List<GridFilter>
        {
            new GridFilter { Filter = includeIds, ExcludedFilter = excludeIds }
        };

        Grid MakeGrid(string name, int cellsH, int cellsV) => new Grid
        {
            Id = new MongoId(),
            Name = name,
            Parent = new MongoId(OnyxId),
            Prototype = new MongoId(GridProto),
            Properties = new GridProperties
            {
                CellsH = cellsH,
                CellsV = cellsV,
                IsSortingTable = false,
                MaxCount = 0,
                MaxWeight = 0,
                MinCount = 0,
                Filters = gridFilter
            }
        };

        var onyxName        = config.Onyx.Name        ?? OnyxDefaultName;
        var onyxShortName   = config.Onyx.ShortName   ?? OnyxDefaultShortName;
        var onyxDescription = config.Onyx.Description ?? OnyxDefaultDescription;
        var onyxFleaPrice   = config.Onyx.FleaPrice > 0 ? config.Onyx.FleaPrice : 12999999;

        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = OnyxCloneBase,
            NewId = OnyxId,
            ParentId = SecureContainerParentId,
            HandbookParentId = OnyxHandbookParentId,
            HandbookPriceRoubles = 12999999,
            FleaPriceRoubles = onyxFleaPrice,
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = onyxName,
                        ShortName = onyxShortName,
                        Description = onyxDescription
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                Name = onyxName,
                ShortName = onyxShortName,
                Description = onyxDescription,
                CanSellOnRagfair = false,
                CanRequireOnRagfair = false,
                ExaminedByDefault = true,
                HideEntrails = true,
                InsuranceDisabled = true,
                ItemSound = "container_metal",
                DiscardLimit = -1,
                MergesWithChildren = false,
                Prefab = new Prefab { Path = "assets/content/items/containers/RopesContainer.bundle" },
                Grids = new List<Grid>
                {
                    MakeGrid("GridView (1)", config.Onyx.Grid1H, config.Onyx.Grid1V),
                    MakeGrid("GridView (2)", config.Onyx.Grid2H, config.Onyx.Grid2V),
                    MakeGrid("GridView (3)", config.Onyx.Grid3H, config.Onyx.Grid3V)
                }
            }
        });
    }

    // Add item name/shortname/description to all language locales via lazy transformer.
    // SPT lazy-loads locales so we must use AddTransformer rather than direct assignment.
    private void AddLocales()
    {
        foreach (var (_, localeKvP) in databaseService.GetLocales().Global)
        {
            localeKvP.AddTransformer(data =>
            {
                if (data is null) return data;
                foreach (var def in _resolvedContainers)
                {
                    if (!GetContainerConfig(def.Id).Enabled) continue;
                    data[$"{def.Id} Name"] = def.Name;
                    data[$"{def.Id} ShortName"] = def.ShortName;
                    data[$"{def.Id} Description"] = def.Description;
                }
                if (config.Mapbook.Enabled)
                {
                    data[$"{MapbookId} Name"] = config.Mapbook.Name        ?? MapbookDefaultName;
                    data[$"{MapbookId} ShortName"] = config.Mapbook.ShortName   ?? MapbookDefaultShortName;
                    data[$"{MapbookId} Description"] = config.Mapbook.Description ?? MapbookDefaultDescription;
                }
                if (config.Onyx.Enabled)
                {
                    data[$"{OnyxId} Name"] = config.Onyx.Name        ?? OnyxDefaultName;
                    data[$"{OnyxId} ShortName"] = config.Onyx.ShortName   ?? OnyxDefaultShortName;
                    data[$"{OnyxId} Description"] = config.Onyx.Description ?? OnyxDefaultDescription;
                }
                return data;
            });
        }
    }

    private void AddToTraderAssorts()
    {
        foreach (var (traderId, itemId, currencyId) in TraderAssorts)
        {
            // Look up config for this item (mapbook uses its own config type)
            var isMapbook = itemId == MapbookId;
            var cc = isMapbook ? null : GetContainerConfig(itemId);
            var enabled = isMapbook ? config.Mapbook.Enabled : cc!.Enabled;
            var cfgPrice = isMapbook ? config.Mapbook.Price : cc!.Price;
            var cfgLoyalty = isMapbook ? config.Mapbook.LoyaltyLevel : cc!.LoyaltyLevel;

            // Guard against a blanked/zeroed price in config: never sell an item for free.
            // Everything falls back to its handbook price (the mapbook's is 76000).
            if (cfgPrice <= 0)
                cfgPrice = isMapbook
                    ? 76000
                    : _resolvedContainers.FirstOrDefault(d => d.Id == itemId)?.HandbookPrice ?? 1;

            if (!enabled) continue;

            var assortItemId = new MongoId();
            var assort = databaseService.GetTrader(new MongoId(traderId))?.Assort;
            if (assort is null) continue;

            assort.Items.Add(new Item
            {
                Id = assortItemId,
                Template = new MongoId(itemId),
                ParentId = "hideout",
                SlotId = "hideout",
                Upd = new Upd
                {
                    UnlimitedCount = true,
                    StackObjectsCount = 999
                }
            });

            assort.BarterScheme[assortItemId] = new List<List<BarterScheme>>
            {
                new List<BarterScheme>
                {
                    new BarterScheme { Template = new MongoId(currencyId), Count = cfgPrice }
                }
            };

            assort.LoyalLevelItems[assortItemId] = cfgLoyalty;
        }

        // Onyx barter trades: Kappa + dollars OR Desecrated Kappa + dollars from Peacekeeper
        if (config.Onyx.Enabled)
        {
            var pkAssort = databaseService.GetTrader(new MongoId("5935c25fb3acc3127c3d8cd9"))?.Assort;
            if (pkAssort is not null)
            {
                // Barter 1: Regular Kappa + Dollars
                var onyxAssortId1 = new MongoId();
                pkAssort.Items.Add(new Item
                {
                    Id = onyxAssortId1,
                    Template = new MongoId(OnyxId),
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        UnlimitedCount = true,
                        StackObjectsCount = 999
                    }
                });

                pkAssort.BarterScheme[onyxAssortId1] = new List<List<BarterScheme>>
                {
                    new List<BarterScheme>
                    {
                        new BarterScheme { Template = new MongoId(OnyxCloneBase), Count = 1 },
                        new BarterScheme { Template = new MongoId(Dollars), Count = config.Onyx.DollarPrice }
                    }
                };

                pkAssort.LoyalLevelItems[onyxAssortId1] = config.Onyx.LoyaltyLevel;

                // Barter 2: Desecrated Kappa + Dollars
                var onyxAssortId2 = new MongoId();
                pkAssort.Items.Add(new Item
                {
                    Id = onyxAssortId2,
                    Template = new MongoId(OnyxId),
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        UnlimitedCount = true,
                        StackObjectsCount = 999
                    }
                });

                pkAssort.BarterScheme[onyxAssortId2] = new List<List<BarterScheme>>
                {
                    new List<BarterScheme>
                    {
                        new BarterScheme { Template = new MongoId(DesecratedKappaId), Count = 1 },
                        new BarterScheme { Template = new MongoId(Dollars), Count = config.Onyx.DollarPrice }
                    }
                };

                pkAssort.LoyalLevelItems[onyxAssortId2] = config.Onyx.LoyaltyLevel;

                // Barter 3: Dollars only (no Kappa sacrificed - costs more to compensate).
                // Lets players who want to keep their Kappa still buy the Onyx.
                // Uses the optional config price if an admin set one (>0), else the code default.
                var onyxDollarOnlyPrice = config.Onyx.DollarOnlyPrice > 0
                    ? config.Onyx.DollarOnlyPrice
                    : OnyxDollarOnlyPrice;
                var onyxAssortId3 = new MongoId();
                pkAssort.Items.Add(new Item
                {
                    Id = onyxAssortId3,
                    Template = new MongoId(OnyxId),
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        UnlimitedCount = true,
                        StackObjectsCount = 999
                    }
                });

                pkAssort.BarterScheme[onyxAssortId3] = new List<List<BarterScheme>>
                {
                    new List<BarterScheme>
                    {
                        new BarterScheme { Template = new MongoId(Dollars), Count = onyxDollarOnlyPrice }
                    }
                };

                pkAssort.LoyalLevelItems[onyxAssortId3] = config.Onyx.LoyaltyLevel;
            }
        }
    }

    private bool IsItemEnabled(string id) => id switch
    {
        MapbookId => config.Mapbook.Enabled,
        OnyxId    => config.Onyx.Enabled,
        _         => GetContainerConfig(id).Enabled
    };

    // Add only portable items to the filter of every secure container.
    // Skip "allow-all" containers - bots were spawning with empty secure containers.
    private void PatchSecureContainers()
    {
        var itemRootId = new MongoId(OnyxFilterInclude); // "54009119af1c881c07000029" - BaseClasses.ITEM
        var newItemMongoIds = PortableItemIds
            .Where(IsItemEnabled)
            .Select(id => new MongoId(id)).ToList();

        foreach (var (_, tpl) in databaseService.GetItems())
        {
            if (!string.Equals(tpl.Parent, SecureContainerParentId, StringComparison.OrdinalIgnoreCase))
                continue;

            var grids = tpl.Properties?.Grids;
            if (grids == null || !grids.Any()) continue;

            foreach (var grid in grids)
            {
                var filterList = grid.Properties?.Filters;
                if (filterList == null || !filterList.Any()) continue;

                var filter = filterList.First();
                filter.Filter ??= new HashSet<MongoId>();

                // Leave allow-all containers alone - adding entries would break the
                // fast-path and cause bots to spawn with empty secure containers.
                if (filter.Filter.Contains(itemRootId))
                    continue;

                foreach (var id in newItemMongoIds)
                    filter.Filter.Add(id);
            }
        }
    }

    // Add the mapbook to every special slot so it can be placed in the special slots.
    private void PatchSpecialSlots()
    {
        if (!config.Mapbook.Enabled) return;

        var mapbookMongoId = new MongoId(MapbookId);

        foreach (var (_, tpl) in databaseService.GetItems())
        {
            if (tpl.Properties?.Slots == null) continue;

            foreach (var slot in tpl.Properties.Slots)
            {
                if (slot?.Name == null || !slot.Name.Contains("SpecialSlot", StringComparison.OrdinalIgnoreCase))
                    continue;

                slot.Properties ??= new SlotProperties();

                var filterList = slot.Properties.Filters?.ToList() ?? new List<SlotFilter>();

                if (!filterList.Any())
                    filterList.Add(new SlotFilter { Filter = new HashSet<MongoId>() });

                var filter = filterList.First();
                filter.Filter ??= new HashSet<MongoId>();
                filter.Filter.Add(mapbookMongoId);

                slot.Properties.Filters = filterList;
            }
        }
    }

    // Prevent stash-only items from being placed in backpacks, vests, and pockets.
    private void ExcludeFromEquipment()
    {
        var excludeIds = StashOnlyItemIds
            .Where(IsItemEnabled)
            .Select(id => new MongoId(id)).ToHashSet();
        string[] equipmentParents = [BackpackParentId, VestParentId, PocketsParentId];

        foreach (var (_, tpl) in databaseService.GetItems())
        {
            if (!equipmentParents.Any(p => string.Equals(tpl.Parent, p, StringComparison.OrdinalIgnoreCase)))
                continue;

            var grids = tpl.Properties?.Grids;
            if (grids == null || !grids.Any()) continue;

            foreach (var grid in grids)
            {
                var filterList = grid.Properties?.Filters;
                if (filterList == null || !filterList.Any()) continue;

                var filter = filterList.First();
                filter.ExcludedFilter ??= new HashSet<MongoId>();

                foreach (var id in excludeIds)
                    filter.ExcludedFilter.Add(id);
            }
        }
    }

    // The Items Case and THICC Items Case use explicit allow-lists, so our
    // smaller container IDs must be added to their filters individually.
    // Gear Box and Wooden Box are excluded.
    private static readonly string[] VanillaCaseCompatibleIds =
    [
        ModCaseId, AmmoBagId, RecycledFakId, SmallFridgeId, SmallToolboxId, MapbookId
    ];

    private void PatchVanillaContainers()
    {
        var patchIds = VanillaCaseCompatibleIds
            .Where(IsItemEnabled)
            .Select(id => new MongoId(id))
            .ToList();

        if (!patchIds.Any()) return;

        string[] vanillaCaseIds = [ItemsCaseId, ThiccItemsCaseId];
        var items = databaseService.GetItems();

        foreach (var caseId in vanillaCaseIds)
        {
            if (!items.TryGetValue(new MongoId(caseId), out var caseTpl)) continue;

            var grids = caseTpl.Properties?.Grids;
            if (grids == null) continue;

            foreach (var grid in grids)
            {
                var filter = grid.Properties?.Filters?.FirstOrDefault();
                if (filter == null) continue;

                filter.Filter ??= new HashSet<MongoId>();
                foreach (var id in patchIds)
                    filter.Filter.Add(id);
            }
        }
    }
}
