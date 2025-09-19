using UnityEngine;
using System;
using Essence = DamageType;
namespace Breakpoint.Revamped
{
    
    public enum StatusTag { None, Stun, Dot, Buff, Debuff, Shield, Heal }

    [Flags]
    public enum OutcomeFlags
    {
        None   = 0,
        Stun   = 1 << 0,
        Dot    = 1 << 1,
        Buff   = 1 << 2,
        Debuff = 1 << 3,
        Shield = 1 << 4,
        Heal   = 1 << 5,
    }

    [CreateAssetMenu(fileName = "RevampTuningConfig", menuName = "Breakpoint/Revamped Tuning")]
    public class RevampTuningConfig : ScriptableObject
    {
        [Header("Affinity Tracks — thresholds & windows")]
        [Tooltip("Marks needed to trigger a SINGLE-track effect.")]
        public int singleTrackThreshold = 6;

        [Tooltip("How many turns define the 'fusion window' for dual/triple triggers.")]
        public int fusionWindowTurns = 2;

        [Tooltip("If true, check for dual/triple immediately after a single fires (in addition to end-of-round check).")]
        public bool checkFusionImmediately = true;

        [Header("Per-ability base marks (added on resolve)")]
        [Tooltip("Marks granted when a Basic ability resolves (before any bonuses).")]
        public int marksOnBasic = 1;

        [Tooltip("Marks granted when a Skill resolves.")]
        public int marksOnSkill = 2;

        [Tooltip("Marks granted when a Signature resolves.")]
        public int marksOnSignature = 3;

        [Header("Bonus marks from outcomes (per resolution)")]
        [Tooltip("Extra marks when the ability applied a Stun/Freeze/Root etc.")]
        public int bonusMarks_Stun = 2;

        [Tooltip("Extra marks when the ability applied a DoT (burn/poison/bleed/etc.).")]
        public int bonusMarks_Dot = 1;

        [Tooltip("Extra marks when the ability applied an ally Buff.")]
        public int bonusMarks_Buff = 1;

        [Tooltip("Extra marks when the ability applied an enemy Debuff.")]
        public int bonusMarks_Debuff = 1;

        [Tooltip("Extra marks when the ability applied a Shield.")]
        public int bonusMarks_Shield = 1;

        [Tooltip("Extra marks when the ability applied a Heal.")]
        public int bonusMarks_Heal = 1;

        [Header("Delays (seconds) to sequence VFX/logic, even before real animations exist")]
        [Tooltip("Delay before applying the SINGLE-track effect (so players can read it).")]
        public float delay_Single = 0.15f;

        [Tooltip("Delay before applying a DUAL fusion effect, if it triggers.")]
        public float delay_Dual = 0.25f;

        [Tooltip("Delay before applying the TRIPLE (Cataclysm) effect, if it triggers.")]
        public float delay_Triple = 0.35f;

        [Header("Effect Durations (turns) — for your status systems to use")]
        [Tooltip("Duration for Force Stagger (Speed down).")]
        public int dur_Force_Stagger = 1;

        [Tooltip("Duration for Elemental sustain spike (shield/heal buff window).")]
        public int dur_Elemental_Sustain = 2;

        [Tooltip("Duration for Arcane cooldown manipulation (enemy +1, allies -1).")]
        public int dur_Arcane_Tempo = 1;

        [Tooltip("Duration for Corrupt DoT amplification window.")]
        public int dur_Corrupt_DoTAmp = 2;

        [Header("Cataclysm knobs")]
        [Tooltip("Percent Max HP to remove on Cataclysm (e.g., 0.15 = 15%).")]
        [Range(0f, 1f)] public float cataclysm_MaxHPPercentLoss = 0.15f;

        [Header("Elemental sustain magnitudes")]
        [Tooltip("Percent of MaxHP to shield each ally on Elemental single (0.0–1.0).")]
        [Range(0f, 1f)] public float elementalShieldPercent = 0.30f; // NEW

        [Tooltip("Optional heal percent of MaxHP for Elemental single (0 = none).")]
        [Range(0f, 1f)] public float elementalHealPercent = 0.00f;
    }
}
