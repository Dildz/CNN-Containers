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
    public override SemanticVersioning.Version Version { get; init; } = new("4.2.2");
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
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 1;
    [JsonPropertyName("gridH")]       public int GridH { get; init; }
    [JsonPropertyName("gridV")]       public int GridV { get; init; }
}

public record MapbookConfig
{
    [JsonPropertyName("enabled")]     public bool Enabled { get; init; } = true;
    [JsonPropertyName("price")]       public int Price { get; init; } = 48500;
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 1;
}

public record OnyxConfig
{
    [JsonPropertyName("enabled")]      public bool Enabled { get; init; } = true;
    [JsonPropertyName("dollarPrice")]  public int DollarPrice { get; init; } = 85000;
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; init; } = 4;
    [JsonPropertyName("grid1H")]       public int Grid1H { get; init; } = 2;
    [JsonPropertyName("grid1V")]       public int Grid1V { get; init; } = 3;
    [JsonPropertyName("grid2H")]       public int Grid2H { get; init; } = 3;
    [JsonPropertyName("grid2V")]       public int Grid2V { get; init; } = 4;
    [JsonPropertyName("grid3H")]       public int Grid3H { get; init; } = 1;
    [JsonPropertyName("grid3V")]       public int Grid3V { get; init; } = 2;
}

public record ModConfig
{
    [JsonPropertyName("gearBox")]      public ContainerConfig GearBox { get; init; } = new() { Price = 1118054, GridH = 12, GridV = 8 };
    [JsonPropertyName("modCase")]      public ContainerConfig ModCase { get; init; } = new() { Price = 434, GridH = 7, GridV = 7 };
    [JsonPropertyName("ammoBag")]      public ContainerConfig AmmoBag { get; init; } = new() { Price = 36980, GridH = 2, GridV = 2 };
    [JsonPropertyName("recycledFak")]  public ContainerConfig RecycledFak { get; init; } = new() { Price = 52572, GridH = 3, GridV = 3 };
    [JsonPropertyName("smallFridge")]  public ContainerConfig SmallFridge { get; init; } = new() { Price = 20690, GridH = 4, GridV = 4 };
    [JsonPropertyName("smallToolbox")] public ContainerConfig SmallToolbox { get; init; } = new() { Price = 118, GridH = 6, GridV = 4 };
    [JsonPropertyName("woodenBox")]    public ContainerConfig WoodenBox { get; init; } = new() { Price = 750000, GridH = 12, GridV = 8 };
    [JsonPropertyName("mapbook")]      public MapbookConfig Mapbook { get; init; } = new();
    [JsonPropertyName("onyx")]         public OnyxConfig Onyx { get; init; } = new();
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class CnnContainersLoader(
    ISptLogger<CnnContainersLoader> logger,
    ModHelper modHelper,
    DatabaseService databaseService,
    CustomItemService customItemService) : IOnLoad
{
    private ModConfig config = new();

    // Clone base - SICC organizational pouch (SimpleContainer, same parent node as our items)
    private const string CloneBase = "5d235bb686f77443f4331278";

    // Item hierarchy parent node (SimpleContainer)
    private const string ContainerParentId = "5795f317245977243854e041";

    // Handbook category (Containers)
    private const string HandbookParentId = "5b5f6fa186f77409407a7eb7";

    // Secure container parent node - used to find all secure containers to patch
    private const string SecureContainerParentId = "5448bf274bdc2dfc2f8b456a";

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

    // Mapbook item - slot-based container for maps (not grid-based, so handled separately)
    private const string MapbookId = "683d09d8a1b2c3d4e5f60001";
    private const string MapbookCloneBase = "5f4f9eb969cdc30ff33f09db";
    private const string MapbookParentId = "55818a104bdc2db9688b4569";
    private const string MapbookHandbookParentId = "5b47574386f77428ca22b345";

    // Onyx secure container - multi-grid secure container cloned from Kappa
    private const string OnyxId = "674a33573fef1c2943025680";
    private const string OnyxCloneBase = "5c093ca986f7740a1867ab12";      // Kappa
    private const string OnyxHandbookParentId = "5b5f6fd286f774093f2ed3c4"; // Secure Containers handbook
    private const string OnyxFilterInclude = "54009119af1c881c07000029";    // Item base category
    private const string OnyxFilterExclude = "5447e1d04bdc2dff2f8b4567";    // Weapons

    // Map template IDs (one per slot in the mapbook)
    private static readonly (string Name, string Id)[] MapIds =
    [
        ("Ground Zero", "6738033eb7305d3bdafe9518"),
        ("Streets",     "673803448cb3819668d77b1b"),
        ("Reserve",     "6738034a9713b5f42b4a8b78"),
        ("Labs",        "6738034e9d22459ad7cd1b81"),
        ("Lighthouse",  "6738035350b24a4ae4a57997"),
        ("Factory",     "574eb85c245977648157eec3"),
        ("Woods",       "5900b89686f7744e704a8747"),
        ("Interchange", "5be4038986f774527d3fae60"),
        ("Shoreline",   "5a8036fb86f77407252ddc02"),
        ("Customs",     "5798a2832459774b53341029"),
        ("Sanatorium",  "5a80a29286f7742b25692012"),
        ("Labyrinth",   "68f1ad32317cc52f4c0b6fae"),
    ];

    // Items allowed in both stash AND player inventory (backpacks, vests, secure containers)
    private static readonly string[] PortableItemIds =
    [
        AmmoBagId, RecycledFakId, SmallFridgeId, MapbookId
    ];

    // Items restricted to stash only - too bulky for player inventory
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
        bool CanRequireOnRagfair = false,
        int DiscardLimit = 0);

