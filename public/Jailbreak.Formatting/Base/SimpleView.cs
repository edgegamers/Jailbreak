using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Base;

public class SimpleView : IView
{
    private readonly Action<FormatWriter> _handler;

    public SimpleView(Action<FormatWriter> handler)
    {
        _handler = handler;
    }

    public SimpleView(params string[] lines)
    {
        _handler = writer =>
        {
            foreach (var line in lines) writer.Line(line);
        };
    }

    public void Render(FormatWriter writer)
    {
        _handler(writer);
    }
}