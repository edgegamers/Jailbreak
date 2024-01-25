using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
/// Represents a shape that is drawn using many beam segments
/// </summary>
public abstract class BeamedShape : DrawableShape
{
    protected CEnvBeam[] beams;

    protected BeamedShape(BasePlugin plugin, Vector position, int resolution) : base(plugin, position)
    {
        beams = new CEnvBeam[resolution];
    }

    public override void Move(Vector position) {

    }
}