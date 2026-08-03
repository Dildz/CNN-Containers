# Changelog

Only user-facing changes are listed. Versions match the Forge releases (4.0.0, 4.2.2, 4.3.0, ...).

## [Unreleased]

## [5.0.0] - 2026-08-03

> ### ⚠️ SPT 4.1.X only — and the install folder changed
> SPT 4.1 refuses to load mods built against 4.0, so this build will not run on SPT 4.0 — stay on v4.4.0 if you haven't upgraded. SPT 4.1 also renamed its server folder from `SPT/` to `SPT_Runtime/`, so extract the zip into your **game root** and delete the old copy out of `SPT\user\mods\CNN-Containers`. Your existing config carries over unchanged.

### Changed
- Rebuilt for **SPT 4.1.X** on .NET 10, and migrated to 4.1's mod API: the new metadata interface, the async load lifecycle with shutdown cancellation support, and direct injection of the database tables now that the old database service is gone.
- Containers are now registered earlier in server startup. SPT 4.1 loads the database before anything else runs and asks mods that add their own items and trader offers to register at that point, so the containers and their trader assorts exist before any profile is loaded.
- Cloned items now carry their own internal name (the Gear Box identifies itself as `item_container_gearbox` rather than inheriting the name of the pouch it was cloned from). Cosmetic — it shows up in logs and debugging tools, not in game.

### Fixed
- Mod now reports its correct version in the server console. The version is read from the assembly (the csproj `<Version>`) instead of a separate hardcoded string, so it can't drift out of sync with the release again. (v4.4.0 displayed as `4.3.0` due to a stale hardcoded string — cosmetic only.)

### Internal
- CI now builds on every push and pull request instead of only when a release is tagged, so a broken commit is caught before release time rather than at it.

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
