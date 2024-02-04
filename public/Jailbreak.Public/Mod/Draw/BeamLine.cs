using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamLine : DrawableShape, IColorable
{
    private Vector end;
    private Color color = Color.White;
    private CEnvBeam? beam;
    private float width = 1f;

    public BeamLine(BasePlugin plugin, Vector position, Vector end) : base(plugin, position)
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
        Remove(); 
        var beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
        if (beam == null) return;
        beam.RenderMode = RenderMode_t.kRenderTransColor;
        beam.Width = width;
        beam.Render = GetColor();

        beam.Teleport(position, new QAngle(), new Vector());
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        this.beam = beam;

        Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
    }

    public override void Remove()
    {
        beam?.Remove();
        beam = null;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public Color GetColor()
    {
        return color;
    }

    public void SetWidth(float width)
    {
        this.width = width;
    }

    public float GetWidth()
    {
        return width;
    }
}