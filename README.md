# Ending Indicators

A `Hollow Knight: Silksong` mod that shows which endings were completed
for every save slot.

**Github Release**:
https://github.com/UserIsntAvailable/Silksong.EndingIndicators/releases/latest

<!-- TODO(Unavailable): **Thunderstore**: -->

## Installation

1. [Download and install BepInEx].
* You can follow this post if you want to run it through [Steam on Linux].
2. Ensure that the game has run at least once so that a `plugins`
   directory is generated in the `BepInEx` directory besides the game
   executable.
3. Download the .zip file from one of the sources listed above.
4. Extract the .zip file to the generated `plugins` directory.
5. Launch the game and enjoy!

## Configuration

A config file would be created, on first plugin load, under
`<SAVEFILES>/plugins/<PLUGINGUID>/config.dat`. You can use
[ConfigurationManager] to update and reload the config in-game; if you
prefer modifying config files by hand, you are gonna need to close and
reopen the game to see the changes.

(below there are the current available configurable options)

<!-- TODO(Unavailable): Automatically generate this section and make
it prettier (add some screenshots?). -->

```ini
## Plugin GUID: unavailable.ending-indicators

[Options]

## When the ending indicators should be displayed
# Setting type: EndingDisplayOpts
# Default value: OnlyCompleted
# Acceptable values: OnlyCompleted, ShowAllIfAnyCompleted, ShowAll
EndingDisplay = OnlyCompleted

## Reverse the order of the endings to match their natural order of unlocking
# Setting type: Boolean
# Default value: false
NaturalOrder = false
```

[Download and install BepInEx]: https://docs.bepinex.dev/articles/user_guide/installation/index.html
[Steam on Linux]: https://discord.com/channels/879125729936298015/1408110517049884783/1415820971507454164
[ConfigurationManager]: https://github.com/BepInEx/BepInEx.ConfigurationManager
