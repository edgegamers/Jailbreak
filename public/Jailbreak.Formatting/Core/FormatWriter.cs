namespace Jailbreak.Formatting.Core;

public class FormatWriter {
  private readonly List<FormatObject[]> lines = new();

  public IEnumerable<FormatObject[]> Lines => lines;

  public IEnumerable<string> Chat
    => Lines.Select(
      array => string.Join(' ', array.Select(obj => obj.ToChat())));

  public IEnumerable<string> Panorama
    => Lines.Select(array
      => string.Join(' ', array.Select(obj => obj.ToPanorama())));

  public IEnumerable<string> Plain
    => Lines.Select(array
      => string.Join(' ', array.Select(obj => obj.ToPlain())));


  public FormatWriter Line(params FormatObject[] args) {
    lines.Add(args);

    return this;
  }
}