using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay.SpecialDays;

public class Freeday : ISpecialDay
{
    public string Name => "Freeday";
    public string Description => "All prisoners and guards are allowed to roam freely.";
    
    public Freeday(BasePlugin plugin)
    {
        
    }
    
    public void OnStart()
    {
        ForceEntInput("func_door","Open");
        ForceEntInput("func_door_rotating","Open");
        ForceEntInput("prop_door_rotating","Open");
    }
    
    private static void ForceEntInput(String name, String input)
    {
        // search for door entitys and open all of them!
        var target = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(name);

        foreach(var ent in target)
        {
            if(!ent.IsValid)
            {
                continue;
            }

            ent.AcceptInput(input);
        }
    }
    
    public void OnEnd()
    {
        //do nothing for now
    }
}