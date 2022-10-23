using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject player;
    public List<GameObject> uiImages;
    public CameraDisplayScriptableObject displayConfig;

    private AudioSource _audioSource;
    private List<PeopleAI> _peoples;
    private List<RenderTexture> _renderTextures;
    private List<int> _displayList;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _audioSource = GetComponent<AudioSource>();
        _peoples = new List<PeopleAI>(FindObjectsOfType<PeopleAI>());
        _renderTextures = new List<RenderTexture>();
        _displayList = new List<int>();
        
        for (var i = 0; i < _peoples.Count; i++)
        {
            _renderTextures.Add(new RenderTexture(1080, 1080, 16));
            _peoples[i].GetComponentInChildren<Camera>().targetTexture = _renderTextures[i];
        }
    }

    private void OnDisplayListChanged()
    {
        for (var i = 0; i < _peoples.Count; i++)
        {
            var img = uiImages[i].GetComponentInChildren<RawImage>();
            if (_displayList.Contains(i))
            {
                uiImages[i].SetActive(true);
                img.texture = _renderTextures[i];
            }
            else
            {
                uiImages[i].SetActive(false);
                uiImages[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                img.texture = null;
            }
        }

        if (_displayList.Count == 0)
            return;
        var number = _displayList.Count;
        var pendingPos = new List<Vector2>(displayConfig.GetUIPos(number));
        foreach (var i in _displayList)
        {
            var rect = uiImages[i].GetComponent<RectTransform>();
            rect.DOScale(displayConfig.GetUISize(number), .8f);

            var minDist = float.MaxValue;
            var minPos = pendingPos[0];
            foreach (var p in pendingPos)
            {
                var d = Vector2.Distance(p, rect.anchoredPosition);
                if (d < minDist)
                {
                    minDist = d;
                    minPos = p;
                }
            }
            rect.DOAnchorPos(minPos, .8f);
            pendingPos.Remove(minPos);
        }
    }

    public void AddToDisplayList(PeopleAI p)
    {
        if (_displayList.Count == 6)
            return;
        
        var index = _peoples.IndexOf(p);
        if (_displayList.Contains(index))
        {
            Debug.LogError("Display list already contains this camera!");
        }
        else
        {
            _displayList.Add(index);
        }

        OnDisplayListChanged();
    }
    
    public void RemoveFromDisplayList(PeopleAI p)
    {
        var index = _peoples.IndexOf(p);
        if (!_displayList.Contains(index))
        {
            Debug.LogError("Display list does not contain this camera!");
        }
        else
        {
            _displayList.Remove(index);
        }

        OnDisplayListChanged();
    }
}