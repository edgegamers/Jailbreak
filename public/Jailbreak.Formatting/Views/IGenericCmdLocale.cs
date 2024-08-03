using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IGenericCmdLocale {
  public IView PlayerNotFound(string query);
  public IView PlayerFoundMultiple(string query);
  public IView CommandOnCooldown(DateTime cooldownEndsAt);
  public IView InvalidParameter(string parameter, string expected);
  public IView NoPermissionMessage(string permission);
  public IView Error(string message);
}