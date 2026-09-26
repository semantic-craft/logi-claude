# Claude Action Ring

<img src="ClaudeActionRingPlugin/assets/icons/generated/plugin/Icon256x256.png" width="96" alt="Claude Action Ring icon">

Thirteen Claude Desktop Code-tab actions for the Logitech MX Master 4 for Mac Actions Ring: eight for the default ring, five optional swaps.
Developed by **xianwei zhang**. Independent integration; not built, endorsed or supported by Anthropic or Logitech.

## Install

Requires macOS, Claude Desktop (Code tab), Logi Options+ with plugin support, and MX Master 4 for Mac.

1. Download [ClaudeActionRing_0_2_0.lplug4](ClaudeActionRingPlugin/artifacts/ClaudeActionRing_0_2_0.lplug4) using GitHub's download button and open it to install.
2. In Options+, select Claude Action Ring and assign any eight actions to the eight ring slots.
3. Keep Claude Desktop frontmost with the Code tab active. The actions send Claude's own documented Code-tab shortcuts, so they do nothing useful on the Chat or Cowork tabs.

| Action | Delivery | Default ring |
| --- | --- | --- |
| Permission Mode | Command–Shift–M | yes |
| Model | Command–Shift–I | yes |
| Effort | Command–Shift–E | yes |
| Side Chat | Command–; | yes |
| Toggle Browser | Command–Shift–B | yes |
| Toggle Terminal | Control–` | yes |
| View Mode | Control–O | yes |
| Next Session | Control–Tab | yes |
| Stop Response | Esc | optional |
| Select Element | Command–Shift–S | optional |
| New Session | Command–N | optional |
| Previous Session | Control–Shift–Tab | optional |
| Close Pane | Command–\ | optional |

Toggle Terminal is sent as the SDK key `Oem102`, which the Logi Plugin Service maps to the physical ` key on ANSI keyboards; Close Pane is sent as `Oem5` (the `\` key). Select Element needs the Browser pane open. Stop Response sends a bare Esc, which closes an open menu instead of stopping Claude when one is showing. Shortcut availability depends on your Claude Desktop version; the mapping follows the [Code tab keyboard shortcuts](https://code.claude.com/docs/en/desktop#keyboard-shortcuts) reference.

## Build and test

Use the .NET 10 SDK and the PluginApi.dll installed by Logitech's plugin service. The SDK is referenced locally and is not bundled in this repository. Python 3 is used for packaging and validation; icon generation also requires librsvg (`rsvg-convert`) and Pillow.

```sh
dotnet build ClaudeActionRingPlugin/ClaudeActionRingPlugin.sln -c Release
python3 -m unittest discover -s ClaudeActionRingPlugin/tests/Contracts -v
python3 -m unittest discover -s ClaudeActionRingPlugin/tests/IconSystem -v
```

Run each .NET test project under `ClaudeActionRingPlugin/tests/` with `dotnet test <project.csproj>`.
See [icons](ClaudeActionRingPlugin/tools/icons/README.md), [packaging](ClaudeActionRingPlugin/tools/package/README.md) and [validation](ClaudeActionRingPlugin/tools/validate/README.md) for the icon pipeline and the official LogiPluginTool workflow. Release versions are immutable; choose a new version when preparing a new package. The checked-in 0.2.0 package is the exact artifact produced by `package_release.py` from a Release build with `-p:DebugType=None -p:DebugSymbols=false` and verified by LogiPluginTool 6.1.4.22672; 0.1.0 is kept as the previous release.

## Privacy and support

The plugin only sends local keyboard shortcuts to Claude Desktop while it is the frontmost application. It does not read prompts, files, clipboard or audio and has no analytics or network client. Local diagnostic logs contain only the plugin version and anonymous error categories.

Report bugs through [GitHub Issues](https://github.com/semantic-craft/logi-claude/issues). Do not include private prompts, credentials or account details.

## License

Developer-owned code and icon assets are available under the [MIT License](LICENSE). Logitech SDK components and third-party trademarks retain their respective rights; no Logitech SDK binaries are included. Claude is a trademark of Anthropic, PBC.
