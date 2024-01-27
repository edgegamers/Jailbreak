﻿using System.Collections.Immutable;
using System.Reflection;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using Jailbreak.Public.Behaviors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Jailbreak;

public class Jailbreak : BasePlugin
{
	public override string ModuleName => "Jailbreak";
	public override string ModuleVersion => "0.1.0.{GIT_VERSION}";
	public override string ModuleAuthor => "EdgeGamers Development";

	private IServiceProvider _provider;
	private IServiceScope _scope;
	private IReadOnlyList<IPluginBehavior> _extensions;

	public Jailbreak(IServiceProvider provider)
	{
		_provider = provider;
	}

	public override void Load(bool hotReload)
	{
		Logger.LogInformation("[Jailbreak] Loading...");

		_scope = _provider.CreateScope();
		_extensions = _scope.ServiceProvider.GetServices<IPluginBehavior>()
			.ToImmutableList();

		Logger.LogInformation("[Jailbreak] Found {@BehaviorCount} behaviors.", _extensions.Count);

		foreach (IPluginBehavior extension in _extensions)
		{
			//	Register all event handlers on the extension object
			RegisterAllAttributes(extension);

			//	Tell the extension to start it's magic
			extension.Start(this);

			Logger.LogInformation("[Jailbreak] Loaded behavior {@Behavior}", extension.GetType().FullName);
		}

		base.Load(hotReload);
	}

	public override void Unload(bool hotReload)
	{
		Logger.LogInformation("[Jailbreak] Shutting down...");

		foreach (IPluginBehavior extension in _extensions)
		{
			extension.Dispose();
		}

		//	Dispose of original extensions scope
		//	When loading again we will get a new scope to avoid leaking state.
		_scope.Dispose();

		base.Unload(hotReload);
	}
}
