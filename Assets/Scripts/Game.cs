using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class Game : MonoBehaviour
{

    public static Game _manager;
    public static Game manager
    {
        get
        {
            if (_manager == null)
            {
                _manager = FindObjectOfType<Game>();
            }
            return _manager;
        }
        set { }
    }

    public bool aPlayerIsMoving = false;
    public Grid unitGrid;  // 16x16 grid that units move on.
    public Tilemap[] tileMaps;
    public GameObject mouseover;
    public TextMeshProUGUI mouseoverText;
    public AudioClip[] audioClips;
    public Player[] players;
    public Enemy[] enemies;

    private AudioSource soundFx;
    private Dictionary<string, AudioClip> audioClipsDict = new Dictionary<string, AudioClip>();

    void Awake()
    {

        //if (manager == null) manager = this;

        // Keeps this from getting destroyed during code swap.
        DontDestroyOnLoad(gameObject);

        // TODO: test
        unitGrid = GameObject.Find("UnitGrid").GetComponent<Grid>();

    }

    // Start is called before the first frame update
    void Start()
    {

        tileMaps = FindObjectsOfType<Tilemap>();
        mouseover = GameObject.Find("UnitMouseover");
        // Set text based on unit information.
        mouseoverText = mouseover.GetComponentInChildren<TextMeshProUGUI>();

        // Keep track of the sound-manager game object.
        soundFx = GetComponent<AudioSource>();
        foreach (AudioClip audioClip in audioClips) audioClipsDict.Add(audioClip.name, audioClip);


        // Keep track of all ally and enemy units.
        players = GameObject.FindObjectsOfType<Player>(true);
        enemies = GameObject.FindObjectsOfType<Enemy>(true);

    }

    // Update is called once per frame
    void Update()
    {

    }

    /**
     * Play a sound using the GameManager
     */
    public void PlaySound(string sound)
    {

        soundFx.enabled = false;
        soundFx.clip = audioClipsDict[sound];
        soundFx.enabled = true;

    }

}
