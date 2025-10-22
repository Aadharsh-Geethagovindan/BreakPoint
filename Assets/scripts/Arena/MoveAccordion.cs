using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveAccordion : MonoBehaviour
{
    [System.Serializable]
    public class MoveEntry
    {
        public RectTransform root;
        public RectTransform body;
        public Button useButton;
        public Button toggleButton;
        public TMP_Text moveTitle;
        public TMP_Text moveDescription;

        [HideInInspector] public bool isOpen;
    }

    public List<MoveEntry> moves = new List<MoveEntry>();
    public float closedHeight = 60f;
    public float openHeight = 180f;

    private MoveEntry currentOpen;

    private void Start()
    {
        foreach (var move in moves)
        {
            move.isOpen = false;
            move.body.gameObject.SetActive(false);

            move.toggleButton.onClick.AddListener(() => ToggleMove(move));
        }

        // Optionally open the first one by default
        if (moves.Count > 0)
            ToggleMove(moves[0]);
    }

    private void ToggleMove(MoveEntry target)
    {
        foreach (var move in moves)
        {
            bool shouldOpen = (move == target);

            if (move.isOpen == shouldOpen) continue;

            move.isOpen = shouldOpen;
            move.body.gameObject.SetActive(shouldOpen);

            var layout = move.root.GetComponent<LayoutElement>();
            if (layout == null)
                layout = move.root.gameObject.AddComponent<LayoutElement>();

            layout.preferredHeight = shouldOpen ? openHeight : closedHeight;

            // rotate chevron if desired
            var icon = move.toggleButton.transform as RectTransform;
            if (icon) icon.localRotation = Quaternion.Euler(0, 0, shouldOpen ? 180 : 0);
        }

        // Force refresh layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}
