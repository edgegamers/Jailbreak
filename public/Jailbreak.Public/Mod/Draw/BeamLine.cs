using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamLine : BeamedShape
{
    private Vector end;

    public BeamLine(BasePlugin plugin, Vector position, Vector end) : base(plugin, position, 1)
    {
        this.end = end;
    }

    public void Move(Vector start, Vector end)
    {
        this.position = start;
        this.end = end;
    }

    public override void Draw()
    {
        var beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
        if (beam == null) return;
        beam.RenderMode = RenderMode_t.kRenderTransColor;
        beam.Render = GetColor();

        beam.Teleport(position, new QAngle(), new Vector());
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beams[0] = beam;

        Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
    }

    public override void Remove()
    {
        beams[0]?.Remove();
        beams[0] = null;
    }
}