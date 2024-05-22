using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jailbreak.Formatting.Views;

public interface IJihadC4Notifications
{
    public IView PlayerDetonateC4(CCSPlayerController player);
    public IView JIHAD_C4_DROPPED { get; }
    public IView JIHAD_C4_PICKUP { get; }
    public IView JIHAD_C4_RECEIVED { get; }

}