    private static readonly ContainerDef[] Containers =
    [
        new(GearBoxId,
            "Gear Box", "G-BOX", "A gear storage box.",
            5, 3, 12, 8,
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
            1308123, CanRequireOnRagfair: true),

        new(ModCaseId,
            "Mod Case", "M-CASE", "A weapon mod storage case.",
            3, 2, 7, 7,
            ["5448fe124bdc2da5018b4567"], // Weapon mods
            2.7, "grey",
            "assets/content/items/containers/item_container_modbox.bundle",
            51284),

        new(AmmoBagId,
            "Recycled Ammo Bag", "A-BAG", "A recycled IFAK to hold ammunition.",
            1, 1, 2, 2,
            [
                "5485a8684bdc2da71d8b4567", // Ammo packs
                "543be5cb4bdc2deb348b4568"  // Ammo
            ],
            0.4, "grey",
            "assets/content/items/containers/recycled_ammo_bag.bundle",
            43266, DiscardLimit: 1),

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
            61509),

        new(SmallFridgeId,
            "Small Portable Fridge", "SP-F", "A small portable fridge.",
            1, 1, 4, 4,
            ["543be6674bdc2df1348b4569"], // Food and drink
            2.36, "black",
            "assets/content/items/containers/small_portable_fridge.bundle",
            17684),

        new(SmallToolboxId,
            "Small Toolbox", "S-TB", "A small toolbox, probably stolen from a Scav.",
            3, 2, 6, 4,
            ["5448eb774bdc2d0a728b4567"], // Tools
            3.6, "blue",
            "assets/content/items/containers/small_toolbox.bundle",
            14045),

