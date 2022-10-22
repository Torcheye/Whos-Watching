using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject player;
    public List<GameObject> renderImages;

    private AudioSource _audioSource;
    private List<Camera> _cameras;
    private List<RenderTexture> _renderTextures;

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        _audioSource = GetComponent<AudioSource>();
        _cameras = new List<Camera>(FindObjectsOfType<Camera>());
        _renderTextures = new List<RenderTexture>();
        
        Assert.AreEqual(renderImages.Count, _cameras.Count);
        for (var i = 0; i < renderImages.Count; i++)
        {
            _renderTextures.Add(new RenderTexture(1600, 900, 16));
            _cameras[i].targetTexture = _renderTextures[i];
            renderImages[i].GetComponentInChildren<RawImage>().texture = _renderTextures[i];
        }
    }
}