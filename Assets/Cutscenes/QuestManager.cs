using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text questsText;

    [Header("Cutscene hookup")]
    public PlayableDirector openingDirector;
    public Behaviour movementScript;

    [Header("Behavior")]
    public float strikeDuration = 1.0f;
    public float hideWhenEmptyDelay = 0f;

    [System.Serializable]
    public struct InitialQuest
    {
        public string id;
        [TextArea] public string text;
    }

    [Header("Initial quests to add after cutscene")]
    public List<InitialQuest> initialQuests = new List<InitialQuest>();

    // ----------------- AUDIO -----------------
    [Header("Audio")]
    [Tooltip("AudioSource used for quest sounds (non-spatial, 2D).")]
    public AudioSource sfxSource;

    [Tooltip("Played each time a quest is added/shown.")]
    public AudioClip questAppearClip;

    [Tooltip("Played when a quest is completed (before strike-through).")]
    public AudioClip questCompleteClip;

    [Range(0f, 1f)] public float appearVolume = 0.8f;
    [Range(0f, 1f)] public float completeVolume = 0.9f;

    // -----------------------------------------

    private readonly Dictionary<string, QuestEntry> _entries = new Dictionary<string, QuestEntry>();
    private bool _subscribed;

    private class QuestEntry
    {
        public string id;
        public string text;
        public bool completed;
        public Coroutine removal;
    }

    void Awake()
    {
        if (questsText) questsText.enableWordWrapping = true;

        if (openingDirector && !_subscribed)
        {
            openingDirector.stopped += OnOpeningCutsceneStopped;
            _subscribed = true;
        }

        // Make sure quest SFX is 2D (no position falloff) if present
        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }

        RefreshUI();
    }

    void OnDestroy()
    {
        if (openingDirector && _subscribed)
        {
            openingDirector.stopped -= OnOpeningCutsceneStopped;
            _subscribed = false;
        }
    }

    private void OnOpeningCutsceneStopped(PlayableDirector d)
    {
        if (movementScript) movementScript.enabled = true; // belt-and-suspenders
        StartQuests();
        openingDirector.stopped -= OnOpeningCutsceneStopped;
        _subscribed = false;
    }

    public void StartQuests()
    {
        foreach (var q in initialQuests)
            AddQuest(q.id, q.text);
        RefreshUI();
    }

    // ---------- Public API ----------

    public void AddQuest(string id, string text)
    {
        if (string.IsNullOrEmpty(id)) return;

        bool isNew = !_entries.ContainsKey(id);
        if (_entries.TryGetValue(id, out var existing))
        {
            existing.text = text;
            existing.completed = false;
            if (existing.removal != null) { StopCoroutine(existing.removal); existing.removal = null; }
        }
        else
        {
            _entries[id] = new QuestEntry { id = id, text = text, completed = false };
        }

        // AUDIO: play “quest appeared” only when it’s new or being re-shown
        if (sfxSource && questAppearClip && isNew)
            sfxSource.PlayOneShot(questAppearClip, appearVolume);

        RefreshUI();
    }

    public void CompleteQuest(string id)
    {
        if (!_entries.TryGetValue(id, out var e)) return;
        if (e.completed) return;

        // AUDIO: play completion sting immediately
        if (sfxSource && questCompleteClip)
            sfxSource.PlayOneShot(questCompleteClip, completeVolume);

        e.completed = true;
        if (e.removal != null) StopCoroutine(e.removal);
        e.removal = StartCoroutine(StrikeThenRemove(e));
        RefreshUI();
    }

    public void RemoveQuest(string id)
    {
        if (_entries.TryGetValue(id, out var e))
        {
            if (e.removal != null) StopCoroutine(e.removal);
            _entries.Remove(id);
            RefreshUI();
        }
    }

    public void ClearAll()
    {
        foreach (var kv in _entries)
            if (kv.Value.removal != null) StopCoroutine(kv.Value.removal);
        _entries.Clear();
        RefreshUI();
    }

    // ---------- internals ----------

    private IEnumerator StrikeThenRemove(QuestEntry e)
    {
        RefreshUI(); // shows <s>struck</s>
        yield return new WaitForSeconds(strikeDuration);
        if (_entries.ContainsKey(e.id))
        {
            _entries.Remove(e.id);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (!questsText) return;

        if (_entries.Count == 0)
        {
            questsText.text = "";
            if (hideWhenEmptyDelay > 0f) StartCoroutine(HideWhenEmptyAfterDelay());
            return;
        }

        var sb = new StringBuilder();
        foreach (var e in _entries.Values)
        {
            if (!e.completed) sb.Append("• ").Append(e.text).Append('\n');
            else sb.Append("• <s>").Append(e.text).Append("</s>\n");
        }
        questsText.text = sb.ToString().TrimEnd();
    }

    private IEnumerator HideWhenEmptyAfterDelay()
    {
        yield return new WaitForSeconds(hideWhenEmptyDelay);
        if (_entries.Count == 0 && questsText) questsText.text = "";
    }

    // Optional singleton finder
    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get { if (_instance == null) _instance = FindObjectOfType<QuestManager>(true); return _instance; }
    }
    void OnEnable() { if (_instance == null) _instance = this; }
}
