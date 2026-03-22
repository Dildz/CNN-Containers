using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
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
    public override SemanticVersioning.Version Version { get; init; } = new("4.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = true;
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class CnnContainersLoader(
    ISptLogger<CnnContainersLoader> logger,
    DatabaseService databaseService,
    CustomItemService customItemService) : IOnLoad
{
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

    // Items allowed in both stash AND player inventory (backpacks, vests, secure containers)
    private static readonly string[] PortableItemIds =
    [
        AmmoBagId, RecycledFakId, SmallFridgeId
    ];

    // Items restricted to stash only - too bulky for player inventory
    private static readonly string[] StashOnlyItemIds =
    [
        GearBoxId, ModCaseId, SmallToolboxId
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
    ];

    // Trader assort entries: one item per trader, matching v3 prices/currencies exactly
    private static readonly (string TraderId, string ItemId, string CurrencyId, int Price, int LoyaltyLevel)[] TraderAssorts =
    [
        ("54cb50c76803fa8b248b4571", AmmoBagId,      Roubles, 36980,   1), // Prapor
        ("54cb57776803fa99248b456e", RecycledFakId,  Roubles, 52572,   1), // Therapist
        ("5935c25fb3acc3127c3d8cd9", ModCaseId,      Dollars, 434,     1), // Skier
        ("5a7c2eca46aef81a7ca2145d", SmallToolboxId, Euros,   118,     1), // Ragman
        ("5ac3b934156ae10c4430e83c", GearBoxId,      Roubles, 1118054, 1), // Mechanic
        ("5c0647fdd443bc2504c2d371", SmallFridgeId,  Roubles, 20690,   1), // Jaeger
    ];

    public Task OnLoad()
    {
        foreach (var def in Containers)
            CreateContainer(def);

        AddLocales();
        AddToTraderAssorts();
        PatchSecureContainers();
        ExcludeFromEquipment();

        logger.Success("[CNN-Containers] Loaded successfully.");
        return Task.CompletedTask;
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
                    data[$"{def.Id} Name"] = def.Name;
                    data[$"{def.Id} ShortName"] = def.ShortName;
                    data[$"{def.Id} Description"] = def.Description;
                }
                return data;
            });
        }
    }

    private void AddToTraderAssorts()
    {
        foreach (var (traderId, itemId, currencyId, price, loyaltyLevel) in TraderAssorts)
        {
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
                    new BarterScheme { Template = new MongoId(currencyId), Count = price }
                }
            };

            assort.LoyalLevelItems[assortItemId] = loyaltyLevel;
        }
    }

    // Add only portable items to the filter of every secure container.
    private void PatchSecureContainers()
    {
        var newItemMongoIds = PortableItemIds.Select(id => new MongoId(id)).ToList();

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

    // Prevent stash-only items from being placed in backpacks, vests, and pockets.
    private void ExcludeFromEquipment()
    {
        var excludeIds = StashOnlyItemIds.Select(id => new MongoId(id)).ToHashSet();
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
}
