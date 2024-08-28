# ![EdgeGamers](assets/ego_text.webp) Jailbreak

[![Discord](https://img.shields.io/discord/161245089774043136?style=for-the-badge&logo=discord&logoColor=%23ffffff&label=Discord&color=%235865F2
)](https://edgm.rs/discord)

The classic Jail gamemode, ported to Counter-Strike 2.

## Downloads

[![Release](https://img.shields.io/badge/Release-mediumseagreen?style=for-the-badge&logo=onlyoffice
)](https://github.com/edgegamers/Jailbreak/releases/)⠀⠀
[![Stable](https://img.shields.io/badge/Stable-orangered?style=for-the-badge&logo=onlyoffice)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/main/jailbreak-nightly)
⠀⠀
[![Dev](https://img.shields.io/badge/Nightly-slateblue?style=for-the-badge&logo=onlyoffice
)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/dev/jailbreak-nightly)

**Release** builds are our full releases. We try to keep these high-quality and bug-free, when we can.
Our **Stable** builds run on EdgeGamers' own Jailbreak servers.
Our **Nightly** builds are used exclusively for development and staging, and are likely to have problems.

## Versioning

Our release tags starting from 'v2.0.0' follow the [Semantic Versioning 2.0.0](https://semver.org/) standard,
where `MAJOR.MINOR.PATCH` are incremented based on the following:

- `MAJOR` when we make incompatible API changes,
- `MINOR` when we add functionality in a backwards-compatible manner.
- `PATCH` when we make backwards-compatible bug fixes.

## Status

- **⚙️ Server**
  - [x] Stats/Analytics Sinks
  - [x] Error reporting
  - [x] Logging
  - [x] Zones
- **👮 Guards**
  - [x] Warden Selection
  - [x] Warden Laser and Paint
  - [x] Special Days
- **🎃 Prisoners**
  - [x] Last Request
  - [x] Rebel System
- **🛕 Maps**
  - [x] Automagic Cell Opening
  - [ ] Custom Entities
  - [ ] Custom I/O
  - [ ] Warden/Guard/Prisoner Filters

## Configuration

Configuration is done through CS#'s [FakeConVars](https://docs.cssharp.dev/examples/WithFakeConvars.html?q=fakeconvar).

You can search for the list of configurable
convars [like so](https://github.com/search?q=repo%3Aedgegamers%2FJailbreak%20fakeconvar&type=code).

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

The jailbreak plugin automatically builds to `build/Jailbreak` when
using `dotnet publish src/Jailbreak/Jailbreak.csproj`.
Please use [SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher.

Note that only the `src/Jailbreak` project is intended to be built directly.

## Using

Jailbreak requires Counter Strike Sharp. If you don't have that installed, [follow the
install instructions here](https://docs.cssharp.dev/docs/guides/getting-started.html).

Install the plugin like any other Counter Strike Sharp plugin: drop the `Jailbreak` folder into
`game/csgo/addons/counterstrikesharp/plugins`.
