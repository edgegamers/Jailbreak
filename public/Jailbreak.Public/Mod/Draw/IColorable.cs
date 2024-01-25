using System.Drawing;

namespace Jailbreak.Public.Mod.Draw;

public interface IColorable
{
    void SetColor(Color color);
    Color GetColor();
}