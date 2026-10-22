# Ward Is Love Weather Patch

A small companion patch for [Ward Is Love](https://thunderstore.io/c/valheim/p/Azumatt/WardIsLove/) that puts the weather back inside ward bubbles.

Newer versions of Ward Is Love hard-block weather inside wards with no option to disable it:

- Rain, snow and ash particles are hidden while you're inside an active ward bubble.
- Wards count as vanilla shield generators, and Valheim never applies the **Wet** debuff inside a shield.

This patch restores both, **without** touching the features you probably want to keep:

- `No Weather Damage` still protects structures inside the ward from rain decay.
- `No Heavy Snow Damage`, `No Ash Damage`, `No Lava Damage` are untouched.
- Everything else Ward Is Love does is untouched. The shield override only applies to the local player's Wet check, so plants, cinders and structures still treat wards as shields.

## Config

`BepInEx/config/com.devin.wardisloveweatherpatch.cfg`

| Setting | Default | Effect |
|---|---|---|
| `AllowWeatherParticlesInsideWard` | `true` | Rain/snow/ash render inside ward bubbles again. |
| `AllowWetDebuffInsideWard` | `true` | You get Wet in the rain inside wards again. Real roofs and vanilla shield generators still keep you dry. |

## Notes

- Soft dependency on Ward Is Love — loads after it so the patches land in the right order.
- If Ward Is Love updates and renames its internals, this mod logs a warning and quietly does nothing rather than breaking your game.

## Changelog

### 1.0.1
- Fixed the Wet debuff not applying inside wards on current Valheim.

### 1.0.0
- Initial release.
