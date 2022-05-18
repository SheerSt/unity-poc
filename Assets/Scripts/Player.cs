using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{

    private bool isDragging = false;
    private Vector3Int prevCellPosition;

    void Awake()
    {

    }

    // Start is called before the first frame update
    protected override void Start()
    {

        gridBox = Instantiate(gridBox);
        gridBox.SetActive(false);
        base.Start();

    }

    // Update is called once per frame
    protected override void Update()
    {

        // Mouse input section.
        if (isMoving) return;

        // Check if just clicked on this object
        if (Input.GetMouseButtonDown(0))
        {

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (boxCollider.OverlapPoint(mousePosition))
            {

                // isDragging tells this to later check if user releases mouse to signal movement.
                isDragging = true;
                foreach (GameObject gridBox in gridBoxes) gridBox.SetActive(true);
                // Must be set active after the other gridboxes in order to appear above them.
                gridBox.SetActive(true);

            }
            else foreach (GameObject gridBox in gridBoxes) gridBox.SetActive(false);

        }

        // If user just clicked right-mouse, cancel any movement.
        if (Input.GetMouseButtonDown(1))
        {

            isDragging = false;
            // Stop showing the gridbox.
            gridBox.SetActive(false);

        }

        // Show the gridbox if the mouse is being dragged.
        if (!isDragging) return;

        if (Input.GetMouseButtonUp(0)) MouseUp();

        // Draw gridBox at cursor
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // Converting to cell and then back should yield something grid-aligned.
        Vector3Int cellPosition = Game.manager.unitGrid.WorldToCell(worldPosition);
        if (cellPosition != prevCellPosition)
        {
            if (cellPositions.Count <= 0 || cellPositions.Contains((Vector2Int)cellPosition))
            {
                prevCellPosition = cellPosition;
                worldPosition = Game.manager.unitGrid.CellToWorld(cellPosition);

                // Set transform of gridbox
                gridBox.transform.position = worldPosition;

                // Have the sprite look in the direction of the candidate world position.
                SpriteLookAt(worldPosition);

            }

        }

    }

    private void MouseUp()
    {

        if (isMoving) return;

        // Return if player cancelled the move to this position.
        //if (!isDragging) return;  // TODO: remove
        isDragging = false;

        // Stop showing the gridbox.
        gridBox.SetActive(false);

        // If mouse button just released,
        // If was on this cell, do nothing
        // If was on another cell, start movement coroutine

        // Compare the gridbox cell position to this cell position.
        Vector3Int gridboxCellPosition = Game.manager.unitGrid.WorldToCell(gridBox.transform.position);
        Vector3Int playerCellPosition = Game.manager.unitGrid.WorldToCell(transform.position);

        if (playerCellPosition != gridboxCellPosition)
        {

            StartCoroutine(Move(Game.manager.unitGrid.GetCellCenterWorld(gridboxCellPosition)));
            isMoving = true;
            spriteAnimator.SetBool("walk", true);

        }

    }

}
