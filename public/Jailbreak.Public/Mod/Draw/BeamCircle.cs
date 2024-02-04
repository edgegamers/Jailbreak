using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamCircle : BeamedShape
{

    private BeamLine?[] lines;
    private Vector[] offsets;
    private float radius;

    public BeamCircle(BasePlugin plugin, Vector position, float radius, int resolution) : base(plugin, position, resolution)
    {
        this.radius = radius;
        this.lines = new BeamLine[resolution];

        offsets = generateOffsets();
    }

    private float degToRadian(float d)
    {
        return (float)(d * (Math.PI / 180));
    }

    private Vector[] generateOffsets()
    {
        var offsets = new Vector[lines.Length];
        var angle = 360f / lines.Length;
        for (var i = 0; i < lines.Length; i++)
        {
            var x = radius * MathF.Cos(degToRadian(angle * i));
            var y = radius * MathF.Sin(degToRadian(angle * i));
            offsets[i] = new Vector(x, y, 0);
        }
        return offsets;
    }

    public override void Draw()
    {
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var start = position + offsets[i];
            var end = position + offsets[(i + 1) % offsets.Length];
            if (line == null)
            {
                line = new BeamLine(plugin, start, end);
                line.SetColor(color);
                line.Draw();
                lines[i] = line;
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
        this.radius = radius;
        offsets = generateOffsets();
    }
}