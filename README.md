# CNN-Containers

Port of [CNN-Containers Re-Upload](https://forge.sp-tarkov.com/mod/2167/cnn-containers-re-upload) mod, for SPT v4.0.X

Full rework of the CNN-CONTAINERS mod (v4) with additional cases added. Cases can now be configured via a config file.

## Installation

Download and extract the mod package zip into your SPT directory.

## Updating

Download, extract the mod package zip into your SPT directory and overwrite the contents when prompted.

> **v4.3.0 Notes:**
>
> - After an update, go to the SPT launcher's settings and click "Clean Temp Files" before launching.
> - v4.3.0 has breaking changes for some who already had the mod installed - READ THE RELEASE NOTES BEFORE UPDATING!

> **v4.4.0 Notes:**
>
> ### ⚠️ Secure Mapbook users - READ BEFORE UPDATING
>
> This version changes how the Secure Mapbook stores maps internally (the fix for the map-duplication bug). Because of that change, **any maps left _inside_ a mapbook in your existing profile will be permanently deleted** the first time the updated mod loads your profile - the maps sit in a slot layout the new mapbook no longer has, so the game removes them as orphaned items.
>
> **Before you update:** open your Secure Mapbook and move every map out into your stash. You can put them back after updating. Empty mapbooks are unaffected.
>
> **Already updated and lost your maps?**
> Here are some options:
>
> - Restore a profile backup from _before_ the update (SPT keeps backups under `user/`), downgrade to v4.3.0, move the maps out of the mapbook, then re-apply the update.
> - Use a dev account to buy & send yourself the missing maps
> - Buy them from the flea
>
> ---
>
> Onyx Secure container can now be purchased from **Peacekeeper** for **Dollars only**. This was added for those that need the Kappa to prestige. Dollars-only price can be configured - **current configs stay the same** - see below.

## Configuration

Edit `config/config.jsonc` to customize the mod. You can:

- Enable or disable individual containers
- Change trader prices and loyalty levels
- Resize container grids (except the Mapbook, whose cells are generated automatically, one per map)
- Resize each of the Onyx's 3 grids independently
- Set the price of the Onyx dollars-only barter (optional `dollarOnlyPrice`; omit the line to use the default $501,437)
- Set custom flea market prices (separate from trader prices)
- Add extra allowed item filters per container (for mod compatibility, e.g. Pack 'n' Strap)
- Rename or translate container names, short names, and descriptions

Optional settings are commented out below each container in the config - just uncomment and edit.

> **WARNING:** Disabling a container or changing its grid size after you've already used it in your profile (items stored inside) can cause profile corruption or item loss. Only change these settings on a fresh profile or after emptying the affected containers.

### Example: changing the Onyx dollars-only price

v4.4.0 adds a 3rd Onyx trade to Peacekeeper that can be bought for **dollars only** (no Kappa required). This barter costs **$501,437 by default**. That default is baked into the mod - there is **no `dollarOnlyPrice` line in the config unless you add it yourself**.

To change the price, open `config/config.jsonc`, find the `"onyx"` block, and add a `dollarOnlyPrice` line (a commented-out example is already there - just uncomment it and edit the number):

```jsonc
"onyx": {
    "enabled": true,
    "dollarPrice": 85259,
    "loyaltyLevel": 4,
    "grid1H": 2, "grid1V": 3,
    "grid2H": 3, "grid2V": 4,
    "grid3H": 1, "grid3V": 2,
    "dollarOnlyPrice": 750000   // <-- add this line to set your own price (here: $750,000)
}
```

Leave the line out entirely to keep the default of $501,437. Setting it to `0` also falls back to the default.

## Containers

Containers are tiered so new players get affordable starter storage while larger/general-purpose containers are gated behind higher trader loyalty.

### Tier 1

| Container                       | Type     | Grid | Description                                        | Trader                   |
| ------------------------------- | -------- | ---- | -------------------------------------------------- | ------------------------ |
| **Recycled Ammo Bag**     | Portable | 2x2  | Holds ammunition, fits inside Secure Containers    | Prapor LL1 - 75,000₽    |
| **Recycled FAK**          | Portable | 3x3  | Holds medical items, fits inside Secure Containers | Therapist LL1 - 85,000₽ |
| **Small Portable Fridge** | Portable | 2x3  | Holds food items, fits inside Secure Containers    | Jaeger LL1 - 38,000₽    |
| **Small Toolbox**         | Stash    | 4x6  | Holds tools/barter items                           | Skier LL1 - €500        |

### Tier 2

| Container                   | Type     | Grid    | Description                                                                                                    | Trader                   |
| --------------------------- | -------- | ------- | -------------------------------------------------------------------------------------------------------------- | ------------------------ |
| **Mod Case**          | Stash    | 6x5     | Holds weapon mod items                                                                                         | Peacekeeper LL2 - $1,200 |
| **Secure Mapbook**    | Portable | per-map | Holds one of each map, fits in Secure Containers and Special Slots. Cannot be insured (like all storage cases) | Therapist LL2 - 65,000₽ |
| **Ruined Wooden Box** | Stash    | 8x6     | General-purpose storage crate that accepts any item                                                            | Jaeger LL2 - 1,000,000₽ |

### Tier 3

| Container          | Type  | Grid | Description                                   | Trader                 |
| ------------------ | ----- | ---- | --------------------------------------------- | ---------------------- |
| **Gear Box** | Stash | 10x8 | Holds clothing (vests, rigs, backpacks, etc). | Ragman LL3 - 850,000₽ |

### Tier 4

| Container                       | Type   | Grid            | Description                                                           | Trader                                    |
| ------------------------------- | ------ | --------------- | --------------------------------------------------------------------- | ----------------------------------------- |
| **Secure Container Onyx** | Secure | 2x3 + 3x4 + 1x2 | Premium secure container with 3 grids. Follows default excluded items | Peacekeeper LL4 (barter: Kappa + $85,259) |

The Onyx is offered three ways by Peacekeeper: barter with the Kappa, barter with the Desecrated Kappa variant, or buy it with dollars only ($501,437) if you need your Kappa to prestige.

## Known Issues

- The Mod Case and Ruined Wooden Box models display with pink/purple textures (missing shaders). The bundles were built for an older Unity version and need to be recompiled for SPT v4.0's Unity version. Functionally they work fine.
- v4.4.0 reports its version as `4.3.0` in the server console (a leftover hardcoded version string). This is cosmetic only — the release is the correct v4.4.0 build. Already fixed in the code and will report correctly from the next release.

## Support

This mod is provided as-is, with no official support.

## License

This mod is licensed under the MIT License. See the LICENSE file for more details.

## Credits

- [Cannuccia](https://forge.sp-tarkov.com/user/16896/cannuccia) for the original CNN_Container mod.
- [AMightyTank](https://forge.sp-tarkov.com/user/59864/amightytank) for updating to SPT 3.11.X
- [MrVibesRSA](https://forge.sp-tarkov.com/user/75504/mrvibesrsa) for the Secure Mapbook mod.
- [Dsnyder](https://forge.sp-tarkov.com/user/28568/dsnyder) for the Container-Onyx (Re-Upload) mod.
- [TheSunGod](https://forge.sp-tarkov.com/user/108019/thesungod) for his time & patience with testing, bug reports & feedback.
