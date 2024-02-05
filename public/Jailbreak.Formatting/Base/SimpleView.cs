using System.Collections;
using System.Text;

using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Base;

public class SimpleView : IView, IEnumerable<IList<FormatObject>>
{
	private List<List<FormatObject>> _lines = new();

	public SimpleView()
	{

	}

	/// <summary>
	/// Add an item to the end of the last row in this SimpleView
	/// Eg, { abc, 123, weee } is all one row
	/// </summary>
	/// <param name="item"></param>
	public void Add(FormatObject item)
	{
		if (_lines.Count == 0)
			_lines.Add(new List<FormatObject>());

		_lines[_lines.Count - 1].Add(item);
	}

	/// <summary>
	/// Add a whole new row to this simpleview
	/// Eg, { { 123 }, { abc }, { weeeeee } } are each their own lines.
	/// </summary>
	/// <param name="line"></param>
	public void Add(params FormatObject[] line)
	{
		_lines.Add(new List<FormatObject>(line));
	}

	public void Render(FormatWriter writer)
	{
		foreach (List<FormatObject> formatObjects in _lines)
			writer.Line(formatObjects.ToArray());
	}

	public IEnumerator<IList<FormatObject>> GetEnumerator()
		=> _lines.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
