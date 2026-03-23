# CNN-Containers

Port of [CNN-Containers Re-Upload](https://forge.sp-tarkov.com/mod/2167/cnn-containers-re-upload) mod, for SPT v4.0.X

Full rework of the CNN-CONTAINERS mod (v4) with additional cases added. Cases can now be configured via a config file.

## Installation

Download and extract the mod package zip into your SPT directory.

> **Note:** If you had a previous version of this mod installed, go to the SPT launcher's settings and click "Clean Temp Files" before launching.

## Configuration

Edit `config/config.jsonc` to customize the mod. You can:

- Enable or disable individual containers
- Change trader prices and loyalty levels
- Resize container grids (except the Mapbook, which uses fixed slots)
- Resize each of the Onyx's 3 grids independently

> **WARNING:** Disabling a container or changing its grid size after you've already used it in your profile (items stored inside) can cause profile corruption or item loss. Only change these settings on a fresh profile or after emptying the affected containers.

## Containers

| Container                 | Type       | Description                                                                             | Trader                                    |
|---------------------------|------------|-----------------------------------------------------------------------------------------|-------------------------------------------|
| **Recycled Ammo Bag**     | Portable   | Holds ammunition, fits inside Secure Containers                                         | Prapor LL1                                |
| **Mod Case**              | Stash only | Holds weapon mod items                                                                  | Peacekeeper LL1                           |
| **Small Toolbox**         | Stash only | Holds barter items                                                                      | Ragman LL1                                |
| **Gear Box**              | Portable   | Holds clothing (vests, rigs, backpacks). Can be listed on the Flea Market               | Mechanic LL1                              |
| **Recycled FAK**          | Portable   | Holds medical items, fits inside Secure Containers                                      | Therapist LL1                             |
| **Small Portable Fridge** | Portable   | Holds food items, fits inside Secure Containers                                         | Jaeger LL1                                |
| **Secure Mapbook**        | Portable   | Slot-based container for maps. Fits in Secure Containers and Special Slots              | Therapist LL1                             |
| **Secure Container Onyx** | Secure     | Premium secure container with 3 grids (2x3, 3x4, 1x2). Accepts all items except weapons | Peacekeeper LL4 (barter: Kappa + $85,000) |
| **Ruined Wooden Box**     | Stash only | Large storage crate that accepts any item                                               | Jaeger LL1                                |

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
