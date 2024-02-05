using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///     Represents a shape that is drawn using many beam segments
/// </summary>
public abstract class BeamedShape : DrawableShape, IColorable
{
    protected BeamLine?[] beams;
    protected Color color = Color.White;
    protected int resolution;

    protected BeamedShape(BasePlugin plugin, Vector position, int resolution) : base(plugin, position)
    {
        beams = new BeamLine[resolution];
    }

    // TODO: Add support for rotation across arbitrary axis

    public Color GetColor()
    {
        return color;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public override void Remove()
    {
        for (var i = 0; i < beams.Length; i++)
        {
            beams[i]?.Remove();
            beams[i] = null;
        }
    }
}