## Contributing

The jail plugin is currently in heavy development and all contributions are welcome!
Please make sure all contributions use the dependency injection system, or ask to have your contribution
ported if you don't know how.

> [!TIP]
> Microsoft has some good documentation on dependency injection here:
> [Overview](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection),
> [Using Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage),
> [Dependency Injection Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines).

All event handlers should derive from `IPluginBehavior` and be registered using
`IServiceCollection.AddPluginBehavior<T>`. If your behavior also acts as a service,
make sure to use `IServiceCollection.AddPluginBehavior<TInterface, T>`. All `IPluginBehavior` objects
have their event handlers automatically registered.

Code style should follow .NET conventions and use the formatting settings specified
in [Jailbreak.sln.DotSettings](./Jailbreak.sln.DotSettings)
(if you need help, make sure to check "enable edits from maintainers" and ask for a format)