        new(WoodenBoxId,
            "Ruined Wooden Box", "RW-BOX",
            "A worn-out wooden crate, still sturdy enough to store just about anything.",
            5, 3, 12, 8,
            ["54009119af1c881c07000029"], // Item (accepts everything)
            11.4, "grey",
            "assets/content/items/containers/ruined_wooden_box.bundle",
            750000),
    ];

    // Trader assort entries: one item per trader, matching v3 prices/currencies exactly
    private static readonly (string TraderId, string ItemId, string CurrencyId, int Price, int LoyaltyLevel)[] TraderAssorts =
    [
        ("54cb50c76803fa8b248b4571", AmmoBagId,      Roubles, 36980,   1), // Prapor
        ("54cb57776803fa99248b456e", RecycledFakId,  Roubles, 52572,   1), // Therapist
        ("54cb57776803fa99248b456e", MapbookId,      Roubles, 48500,   1), // Therapist
        ("5935c25fb3acc3127c3d8cd9", ModCaseId,      Dollars, 434,     1), // Peacekeeper
        ("5a7c2eca46aef81a7ca2145d", SmallToolboxId, Euros,   118,     1), // Ragman
        ("5ac3b934156ae10c4430e83c", GearBoxId,      Roubles, 1118054, 1), // Mechanic
        ("5c0647fdd443bc2504c2d371", SmallFridgeId,  Roubles, 20690,   1), // Jaeger
        ("5c0647fdd443bc2504c2d371", WoodenBoxId,    Roubles, 750000,  1), // Jaeger
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

    public Task OnLoad()
    {
        LoadConfig();

        foreach (var def in Containers)
        {
            var cc = GetContainerConfig(def.Id);
            if (!cc.Enabled) continue;
            CreateContainer(def with { GridH = cc.GridH, GridV = cc.GridV });
        }

        if (config.Mapbook.Enabled) CreateMapbook();
        if (config.Onyx.Enabled) CreateOnyx();
        AddLocales();
        AddToTraderAssorts();
        PatchSecureContainers();
        PatchSpecialSlots();
        ExcludeFromEquipment();
        if (config.ModCase.Enabled && config.Mapbook.Enabled) ExcludeMapbookFromModCase();

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
        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = CloneBase,
            NewId = def.Id,
            ParentId = ContainerParentId,
            HandbookParentId = HandbookParentId,
            HandbookPriceRoubles = def.HandbookPrice,
            FleaPriceRoubles = def.HandbookPrice,
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
                                    ExcludedFilter = new HashSet<MongoId>()
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
        var slots = new List<Slot>();
        for (var i = 0; i < MapIds.Length; i++)
        {
            var (_, mapId) = MapIds[i];
            slots.Add(new Slot
            {
                Name = $"mod_mount_{(i + 1).ToString().PadLeft(2, '0')}",
                Id = new MongoId(),
                Parent = "55818b224bdc2dde698b456f",
                Properties = new SlotProperties
                {
                    Filters = new List<SlotFilter>
                    {
                        new SlotFilter
                        {
                            Filter = new HashSet<MongoId> { new MongoId(mapId) }
                        }
                    }
                },
                Required = false,
                MergeSlotWithChildren = false,
                Prototype = "55d4af244bdc2d962f8b4571"
            });
        }

        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = MapbookCloneBase,
            NewId = MapbookId,
            ParentId = MapbookParentId,
            HandbookParentId = MapbookHandbookParentId,
            HandbookPriceRoubles = 48500,
            FleaPriceRoubles = 48500,
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = "Secure Mapbook",
                        ShortName = "Mapbook",
                        Description = "A meticulously crafted book designed for storing and organizing maps."
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                Name = "Secure Mapbook",
                ShortName = "Mapbook",
                Description = "A meticulously crafted book designed for storing and organizing maps.",
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
                Grids = new List<Grid>(),
                Slots = slots
            }
        });
    }

    private void CreateOnyx()
    {
        var gridFilter = new List<GridFilter>
        {
            new GridFilter
            {
                Filter = new HashSet<MongoId> { new MongoId(OnyxFilterInclude) },
                ExcludedFilter = new HashSet<MongoId> { new MongoId(OnyxFilterExclude) }
            }
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

        customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = OnyxCloneBase,
            NewId = OnyxId,
            ParentId = SecureContainerParentId,
            HandbookParentId = OnyxHandbookParentId,
            HandbookPriceRoubles = 12999999,
            FleaPriceRoubles = 12999999,
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = "Secure Container Onyx",
                        ShortName = "OnyxSC",
                        Description = "A secret Black Division invention for maximum storage - the Onyx secured container."
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                Name = "Secure Container Onyx",
                ShortName = "OnyxSC",
                Description = "A secret Black Division invention for maximum storage - the Onyx secured container.",
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
                foreach (var def in Containers)
                {
                    if (!GetContainerConfig(def.Id).Enabled) continue;
                    data[$"{def.Id} Name"] = def.Name;
                    data[$"{def.Id} ShortName"] = def.ShortName;
                    data[$"{def.Id} Description"] = def.Description;
                }
                if (config.Mapbook.Enabled)
                {
                    data[$"{MapbookId} Name"] = "Secure Mapbook";
                    data[$"{MapbookId} ShortName"] = "Mapbook";
                    data[$"{MapbookId} Description"] = "A meticulously crafted book designed for storing and organizing maps.";
                }
                if (config.Onyx.Enabled)
                {
                    data[$"{OnyxId} Name"] = "Secure Container Onyx";
                    data[$"{OnyxId} ShortName"] = "OnyxSC";
                    data[$"{OnyxId} Description"] = "A secret Black Division invention for maximum storage - the Onyx secured container.";
                }
                return data;
            });
        }
    }

    private void AddToTraderAssorts()
    {
        foreach (var (traderId, itemId, currencyId, _, _) in TraderAssorts)
        {
            // Look up config for this item (mapbook uses its own config type)
            var isMapbook = itemId == MapbookId;
            var cc = isMapbook ? null : GetContainerConfig(itemId);
            var enabled = isMapbook ? config.Mapbook.Enabled : cc!.Enabled;
            var cfgPrice = isMapbook ? config.Mapbook.Price : cc!.Price;
            var cfgLoyalty = isMapbook ? config.Mapbook.LoyaltyLevel : cc!.LoyaltyLevel;

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

        // Onyx barter trade: Kappa + dollars from Peacekeeper
        if (config.Onyx.Enabled)
        {
            var onyxAssortId = new MongoId();
            var pkAssort = databaseService.GetTrader(new MongoId("5935c25fb3acc3127c3d8cd9"))?.Assort;
            if (pkAssort is not null)
            {
                pkAssort.Items.Add(new Item
                {
                    Id = onyxAssortId,
                    Template = new MongoId(OnyxId),
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        UnlimitedCount = true,
                        StackObjectsCount = 999
                    }
                });

                pkAssort.BarterScheme[onyxAssortId] = new List<List<BarterScheme>>
                {
                    new List<BarterScheme>
                    {
                        new BarterScheme { Template = new MongoId(OnyxCloneBase), Count = 1 },
                        new BarterScheme { Template = new MongoId(Dollars), Count = config.Onyx.DollarPrice }
                    }
                };

                pkAssort.LoyalLevelItems[onyxAssortId] = config.Onyx.LoyaltyLevel;
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
    private void PatchSecureContainers()
    {
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
                grid.Properties ??= new GridProperties();

                var filterList = grid.Properties.Filters?.ToList() ?? new List<GridFilter>();

                if (!filterList.Any())
                    filterList.Add(new GridFilter { Filter = new HashSet<MongoId>() });

                var filter = filterList.First();
                filter.Filter ??= new HashSet<MongoId>();

                foreach (var id in newItemMongoIds)
                    filter.Filter.Add(id);

                grid.Properties.Filters = filterList;
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
                grid.Properties ??= new GridProperties();

                var filterList = grid.Properties.Filters?.ToList() ?? new List<GridFilter>();

                if (!filterList.Any())
                    filterList.Add(new GridFilter
                    {
                        Filter = new HashSet<MongoId>(),
                        ExcludedFilter = new HashSet<MongoId>()
                    });

                var filter = filterList.First();
                filter.ExcludedFilter ??= new HashSet<MongoId>();

                foreach (var id in excludeIds)
                    filter.ExcludedFilter.Add(id);

                grid.Properties.Filters = filterList;
            }
        }
    }

    // The mapbook's parent falls under weapon mods, so exclude it from the mod case.
    private void ExcludeMapbookFromModCase()
    {
        var items = databaseService.GetItems();
        if (!items.TryGetValue(new MongoId(ModCaseId), out var modCase)) return;

        var grids = modCase.Properties?.Grids;
        if (grids == null) return;

        var mapbookMongoId = new MongoId(MapbookId);
        foreach (var grid in grids)
        {
            var filter = grid.Properties?.Filters?.FirstOrDefault();
            if (filter == null) continue;

            filter.ExcludedFilter ??= new HashSet<MongoId>();
            filter.ExcludedFilter.Add(mapbookMongoId);
        }
    }
}
