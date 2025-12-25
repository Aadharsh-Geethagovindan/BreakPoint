using System.Collections.Generic;
using Mirror;
using UnityEngine;

public static class NetworkEventBridge
{
    // Only replicate the events that drive UI/audio/logs.
    // Add more as you validate payload shapes.
    private static readonly HashSet<string> Whitelist = new()
    {
        "OnDamageDealt",
        "OnMiss",
        "OnHealed",
        "OnHealApplied",
        "OnShielded",
        "OnShieldApplied",
        "OnBuffApplied",
        "OnImmunityTriggered",

        //"OnAbilityUsed",
        "OnStatusApplied",
        "OnCharacterDied",
        "OnCharacterRevived",

        "OnBreakpointUpdated",
        "OnBreakpointTriggered",
    };

    private static bool _wired;

    public static void Wire()
    {
        if (_wired) return;
        _wired = true;

        EventManager.OnEventTriggered += OnLocalEventTriggered;
    }

    public static void Unwire()
    {
        if (!_wired) return;
        _wired = false;

        EventManager.OnEventTriggered -= OnLocalEventTriggered;
    }

    private static void OnLocalEventTriggered(string eventName, object param)
    {
        // Only online
        if (!MatchTypeService.IsOnline) return;

        // Only server broadcasts authoritative events
        if (!NetworkServer.active) return;

        if (!Whitelist.Contains(eventName)) return;

        if (!TryBuildMessage(eventName, param, out var msg))
            return;

        NetworkServer.SendToAll(msg);
    }

    private static bool TryBuildMessage(string eventName, object param, out ReplicatedEventNetMessage msg)
    {
        msg = default;
        msg.EventName = eventName;
        msg.SourceId = -1;
        msg.TargetId = -1;

        // Most of your systems pass GameEventData.
        var evt = param as GameEventData;

        // Pattern 1: popup/sfx that only needs a target
        // PopupManager’s ShowDamagePopup expects GameEventData with Target set.
        if (eventName == "OnDamageDealt" || eventName == "OnMiss" || eventName == "OnImmunityTriggered")
        {
            var target = evt?.Get<GameCharacter>("Target");
            if (target == null) return false;

            msg.PayloadType = ReplicatedEventPayloadType.TargetOnly;
            msg.TargetId = target.Id;
            return true;
        }

        // Pattern 2: heal/shield/buff popups (no param usage in your code, but keep consistent)
        if (eventName == "OnHealed" || eventName == "OnShielded" || eventName == "OnBuffApplied")
        {
            // If you have target in those events, include it; otherwise just fire with None.
            var target = evt?.Get<GameCharacter>("Target");
            msg.PayloadType = target != null ? ReplicatedEventPayloadType.TargetOnly : ReplicatedEventPayloadType.None;
            msg.TargetId = target != null ? target.Id : -1;
            return true;
        }

        // Pattern 3: logger-style events often have Source/Ability/Targets/etc.
        // For now, if you want the client logger to show “something”, you can replicate as Text.
        


        // Default: don’t replicate unknown shapes yet
        return false;
    }

    // Client-side replay (called by network manager handler)
    public static void ReplayOnClient(ReplicatedEventNetMessage msg)
    {
        if (!MatchTypeService.IsOnline) return;
        if (!NetworkClient.active) return;

        // Reconstruct the param into what listeners expect (usually GameEventData).
        object param = null;
        

        if (msg.PayloadType == ReplicatedEventPayloadType.TargetOnly)
        {
            var target = BattleManager.Instance != null ? BattleManager.Instance.GetCharacterById(msg.TargetId) : null;
            if (target != null)
            {
                var evt = new GameEventData().Set("Target", target);
                param = evt;
            }
        }
        else if (msg.PayloadType == ReplicatedEventPayloadType.Text)
        {
            // If you want announcer/logger to use this, you’ll need listeners that accept text
            // or wrap it in GameEventData with a "Text" key.
            var evt = new GameEventData().Set("Text", msg.Text);
            param = evt;
        }

        // Prevent rebroadcast loops while replaying
        EventManager.SuppressGlobalHooks = true;
        EventManager.Trigger(msg.EventName, param);
        EventManager.SuppressGlobalHooks = false;
    }
}
