# CNN-Containers

Port of [CNN-Containers Re-Upload](https://forge.sp-tarkov.com/mod/2167/cnn-containers-re-upload) mod, for SPT v4.0.X

Full rework of the CNN-CONTAINERS mod (v4) with additional cases added. Cases can now be configured via a config file.

## Installation

Download and extract the mod package zip into your SPT directory.

## Updating

Download, extract the mod package zip into your SPT directory and overwrite the contents when prompted.

> **Notes:**
> - After an update, go to the SPT launcher's settings and click "Clean Temp Files" before launching.
> - v4.3.0 will have breaking changes for some who already the mod installed - READ THE RELEASE NOTES BEFORE UPDATING!

## Configuration

Edit `config/config.jsonc` to customize the mod. You can:

- Enable or disable individual containers
- Change trader prices and loyalty levels
- Resize container grids (except the Mapbook, which uses fixed slots)
- Resize each of the Onyx's 3 grids independently
- Set custom flea market prices (separate from trader prices)
- Add extra allowed item filters per container (for mod compatibility, e.g. Pack 'n' Strap)
- Rename or translate container names, short names, and descriptions

Optional settings are commented out below each container in the config - just uncomment and edit.

> **WARNING:** Disabling a container or changing its grid size after you've already used it in your profile (items stored inside) can cause profile corruption or item loss. Only change these settings on a fresh profile or after emptying the affected containers.

## Containers

Containers are tiered so new players get affordable starter storage while larger/general-purpose containers are gated behind higher trader loyalty.

### Tier 1

| Container                 | Type     | Grid | Description                                        | Trader                  |
|---------------------------|----------|----- |----------------------------------------------------|-------------------------|
| **Recycled Ammo Bag**     | Portable | 2x2  | Holds ammunition, fits inside Secure Containers    | Prapor LL1 - 75,000₽    |
| **Recycled FAK**          | Portable | 3x3  | Holds medical items, fits inside Secure Containers | Therapist LL1 - 85,000₽ |
| **Small Portable Fridge** | Portable | 2x3  | Holds food items, fits inside Secure Containers    | Jaeger LL1 - 38,000₽    |
| **Small Toolbox**         | Stash    | 4x6  | Holds tools/barter items                           | Skier LL1 - €500        |

### Tier 2

| Container             | Type     | Grid       | Description                                                                | Trader                   |
|-----------------------|----------|------------|----------------------------------------------------------------------------|--------------------------|
| **Mod Case**          | Stash    | 6x5        | Holds weapon mod items                                                     | Peacekeeper LL2 - $1,200 |
| **Secure Mapbook**    | Portable | slot-based | Slot-based container for maps, fits in Secure Containers and Special Slots | Therapist LL2 - 65,000₽  |
| **Ruined Wooden Box** | Stash    | 8x6        | General-purpose storage crate that accepts any item                        | Jaeger LL2 - 1,000,000₽  |

### Tier 3

| Container    | Type  | Grid | Description                                                                    | Trader                |
|--------------|-------|------|--------------------------------------------------------------------------------|-----------------------|
| **Gear Box** | Stash | 10x8 | Holds clothing (vests, rigs, backpacks, etc). | Ragman LL3 - 850,000₽ |

### Tier 4

| Container                 | Type   | Grid            | Description                                                           | Trader                                    |
|---------------------------|--------|-----------------|-----------------------------------------------------------------------|-------------------------------------------|
| **Secure Container Onyx** | Secure | 2x3 + 3x4 + 1x2 | Premium secure container with 3 grids. Follows default excluded items | Peacekeeper LL4 (barter: Kappa + $85,259) |

The Onyx is listed twice with Peacekeeper - one with the Kappa as a barter, the other with the Desecrated Kappa variant.

## Known Issues

- The Mod Case and Ruined Wooden Box models display with pink/purple textures (missing shaders). The bundles were built for an older Unity version and need to be recompiled for SPT v4.0's Unity version. Functionally they work fine.

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
