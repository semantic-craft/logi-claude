# Icon generation

`generate_icons.py` treats the eight SVG files in `assets/icons/masters/` as the geometry source of truth. It deterministically creates white Ring projections, 38% unavailable projections, black picker symbols, the plugin icon, and review sheets under `assets/icons/generated/`.

Run from any directory:

```sh
python3 CodexActionRingPlugin/tools/icons/generate_icons.py
python3 CodexActionRingPlugin/tools/icons/generate_icons.py --check
python3 -m unittest discover -s CodexActionRingPlugin/tests/IconSystem -v
```

PNG rendering requires `rsvg-convert`. Tests additionally use Pillow to inspect RGBA dimensions and the plugin-icon safe area. Package class-name mapping and copying remain the responsibility of I09.
