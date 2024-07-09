using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamLine(BasePlugin plugin, Vector position, Vector end) : DrawableShape(plugin, position), IColorable
{
    private CEnvBeam? _beam;
    private Color _color = Color.White;
    private float _width = 1f;

    public void SetColor(Color color)
    {
        _color = color;
    }

    public Color GetColor()
    {
        return _color;
    }

    public void Move(Vector start, Vector end1)
    {
        Position = start;
        end = end1;
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
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        _beam = beam;

        Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
    }

    public override void Remove()
    {
        KillTimer?.Kill();
        if (_beam != null && _beam.IsValid)
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