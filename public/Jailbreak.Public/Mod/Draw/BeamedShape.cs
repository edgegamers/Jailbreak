using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
/// Represents a shape that is drawn using many beam segments
/// </summary>
public abstract class BeamedShape : DrawableShape, IColorable
{
    protected CEnvBeam?[] beams;
    protected Color color = Color.White;

    protected BeamedShape(BasePlugin plugin, Vector position, int resolution) : base(plugin, position)
    {
        beams = new CEnvBeam[resolution];
    }

    public Color GetColor()
    {
        return color;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }
}