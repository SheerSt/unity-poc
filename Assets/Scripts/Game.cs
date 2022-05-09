using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class Game : MonoBehaviour
{

    public static Game manager;
    
    public Grid unitGrid;  // 16x16 grid that units move on.
    public Tilemap[] tileMaps;
    public GameObject mouseover;
    public TextMeshProUGUI mouseoverText;

    void Awake()
    {

        if (manager == null) manager = this;

    }

    // Start is called before the first frame update
        void Start()
    {
        manager = this;
        tileMaps = FindObjectsOfType<Tilemap>();
        unitGrid = GameObject.Find("UnitGrid").GetComponent<Grid>();
        mouseover = GameObject.Find("UnitMouseover");
        // Set text based on unit information.
        mouseoverText = mouseover.GetComponentInChildren<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
