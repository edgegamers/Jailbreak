# ![EdgeGamers](assets/ego_text.webp) Jailbreak

[![Discord](https://img.shields.io/discord/161245089774043136?style=for-the-badge&logo=discord&logoColor=%23ffffff&label=Discord&color=%235865F2
)](https://edgm.rs/discord)

The classic Jail gamemode, ported to Counter-Strike 2.

> [!WARNING]
> This plugin is in active development and may cause server crashes or stability issues.

## Status

- **⚙️ Server**
  - [ ] Stats/Analytics Sinks
  - [ ] Error reporting
  - [ ] Configuration system
      - Note: Passable, but in a terrible state. Needs TLC.
- **👮 Guards**
  - [x] Warden Selection
  - [ ] Special Days
  - [ ] Ratio Enforcement
      - Note: Mostly there, needs debug.
  - [ ] Bans/Punishments
- **🎃 Prisoners**
  - [ ] Last Request
- **🛕 Maps**
  - [ ] Custom Entities
  - [ ] Custom I/O
  - [ ] Warden/Guard/Prisoner Filters

## Contributing

The jail plugin is currently in heavy development and all contributions are welcome!
Please make sure all contributions use the dependency injection system, or ask to have your contribution
ported if you don't know how.

Ports to DI containers that have more verbose scoping systems for round-based or game-based scoping are welcome.

> [!TIP]
> Microsoft has some good documentation on dependency injection here: 
> [Overview](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection),
> [Using Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage),
> [Dependency Injection Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines).

All event handlers should derive from `IPluginBehavior` and be registered using
`IServiceCollection.AddPluginBehavior<T>`. If your behavior also acts as a service,
make sure to use `IServiceCollection.AddPluginBehavior<TInterface, T>`. All `IPluginBehavior` objects
have their event handlers automatically registered.

Code style should follow .NET conventions
(if you need help, make sure to check "enable edits from maintainers" and ask for a format)

## Building

The jailbreak plugin automatically builds to `build/Jailbreak` when using `dotnet publish src/Jailbreak/Jailbreak.csproj`.
Please use [SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher.

Note that only the `src/Jailbreak` project is intended to be built directly.

## Using

Jailbreak requires Counter Strike Sharp. If you don't have that installed, [follow the
install instructions here](https://docs.cssharp.dev/docs/guides/getting-started.html).

Install the plugin like any other Counter Strike Sharp plugin: drop the `Jailbreak` folder into
`game/csgo/addons/counterstrikesharp/plugins`.
