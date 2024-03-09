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
    private float _boltWidth = 1f;

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
        //beam.RenderMode = RenderMode_t.kRenderTransColor;
        // kRenderGlow wasn't too bad
        beam.RenderMode = RenderMode_t.kRenderWorldGlow;

        beam.ClipStyle = BeamClipStyle_t.kMODELCLIP; // clip to everything?
        beam.BoltWidth = _boltWidth;
        beam.NoiseAmplitude = 0;
        beam.Width = _width;
        beam.Render = GetColor();

        // hopefully enables the diameter flag??
        beam.BeamFlags |= (1 << 3);

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
        //if (!_beam!.IsValid) { return; } // I was getting errors without this
        _beam?.Remove();
        _beam = null;
    }

    public void SetBoltWidth(float width)
    {
        _boltWidth = width;
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