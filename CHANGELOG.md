# Changelog

Only user-facing changes are listed. Versions match the Forge releases (4.0.0, 4.2.2, 4.3.0, ...).

## [4.4.0] - 2026-07-05

> ### ⚠️ Breaking - empty your Secure Mapbook before updating
> The Secure Mapbook's internal storage changed from slots to grid cells (this is what fixes the map-duplication bug). Maps left **inside** a mapbook in an existing profile will be **permanently deleted** on the first load after updating, because they reference a slot layout the new mapbook no longer has. **Move all maps out of your mapbook into your stash before updating**, then put them back. Empty mapbooks are unaffected. If you already lost maps, restore a profile backup from before the update.

### Added
- **Onyx dollars-only barter.** Peacekeeper now offers the Onyx three ways: barter with the Kappa, barter with the Desecrated Kappa, or buy it for **dollars only** with no Kappa required. The dollars-only option costs $501,437 by default (higher than the Kappa barters, since no Kappa is sacrificed) so you can buy the Onyx without giving up your Kappa.
- **Optional `dollarOnlyPrice` config key** (under `onyx`) to set the price of that dollars-only barter. It ships commented out — add the line only if you want to change the default. Existing configs are untouched.

### Fixed
- **Secure Mapbook insurance duplication.** Maps stored in the mapbook could be duplicated through insurance. The mapbook is now a proper container (one cell per map, discovered automatically from the game — including modded maps), which makes the dupe impossible. As a side effect the mapbook, like all storage cases, can no longer be insured — this is expected EFT behaviour, not a bug.
- **Mapbook load order.** The mapbook now builds after map-adding mods (e.g. DynamicMaps), so modded maps also get a cell.
- **Config guards.** A blanked or zeroed price/grid size in config now falls back to a sensible default instead of 0 (no more free or zero-size items).
