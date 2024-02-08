using Jailbreak.Drawable.Markers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Drawable;

public static class MarkerServiceExtensions
{
	public static void AddLasers(this IServiceCollection collection)
	{
		collection.AddSingleton<IDrawableFactory, DrawableFactory>();

		collection.AddPluginBehavior<IMarkerService, MarkerBehavior>();
		collection.AddPluginBehavior<MarkerListener>();
	}
}
