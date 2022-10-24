using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum Sound
{
    Footstep,
    EnemyDetect,
    BGM,
    Chase,
    Checkpoint
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject player;
    public List<GameObject> uiImages;
    public CameraDisplayScriptableObject displayConfig;
    public CanvasGroup win;
    public CanvasGroup lose;

    public AudioClip[] footSteps;
    public AudioClip enemyDetect;
    public AudioClip checkpoint;
    public AudioClip bgm;
    public AudioClip chase;
    public List<GameObject> checkPoints;

    private AudioSource _audioSource;
    private AudioSource _bgmSource;
    private List<PeopleAI> _peoples;
    private List<RenderTexture> _renderTextures;
    private List<int> _displayList;
    private int _lastFootstep;
    private bool _win;

    public void PlaySound(Sound s)
    {
        switch (s)
        {
            case Sound.Footstep:
                int i; 
                do
                {
                    i = Random.Range(0, footSteps.Length);
                } while (i == _lastFootstep);
                
                _audioSource.PlayOneShot(footSteps[i]);
                break;
            case Sound.EnemyDetect:
                _audioSource.PlayOneShot(enemyDetect);
                break;
            case Sound.Checkpoint:
                _audioSource.PlayOneShot(checkpoint);
                break;
            case Sound.BGM:
                _bgmSource.clip = bgm;
                _bgmSource.Play();
                break;
            case Sound.Chase:
                _bgmSource.clip = chase;
                _bgmSource.Play();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _bgmSource = GetComponentInChildren<AudioSource>();
        _audioSource = GetComponent<AudioSource>();
        _peoples = new List<PeopleAI>(FindObjectsOfType<PeopleAI>());
        _renderTextures = new List<RenderTexture>();
        _displayList = new List<int>();

        for (var i = 1; i < checkPoints.Count; i++)
        {
            checkPoints[i].SetActive(false);
        }
        
        for (var i = 0; i < _peoples.Count; i++)
        {
            _renderTextures.Add(new RenderTexture(1080, 1080, 16));
            _peoples[i].GetComponentInChildren<Camera>().targetTexture = _renderTextures[i];
            if (_peoples[i].isEnemy)
            {
                uiImages[i].transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void Start()
    {
        PlaySound(Sound.BGM);
    }

    public void OnCheckPointReached(GameObject c)
    {
        PlaySound(Sound.Checkpoint);
        c.SetActive(false);
        var index = checkPoints.IndexOf(c);

        if (index != checkPoints.Count - 1)
            checkPoints[index + 1].SetActive(true);
        else 
            Win();
    }

    private void Win()
    {
        _win = true;
        win.DOFade(1, 1);
    }

    public void Lose()
    {
        if (_win)
            return;
        lose.DOFade(1, 1);
        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(1);
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
        if (_displayList.Count == 0)
            return;
        
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