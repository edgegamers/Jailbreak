using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///     Represents a shape that is drawn using many beam segments
/// </summary>
public abstract class BeamedShape : DrawableShape, IColorable
{
    protected BeamLine?[] Beams;
    protected int Resolution;

    protected BeamedShape(BasePlugin plugin, Vector position, int resolution) : base(plugin, position)
    {
        Beams = new BeamLine[resolution];
    }

    // TODO: Add support for rotation across arbitrary axis


    public void SetColor(Color color)
    {
        Color = color;
    }

    public Color Color { get; protected set; }

    protected override void RemoveInternal()
    {
        for (var i = 0; i < Beams.Length; i++)
        {
            Beams[i]?.Remove();
            Beams[i] = null;
        }
    }
}
