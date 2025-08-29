# Instant Fertilizer™

[Valheim](https://store.steampowered.com/app/892970/Valheim/) mod that allows to fertilize plants (crops, trees) and pickables (berries, mushrooms, flowers) to grow / respawn them faster (or even instantaneously!).
Configurable fertilizer list (default: **3 Ancient Seeds** or **1 Ymir Flesh**) and remaining time reduction, enforceable server-side.
Translations available.

## Video showcase

https://github.com/nbusseneau/InstantFertilizer/assets/4659919/08ef5ded-b1bb-41f7-abd7-689072d7b3f3

## Features

- Fertilize plants (crops, trees) and pickables (berries, mushrooms, flowers) to make them grow or respawn faster, using items from a configurable fertilizer list.
  - Vines can also be fertilized, after which their pickable berries can also be fertilized, though it should be noted this is a bit finnicky and works best with fresh vines that were just grown rather than preexisting vines.
- By default, the fertilizer list allows using either **3 Ancient Seeds** (after **Eikthyr** has been defeated) or **1 Ymir Flesh** (after **The Elder** has been defeated).
  - If multiple fertilizers are in inventory, consumption priority is given based on list order: make sure to position less valuable fertilizers before more valuable ones in the list.
- By default, remaining time is reduced by 100% when fertilizing, meaning the effect is instantaneous, but this percentage can also be configured to a lower value. In this mode, a single plant / pickable can be fertilized multiple times with diminishing returns (but not more than once with the same fertilizer).
- Translations available: English, French. New languages can be added easily ([see below for details](#translations)).

## But why?

**Instant Fertilizer™** aims to address the annoyances around initially setting up a farm: the first few **Carrot seeds**, **Turnip seeds**, and **Onion seeds** you find in the wild can be quickly cycled with fertilizers in order to speed up getting to a sustainable state.

To achieve that, the default configuration aims to give a purpose to niche items with little use in vanilla by:

- Adding a new resource sink for **Ancient Seeds**, offering:
  - An interesting choice in early game between using your first few **Ancient Seeds** to set up your first **Carrot** farm or stocking up to summon **The Elder**.
  - An incentive in mid game / early late game to keep farming **Ancient Seeds** to set up your first **Turnip** or **Onion** farms.
- Adding a new resource sink for **Coins** via **Ymir Flesh**, offering an interesting choice in mid game / early late game between spending your **Coins** on fertilizers or stocking up to buy expensive **Eggs**.

## Compatibility

### With vanilla clients / clients not using the mod

**Plants** and **pickables** are managed by the current zone owner (usually the first person to enter a zone), however **Instant Fertilizer™** claims ownership of any plant or pickable on fertilizing.
It should thus work transparently with all clients, regardless of if they use the mod or not: the fertilized plant or pickable will grow or respawn instantaneously from their point of view as well (which might make you look suspicious if they're unaware).

### With other mods

**Instant Fertilizer™** hooks onto the `Pickable`, `Vine`, and `Plant` components and should work transparently with all mods, including those that add new **plants** or **pickables**.
In particular, **Instant Fertilizer™** is explicitly made to be compatible with [PlantEverything](https://thunderstore.io/c/valheim/p/Advize/PlantEverything/).
Feel free to [report any issue you find](https://github.com/nbusseneau/InstantFertilizer/issues/new).

### Translations

**Instant Fertilizer™** comes with the following languages available out of the box:

- English
- French

To add a new language as a user:

- Navigate to your `BepInEx/plugins/nbusseneau-Instant_Fertilizer/` directory.
  - From `r2modman`, use `Settings > Browse profile folder` to find your `BepInEx/` directory.
- Find the `Translations/` directory.
- Make a copy of the `English/` directory, then rename it to the appropriate name for your language (see [valid folder names](https://valheim-modding.github.io/Jotunn/data/localization/language-list.html)).
- Edit `<your_language_name>/InstantFertilizer.json` as appropriate using a text editor.

If you localize **Instant Fertilizer™** for your own language, you are most welcome to [send your translation file my way](https://github.com/nbusseneau/InstantFertilizer/issues/new), and I will integrate it as part of the languages available by default.

## Install

- This is a client-side mod, which can also be installed server-side.
- If installed on the server, server configuration will be enforced to all clients, however clients will not be forced to have the mod installed.

In other words:

- This mod can be installed on servers intended for Xbox crossplay, and all clients will still be able to join.
- This mod can be installed on your side as a client, and you will still be able to join any server (even vanilla ones).
  However, since this mod is not only cosmetic but impacts gameplay, you should probably ask the admin and other players first out of courtesy.

### Thunderstore (recommended)

- **[Prerequisite]** Install [**r2modman**](https://thunderstore.io/c/valheim/p/ebkr/r2modman/).
- Click **Install with Mod Manager** from the [mod page](https://thunderstore.io/c/valheim/p/nbusseneau/Instant_Fertilizer/).

### Manually (not recommended)

- **[Prerequisites]**
  - Install [**BepInExPack Valheim**](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
  - Install [**Jötunn, the Valheim Library**](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
- Download [nbusseneau-Instant_Fertilizer-0.3.0.zip](https://github.com/nbusseneau/InstantFertilizer/releases/latest/download/nbusseneau-Instant_Fertilizer-0.3.0.zip).
- Extract the archive and move everything to your `BepInEx/plugins/` directory. It should look like this:
  ```
  BepInEx/
  └── plugins/
      └── nbusseneau-Instant_Fertilizer/
          ├── CHANGELOG.md
          ├── icon.png
          ├── manifest.json
          ├── plugins/
          └── README.md
  ```

## Special thanks

**Instant Fertilizer™** is a reimplementation of a mod initially created by [warpalicious](https://thunderstore.io/c/valheim/p/warpalicious/).
Check out their POI content mods, you won't regret it 👍
