using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamCircle : BeamedShape
{
    private readonly BeamLine?[] _lines;
    private Vector[] _offsets;
    private float _radius;

    public BeamCircle(BasePlugin plugin, Vector position, float radius, int resolution) : base(plugin, position,
        resolution)
    {
        _radius = radius;
        _lines = new BeamLine[resolution];

        _offsets = GenerateOffsets();
    }

    private float DegToRadian(float d)
    {
        return (float)(d * (Math.PI / 180));
    }

    private Vector[] GenerateOffsets()
    {
        var offsets = new Vector[_lines.Length];
        var angle = 360f / _lines.Length;
        for (var i = 0; i < _lines.Length; i++)
        {
            var x = _radius * MathF.Cos(DegToRadian(angle * i));
            var y = _radius * MathF.Sin(DegToRadian(angle * i));
            offsets[i] = new Vector(x, y, 0);
        }

        return offsets;
    }

    public override void Draw()
    {
        for (var i = 0; i < _lines.Length; i++)
        {
            var line = _lines[i];
            var start = Position + _offsets[i];
            var end = Position + _offsets[(i + 1) % _offsets.Length];
            if (line == null)
            {
                line = new BeamLine(Plugin, start, end);
                line.SetColor(Color);
                line.Draw();
                _lines[i] = line;
            }
            else
            {
                line.Move(start, end);
                line.Update();
            }
        }
    }

    public void SetRadius(float radius)
    {
        _radius = radius;
        _offsets = GenerateOffsets();
    }
}