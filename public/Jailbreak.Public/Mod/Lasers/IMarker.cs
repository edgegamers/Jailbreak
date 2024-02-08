namespace Jailbreak.Public.Mod.Draw;

public interface IMarker : IDrawable, IColorable
{
	float Radius { get; }

	void SetRadius(float radius);
}
