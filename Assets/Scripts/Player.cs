using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public GameObject gridBox;  // Some square sprite asset.
    private Grid unitGrid;  // Grid that units move on.

    // Start is called before the first frame update
    void Start()
    {

        gridBox = Instantiate(gridBox);

        // TODO: likely want some sort of GameManger / Game object, which keeps track the grid.
        // In that case just reference Game.instance.playerGrid.
        unitGrid = GameObject.Find("UnitGrid").GetComponent<Grid>();

    }

    // Update is called once per frame
    void Update()
    {

        // If just clicked on this object, set selected = true

        // If selected, draw a box over whichever cell is selected

        // If mouse button just released,
        // If was on this cell, do nothing
        // If was on another cell, start movement coroutine

        // movement coroutine - step towards the other cell

        // Debug - draw gridBox at cursor
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // Converting to cell and then back should yield something grid-aligned (?)
        Vector3Int cellPosition = unitGrid.WorldToCell(worldPosition);
        worldPosition = unitGrid.CellToWorld(cellPosition);

        // Make position align to 16x16 grid.
//        worldPosition.x -= worldPosition.x % 16;
//        worldPosition.y -= worldPosition.y % 16;
//        worldPosition.z = 0f;  // This is a gotcha, will have z = -10 from Camera.main.ScreenToWorldPoint, which makes it invisible.

        // Set transform of gridbox
        gridBox.transform.position = worldPosition;


    }
}
