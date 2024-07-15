# ![EdgeGamers](assets/ego_text.webp) Jailbreak

[![Discord](https://img.shields.io/discord/161245089774043136?style=for-the-badge&logo=discord&logoColor=%23ffffff&label=Discord&color=%235865F2
)](https://edgm.rs/discord)

The classic Jail gamemode, ported to Counter-Strike 2.

## Downloads

[![Release](https://img.shields.io/badge/Release-mediumseagreen?style=for-the-badge&logo=onlyoffice
)](https://github.com/edgegamers/Jailbreak/releases/)⠀⠀
[![Stable](https://img.shields.io/badge/Stable-orangered?style=for-the-badge&logo=onlyoffice)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/main/jailbreak-nightly)⠀⠀
[![Dev](https://img.shields.io/badge/Nightly-slateblue?style=for-the-badge&logo=onlyoffice
)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/dev/jailbreak-nightly)

**Release** builds are our full releases. We try to keep these high-quality and bug-free, when we can.
Our **Stable** builds run on EdgeGamers' own Jailbreak servers. 
Our **Nightly** builds are used exclusively for development and staging, and are likely to have problems.

## Versioning
Our release tags starting from 'v1.2.0' follow the [Semantic Versioning 2.0.0](https://semver.org/) standard,
where `MAJOR.MINOR.PATCH` are incremented based on the following:
- `MAJOR` when we make incompatible API changes,
- `MINOR` when we add functionality in a backwards-compatible manner.
- `PATCH` when we make backwards-compatible bug fixes.

## Status

- **⚙️ Server**
  - [ ] Stats/Analytics Sinks
  - [ ] Error reporting
  - [x] Configuration system
      - Note: Passable, but in a terrible state. Needs TLC.
  - [x] Logging
- **👮 Guards**
  - [x] Warden Selection
  - [x] Warden Laser and Paint
  - [ ] Special Days
  - [x] Ratio Enforcement
  - [ ] Bans/Punishments
- **🎃 Prisoners**
  - [x] Last Request
  - [x] Rebel System
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

## Modding

Want to fork Jailbreak and add in your own custom behavior? No sweat!
The jailbreak repository is designed to act as a submodule.

```shell
git submodule add https://github.com/edgegamers/Jailbreak 
```

Once you have a dependency to `Jailbreak.Public`, you can add in whatever functionality
you want from the current plugin, and choose to add in your own handlers if you wish.
Don't forget to register them with the service container!

To boot your plugin, simply iterate over all services that inherit from `IPluginBehavior`,
as demonstrated in `src/Jailbreak/Jailbreak.cs`:

```cs
foreach (IPluginBehavior extension in _extensions)
{
    //	Register all event handlers on the extension object
    RegisterAllAttributes(extension);

    //	Tell the extension to start it's magic
    extension.Start(this);
}
```

## Building

The jailbreak plugin automatically builds to `build/Jailbreak` when using `dotnet publish src/Jailbreak/Jailbreak.csproj`.
Please use [SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher.

Note that only the `src/Jailbreak` project is intended to be built directly.

## Using

Jailbreak requires Counter Strike Sharp. If you don't have that installed, [follow the
install instructions here](https://docs.cssharp.dev/docs/guides/getting-started.html).

Install the plugin like any other Counter Strike Sharp plugin: drop the `Jailbreak` folder into
`game/csgo/addons/counterstrikesharp/plugins`.
