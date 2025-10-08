using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Breakpoint.Revamped;  // for Essence enum
using Essence = DamageType;
public class AffinityTrackManager : MonoBehaviour
{
    [Header("Track Containers")]
    public Transform[] team1Tracks; // 4 tracks in order: Force, Arcane, Elemental, Corrupt
    public Transform[] team2Tracks;

    [Header("Sprites")]
    public Sprite forceSprite;
    public Sprite arcaneSprite;
    public Sprite elementalSprite;
    public Sprite corruptSprite;

    [Header("Mark Prefab")]
    public Image markPrefab;

    private readonly Dictionary<(int team, Essence essence), List<Image>> marks = new();

    private void Awake()
    {
        EventManager.Subscribe("OnTrackMarkAdded", HandleMarkAdded);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe("OnTrackMarkAdded", HandleMarkAdded);
    }

    private void HandleMarkAdded(object data)
    {
        if (data is not GameEventData evt) return;

        int teamId = evt.Get<int>("TeamId");
        Essence essence = evt.Get<Essence>("Essence");
        int currentMarks = evt.Get<int>("CurrentMarks");
        int threshold = evt.Get<int>("Threshold");

        Transform track = GetTrack(teamId, essence);
        if (track == null) return;

        RefreshTrack(track, teamId, essence, currentMarks, threshold);
    }

    private Transform GetTrack(int teamId, Essence essence)
    {
        Transform[] arr = teamId == 1 ? team1Tracks : team2Tracks;
        return essence switch
        {
            Essence.Force => arr[0],
            Essence.Arcane => arr[1],
            Essence.Elemental => arr[2],
            Essence.Corrupt => arr[3],
            _ => null
        };
    }

    private Sprite GetSprite(Essence essence)
    {
        return essence switch
        {
            Essence.Force => forceSprite,
            Essence.Arcane => arcaneSprite,
            Essence.Elemental => elementalSprite,
            Essence.Corrupt => corruptSprite,
            _ => null
        };
    }

    private void RefreshTrack(Transform track, int team, Essence essence, int currentMarks, int threshold)
    {
        var key = (team, essence);
        if (!marks.ContainsKey(key))
            marks[key] = new List<Image>();

        var list = marks[key];

        // Add missing marks
        while (list.Count < currentMarks)
        {
            Image newMark = Instantiate(markPrefab, track);
            newMark.sprite = GetSprite(essence);
            newMark.gameObject.SetActive(true);
            list.Add(newMark);
        }

        // Adjust sizing for new threshold dynamically
        var layout = track.GetComponent<VerticalLayoutGroup>();
        float trackHeight = (track as RectTransform).rect.height;

        // total space = height - top/bottom padding - (spacing * (threshold-1))
        float available = trackHeight - layout.padding.top - layout.padding.bottom - (layout.spacing * (threshold - 1));
        float markHeight = available / threshold;

        foreach (var img in list)
        {
            var r = img.rectTransform;
            r.sizeDelta = new Vector2(13f, markHeight); // fixed width, dynamic height
        }
    }
}
