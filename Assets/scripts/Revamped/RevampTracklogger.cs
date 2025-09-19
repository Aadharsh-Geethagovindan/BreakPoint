using UnityEngine;

public class RevampTrackLogger : MonoBehaviour
{
    private System.Action<object> hForce, hElem, hArc, hCor;
    private System.Action<object> hFE, hFA, hFC, hEA, hEC, hAC;
    private System.Action<object> hTriple;

    void OnEnable()
    {
        hForce = e => Handle("Force track triggered", e);
        hElem  = e => Handle("Elemental track triggered", e);
        hArc   = e => Handle("Arcane track triggered", e);
        hCor   = e => Handle("Corrupt track triggered", e);

        hFE = e => Handle("Fusion: Eruption (Force + Elemental)", e);
        hFA = e => Handle("Fusion: Disruption (Force + Arcane)", e);
        hFC = e => Handle("Fusion: Crush (Force + Corrupt)", e);
        hEA = e => Handle("Fusion: Purify (Elemental + Arcane)", e);
        hEC = e => Handle("Fusion: Blightstorm (Elemental + Corrupt)", e);
        hAC = e => Handle("Fusion: Mindbreak (Arcane + Corrupt)", e);

        hTriple = e => Handle("CATACLYSM triggered", e);

        EventManager.Subscribe("OnTrack_Force", hForce);
        EventManager.Subscribe("OnTrack_Elemental", hElem);
        EventManager.Subscribe("OnTrack_Arcane", hArc);
        EventManager.Subscribe("OnTrack_Corrupt", hCor);

        EventManager.Subscribe("OnDual_FE_Eruption", hFE);
        EventManager.Subscribe("OnDual_FA_Disruption", hFA);
        EventManager.Subscribe("OnDual_FC_Crush", hFC);
        EventManager.Subscribe("OnDual_EA_Purify", hEA);
        EventManager.Subscribe("OnDual_EC_Blightstorm", hEC);
        EventManager.Subscribe("OnDual_AC_Mindbreak", hAC);

        EventManager.Subscribe("OnTriple_Cataclysm", hTriple);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnTrack_Force", hForce);
        EventManager.Unsubscribe("OnTrack_Elemental", hElem);
        EventManager.Unsubscribe("OnTrack_Arcane", hArc);
        EventManager.Unsubscribe("OnTrack_Corrupt", hCor);

        EventManager.Unsubscribe("OnDual_FE_Eruption", hFE);
        EventManager.Unsubscribe("OnDual_FA_Disruption", hFA);
        EventManager.Unsubscribe("OnDual_FC_Crush", hFC);
        EventManager.Unsubscribe("OnDual_EA_Purify", hEA);
        EventManager.Unsubscribe("OnDual_EC_Blightstorm", hEC);
        EventManager.Unsubscribe("OnDual_AC_Mindbreak", hAC);

        EventManager.Unsubscribe("OnTriple_Cataclysm", hTriple);
    }

    private void Handle(string label, object payload)
    {
        int teamId = -1;
        if (payload is GameEventData d && d.Has("TeamId"))
            teamId = d.Get<int>("TeamId");

        string msg = teamId >= 0 ? $"[Revamped] {label} — Team {teamId}" : $"[Revamped] {label}";
        Logger.Instance.PostLog(msg, LogType.Shield);
    }
}
