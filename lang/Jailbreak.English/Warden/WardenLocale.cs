using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenLocale : IWardenLocale,
  ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Guarda>") {
      //  Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public static readonly FormatObject COMMAND_STANDS =
    new HiddenFormatObject($"O comando anterior vale por 10 segundos.");

  public IView PickingShortly
    => new SimpleView {
      PREFIX,
      $"Escolhendo um guarda em breve, digite {ChatColors.BlueGrey}!warden{ChatColors.Grey} para entrar na fila."
    };

  public IView NoWardens
    => new SimpleView {
      PREFIX,
      $"Ninguém na fila. O próximo guarda a {ChatColors.BlueGrey}!warden{ChatColors.Grey} será o guarda."
    };

  public IView NowFreeday
    => new SimpleView {
      PREFIX,
      $"Agora é freeday! CTs devem ir atrás do {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
    };

  public IView WardenLeft
    => new SimpleView { PREFIX, "O guarda saiu do jogo.", COMMAND_STANDS };

  public IView WardenDied
    => new SimpleView {
      {
        PREFIX,
        $"O guarda {ChatColors.Red}morreu{ChatColors.Grey}. Agora é freeday!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"CTs devem ir atrás do {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
      }
    };

  public IView BecomeNextWarden
    => new SimpleView {
      PREFIX,
      $"Digite {ChatColors.BlueGrey}!warden{ChatColors.Grey} para se tornar o guarda."
    };

  public IView JoinRaffle
    => new SimpleView {
      PREFIX,
      $"Você {ChatColors.White}entrou {ChatColors.Grey}no sorteio para guarda."
    };

  public IView LeaveRaffle
    => new SimpleView {
      PREFIX, $"Você {ChatColors.Red}saiu {ChatColors.Grey}do sorteio para guarda."
    };

  public IView NotWarden
    => new SimpleView {
      PREFIX, $"{ChatColors.LightRed}Você não é o guarda."
    };

  public IView FireCommandFailed
    => new SimpleView {
      PREFIX, "O comando para demitir falhou por um motivo desconhecido..."
    };
  
  public IView TogglingNotEnabled
    => new SimpleView {
      PREFIX, "Ativar/Desativar Auto-Guarda não é suportado neste servidor."
    };

  public IView PassWarden(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "renunciou ao cargo de guarda.", COMMAND_STANDS
    };
  }

  public IView FireWarden(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "foi demitido do cargo de guarda.", COMMAND_STANDS
    };
  }

  public IView
    FireWarden(CCSPlayerController player, CCSPlayerController admin) {
    return new SimpleView {
      PREFIX,
      admin,
      "demitiu",
      player,
      "do cargo de guarda.",
      COMMAND_STANDS
    };
  }

  public IView NewWarden(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "é o novo guarda." };
  }
  
  public IView CurrentWarden(CCSPlayerController? player) {
    return player is not null ?
      new SimpleView { PREFIX, "O guarda é", player, "." } :
      new SimpleView { PREFIX, "Não há nenhum guarda." };
  }

  public IView CannotWardenDuringWarmup
    => new SimpleView { PREFIX, "Você não pode ser guarda durante o aquecimento." };

  public IView FireCommandSuccess(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "foi demitido e não é mais o guarda."
    };
  }

  public IView MarkerPlaced(string marker) {
    return new SimpleView {
      PREFIX, $"{marker}{ChatColors.Grey} marcador colocado."
    };
  }

  public IView MarkerRemoved(string marker) {
    return new SimpleView {
      PREFIX, $"{marker}{ChatColors.Grey} marcador removido."
    };
  }

  public IView AutoWardenToggled(bool enabled) {
    return new SimpleView {
      PREFIX,
      ChatColors.Grey + "Você",
      enabled ? ChatColors.Green + "ativou" : ChatColors.Red + "desativou",
      ChatColors.Grey + " o Auto-Guarda."
    };
  }
}