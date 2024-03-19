using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Behaviors;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Generic;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Serilog;
using CounterStrikeSharp.API.Modules.Memory;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{

    // use services we want here
    private readonly IJihadC4Notifications _jihadC4Notifications;
    private readonly ICoroutines _coroutines;
    private BasePlugin? _basePlugin;

    // any entry in this list implies the player has an active jihad c4 in their pawn inventory
    private Dictionary<CCSPlayerController, JihadC4Metadata> _activeJihadC4Players;
    // This is so we can play a bomb explosion sound when a player uses the jihad c4, CS# may implement this natively soon :)
    //private static readonly MemoryFunctionVoid<nint, string> CBaseEntity_EmitSound = new("\\x48\\x8B\\xC4\\x48\\x89\\x58\\x10\\x48\\x89\\x70\\x18\\x55\\x57\\x41\\x56\\x48\\x8D\\xA8\\x08\\xFF\\xFF\\xFF");

    private class JihadC4Metadata
    {
        public JihadC4Metadata(nint bombEntityPointer, float Delay) { this.bombEntityPointer = bombEntityPointer; this.Delay = Delay; }
        public nint bombEntityPointer { get; set; }
        public float Delay { get; set; }
    }

    public JihadC4Behavior(IJihadC4Notifications jihadC4Notifications, ICoroutines coroutines)
    {
        _jihadC4Notifications = jihadC4Notifications;
        _coroutines = coroutines;

        _activeJihadC4Players = new Dictionary<CCSPlayerController, JihadC4Metadata>();
    }


    public void Start(BasePlugin parent)
    {
        _basePlugin = parent;
        if (_basePlugin == null) { throw new NullReferenceException("_basePlugin reference was null...?"); }
    }

    // debug cmd remove later
    [ConsoleCommand("css_d", "")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Debug(CCSPlayerController? player, CommandInfo info)
    {

        if (player == null) { return; }
        if (!player.IsValid) { return; }
        GiveJihadC4ToTerrorist(player);

    }

    // todo make sure active weapon out is a c4
    [ConsoleCommand("css_delay", "Sets a delay on the Jihad C4. Must be holding it out. Usage: !delay <seconds>")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Delay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) { return; }
        if (!_activeJihadC4Players.ContainsKey(player)) { return; }
        if (info.ArgCount <= 1) { _jihadC4Notifications.JIHAD_DELAY_CMD_USAGE.ToPlayerChat(player); return; } // I guess by default no args = 0 seconds

        try
        {
            float delay = Convert.ToSingle(info.GetArg(1));
            TrySetDelayJihadC4(player, delay);
            _jihadC4Notifications.JihadCurrentDelay(delay).ToPlayerChat(player);
        } catch (Exception)
        {
            _jihadC4Notifications.JIHAD_DELAY_CMD_USAGE.ToPlayerChat(player);
            return;
        }


    }

    /// <summary>
    /// The Terrorist with the Jihad C4 is classed as "Using" the C4 if they drop it into the world with the G key (or whatever
    /// it's bound to).
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnTerroristUseJihadC4(EventBombDropped @event, GameEventInfo info)
    {
        Console.WriteLine("we got here or... ? (first print)");
        
        if (@event.Userid == null || !@event.Userid.IsValid) { return HookResult.Continue; }
        long entityIndex = @event.Entindex; // needed if we do any nextframe shit
        CC4 bombEntity = Utilities.GetEntityFromIndex<CC4>((int)entityIndex);
        if (bombEntity == null) { return HookResult.Continue; }
        if (bombEntity.Entity == null || bombEntity.Entity.Name == null || !bombEntity.Entity.Name.StartsWith("jihad_c4") ) { return HookResult.Continue; }
        if (!_activeJihadC4Players.ContainsKey(@event.Userid)) { return HookResult.Continue; }

        // else the JihadC4 is active for the specified player and we can invoke TryBlowUpJihadC4()
        
        
        //bombEntity.CanBePickedUp = false;


        /**Server.NextFrame(() =>
        {
            Console.WriteLine("ok we got here");
            //Utilities.GetEntityFromIndex<CC4>((int)entityIndex).CanBePickedUp = false;
            bombEntity.CanBePickedUp = false;
            //TryBlowUpJihadC4(bombEntity, @event.Userid);
        });**/
        
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnTerroristEquipJihadC4(EventItemEquip @event, GameEventInfo info)
    {
        if (@event.Userid == null || !@event.Userid.IsValid) { return HookResult.Continue; }
        if (@event.Weptype != ((uint)CSWeaponType.WEAPONTYPE_C4)) { return HookResult.Continue; }
        if (!_activeJihadC4Players.Keys.Contains(@event.Userid)) { return HookResult.Continue; }
        
        // else the currently held out item is a weapon_c4 and the player holding it has the capabilities to blow it up
        // thanks Roflmuffin 
        CCSPlayerController player = @event.Userid;
        Server.NextFrame(() =>
        {
            if (player == null) { return; }
            if (!player.IsValid) { return; }
            _jihadC4Notifications.JihadCurrentDelay(_activeJihadC4Players[player].Delay).ToPlayerChat(player);
        });

        return HookResult.Continue;
    }

    public void GiveJihadC4ToRandomTerrorist()
    {
        throw new NotImplementedException();
    }

    public void GiveJihadC4ToTerrorist(CCSPlayerController terrorist)
    {
        if (_activeJihadC4Players.ContainsKey(terrorist)) { return; }
        // this is a pointer... nint maps to System.IntPtr
        nint bombEntityPointer = terrorist.GiveNamedItem(CsItem.C4);

        CC4 entityC4 = new CC4(bombEntityPointer);
        
        entityC4.Entity!.Name = "jihad_c4";

        // todo find function signature of item_pickup 

        // by default the bomb explodes immediately
        JihadC4Metadata metadata = new JihadC4Metadata(bombEntityPointer, 0.0f);
        _activeJihadC4Players.Add(terrorist, metadata);

        _jihadC4Notifications.JIHAD_GIVEN_BOMB.ToPlayerChat(terrorist);

    }


    // Should blow up the player only at the given location 
    public void TryBlowUpJihadC4(CC4 bombEntity, CCSPlayerController terrorist)
    {
        if (!_activeJihadC4Players.ContainsKey(terrorist)) {  return; }

        float delay = _activeJihadC4Players[terrorist].Delay;
        Console.WriteLine($"delay is: {delay}");

        // todo play WA LA LA LA sound now

        // todo fix crash, why are we crashing???...
        /*_coroutines.Round(() =>
        {

            CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
            //particleSystemEntity.EffectName = "particles/jihad_c4_explosion.vpcf";
            particleSystemEntity.EffectName = "particles/lastguard_beacon.vpcf";
            particleSystemEntity.Teleport(bombEntity.AbsOrigin! + new Vector(0, 10, 0), new QAngle(), new Vector());
            
            particleSystemEntity.StartActive = true;
            particleSystemEntity.DispatchSpawn();

            // once we've spawned the particles in we want play a sound  
            // TODO UNCOMMENT
            Console.WriteLine("alright we got here no crash");

            //EmitSoundFunc(bombEntity, "c4.explode"); // play the BOOM sound in the game world relative to the c4!

            Console.WriteLine("we emitted the sound");
            // next we want to spawn an env_physexplosion to push players, and env_explosion to damage them!
            /**CPhysExplosion physExplosionEntity = Utilities.CreateEntityByName<CPhysExplosion>("env_physexplosion")!;
            physExplosionEntity.Flags = (1 << 1); // https://developer.valvesoftware.com/wiki/Env_physexplosion
            physExplosionEntity.Magnitude = 45.0f;
            physExplosionEntity.Damage = 200f;
            physExplosionEntity.Radius = 400f;
            physExplosionEntity.PushScale = 1f;
    
            physExplosionEntity.Teleport(bombEntity.AbsOrigin! + new Vector(0, 10, 0), new QAngle(0, 0, 0), new Vector());

            physExplosionEntity.ExplodeOnSpawn = true;
            physExplosionEntity.DispatchSpawn();

            bombEntity.Remove();

        }, delay);*/

        CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
        particleSystemEntity.EffectName = "particles/lastguard_beacon.vpcf";
        particleSystemEntity.Teleport(bombEntity.AbsOrigin! + new Vector(0, 10, 0), new QAngle(), new Vector());

        particleSystemEntity.StartActive = true;
        particleSystemEntity.DispatchSpawn();
        Console.WriteLine("alright we got here no crash");

        EmitSoundFunc(bombEntity, "c4.explode"); // play the BOOM sound in the game world relative to the c4!

        Console.WriteLine("we emitted the sound");

        bombEntity.Remove();

        Console.WriteLine("why did we instantaneously FIRE... (should be last print line)");
        _activeJihadC4Players.Remove(terrorist);

    }

    public void TryDefuseJihadC4(CCSPlayerController terrorist)
    {
        throw new NotImplementedException();
    }

    public void TrySetDelayJihadC4(CCSPlayerController terrorist, float delay)
    {
        if (!_activeJihadC4Players.ContainsKey(terrorist)) { return; }
        JihadC4Metadata metadata = _activeJihadC4Players[terrorist]; // make it clear
        metadata.Delay = delay;
    }

    private void EmitSoundFunc(CBaseEntity entity, string soundName)
    {
        //CBaseEntity_EmitSound.Invoke(entity.Handle, soundName);
    }


}
