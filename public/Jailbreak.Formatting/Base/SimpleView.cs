using System.Collections;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Base;

public class SimpleView : IView, IEnumerable<IList<FormatObject>>
{
    public static Newline NEWLINE = new();

    private readonly List<List<FormatObject>> _lines = new();

    public IEnumerator<IList<FormatObject>> GetEnumerator()
    {
        return _lines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Render(FormatWriter writer)
    {
        foreach (var formatObjects in _lines)
            writer.Line(formatObjects.ToArray());
    }

    /// <summary>
    ///     Add an item to the end of the last row in this SimpleView
    ///     Eg, { abc, 123, weee } is all one row
    /// </summary>
    /// <param name="item"></param>
    public void Add(FormatObject item)
    {
        if (_lines.Count == 0)
            _lines.Add(new List<FormatObject>());

        _lines[_lines.Count - 1].Add(item);
    }

    /// <summary>
    ///     Add multiple items at a time to this SimpleView
    /// </summary>
    /// <param name="line"></param>
    public void Add(params FormatObject[] line)
    {
        if (_lines.Count == 0)
            _lines.Add(new List<FormatObject>());

        _lines[_lines.Count - 1].AddRange(line);
    }

    /// <summary>
    ///     Add a new line to this SimpleView
    /// </summary>
    /// <param name="newline"></param>
    public void Add(Newline newline)
    {
        _lines.Add(new List<FormatObject>());
    }

    public struct Newline
    {
    }
}