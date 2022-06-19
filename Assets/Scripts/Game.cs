using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;

public class Game : MonoBehaviour
{

    public static Game manager;
        /*
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
    }*/

    public bool aPlayerIsMoving = false;
    public int levelNumber = 1;
    public Grid unitGrid;  // 16x16 grid that units move on.
    public Tilemap[] tileMaps;
    public GameObject mouseover;
    public TextMeshProUGUI mouseoverText;
    public AudioClip[] audioClips;
    public List<Player> players = new List<Player>();
    public List<Enemy> enemies = new List<Enemy>();
    public GameObject overlay;
    public LevelInfo[] levels;

    private AudioSource soundFx;
    private Dictionary<string, AudioClip> audioClipsDict = new Dictionary<string, AudioClip>();
    private LevelInfo currentLevel;
    public GameObject tiledMap;


    void Awake()
    {
        if (manager == null) manager = this;
        else if (manager != this) Destroy(gameObject);

        // Keeps this from getting destroyed during code swap.
        DontDestroyOnLoad(gameObject);

        currentLevel = levels[0];

        foreach (AudioClip audioClip in audioClips) audioClipsDict.Add(audioClip.name, audioClip);

        // Keep track of the sound-manager game object.
        soundFx = GetComponent<AudioSource>();

        //
        unitGrid = GameObject.Find("UnitGrid").GetComponent<Grid>();
        overlay = GameObject.Find("Overlay");
        overlay.SetActive(false);

        mouseover = GameObject.Find("UnitMouseover");

        // Set text based on unit information.
        mouseoverText = mouseover.GetComponentInChildren<TextMeshProUGUI>();

        // Keep track of all ally and enemy units.
        players.Clear();
        players.AddRange(GameObject.FindObjectsOfType<Player>(true));
        enemies.Clear();
        enemies.AddRange(GameObject.FindObjectsOfType<Enemy>(true));

        Init();
    }

    // Start is called before the first frame update
    void Init()
    {

        tileMaps = FindObjectsOfType<Tilemap>();
        tiledMap = GameObject.Find(currentLevel.name);
        Tilemap tileMap = tileMaps[0];

        // Prevent viewing of the 'zones' layer (but keep collision)
        tileMap.GetComponent<TilemapRenderer>().enabled = false;

        // Get all start tiles.
        List<Vector3Int> startZone = new List<Vector3Int>();
        SuperTiled2Unity.CustomProperty customProperty = new SuperTiled2Unity.CustomProperty();
        customProperty.m_Name = "startZone";
        customProperty.m_Type = "bool";
        customProperty.m_Value = "true";
        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {

            SuperTile superTile = tileMap.GetTile<SuperTile>(position);
            if (superTile == null) continue;

            foreach (CustomProperty property in superTile.m_CustomProperties)
            {

                if (property.m_Name.Equals(customProperty.m_Name) && property.m_Value.Equals(customProperty.m_Value)) startZone.Add(position);

            }

        }

        // For each ally, place on start tiles.
        int i = 0;
        foreach (Player ally in players)
        {


            Vector3Int startPosition = startZone[i];
            ally.transform.position = tileMap.CellToWorld(startPosition);
            i = (i + 1) % startZone.Count;

        }


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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /*
     * TODO: remove if unused. 
     */
    static private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ++Game.manager.levelNumber;
        Game.manager.Init();
    }

    public IEnumerator GotoNextLevel()
    {

        overlay.SetActive(true);

        ++levelNumber;
        foreach (LevelInfo levelInfo in levels)
        {

            if (levelInfo.name == currentLevel.nextLevel)
            {

                currentLevel = levelInfo;
                break;

            }

        }



        // TODO: place player and friendly units in the start zone.
        // SceneManager.LoadScene(nextLevel.name, LoadSceneMode.Single);
        Destroy(tiledMap);
        tiledMap = Instantiate(Resources.Load("TiledMaps/" + currentLevel.name)) as GameObject;
        Init();

        // Fixes layering issue.
        overlay.SetActive(false); 
        overlay.SetActive(true);

        // Leave transition over the screen.
        yield return new WaitForSeconds(2f);

        overlay.SetActive(false);

    }

}

[System.Serializable]
public class LevelInfo
{

    public string name;
    public string nextLevel;

}
