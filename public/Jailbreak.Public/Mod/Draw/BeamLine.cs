using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamLine : DrawableShape, IColorable
{
    private CEnvBeam? _beam;
    private Color _color = Color.White;
    private Vector _end;
    private float _width = 1f;

    public BeamLine(BasePlugin plugin, Vector position, Vector end) : base(plugin, position)
    {
        _end = end;
    }

    public void SetColor(Color color)
    {
        _color = color;
    }

    public Color GetColor()
    {
        return _color;
    }

    public void Move(Vector start, Vector end)
    {
        Position = start;
        _end = end;
    }

    public override void Draw()
    {
        Remove();
        var beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
        if (beam == null) return;
        beam.RenderMode = RenderMode_t.kRenderTransColor;
        beam.Width = _width;
        beam.Render = GetColor();

        beam.Teleport(Position, new QAngle(), new Vector());
        beam.EndPos.X = _end.X;
        beam.EndPos.Y = _end.Y;
        beam.EndPos.Z = _end.Z;
        _beam = beam;

        Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
    }

    public override void Remove()
    {
        KillTimer?.Kill();
        if(_beam != null && _beam.IsValid)
            _beam?.Remove();
        _beam = null;
    }

    public void SetWidth(float width)
    {
        _width = width;
    }

    public float GetWidth()
    {
        return _width;
    }
}