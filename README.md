# Ward Is Love Weather Patch

A small companion patch for [Ward Is Love](https://thunderstore.io/c/valheim/p/Azumatt/WardIsLove/) that puts the weather back inside ward bubbles.

Newer versions of Ward Is Love hard-block weather inside wards with no option to disable it:

- Rain, snow and ash particles are hidden while you're inside an active ward bubble.
- The bubble's collider counts as a roof, so the **Wet** debuff (and shelter checks) never apply inside.

This patch restores both, **without** touching the features you probably want to keep:

- `No Weather Damage` still protects structures inside the ward from rain decay.
- `No Heavy Snow Damage`, `No Ash Damage`, `No Lava Damage` are untouched.
- Everything else Ward Is Love does is untouched — this mod only contains two Harmony postfix patches.

## Config

`BepInEx/config/com.devin.wardisloveweatherpatch.cfg`

| Setting | Default | Effect |
|---|---|---|
| `AllowWeatherParticlesInsideWard` | `true` | Rain/snow/ash render inside ward bubbles again. |
| `AllowWetDebuffInsideWard` | `true` | The ward dome no longer counts as a roof for cover checks, so you get Wet in the rain. Real roofs you build inside the ward still shelter you. |

## Notes

- Soft dependency on Ward Is Love — loads after it so the patches land in the right order.
- If Ward Is Love updates and renames its internals, this mod logs a warning and quietly does nothing rather than breaking your game.

## Changelog

### 1.0.0
- Initial release.
