using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{

    private bool isDragging = false;
    private Vector3Int prevCellPosition;

    void Awake()
    {

        // DontDestroyOnLoad(gameObject);

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

        // Cannot select a player if something is moving, or if this has already moved this turn.
        if (Game.manager.aPlayerIsMoving) return;
        if (hasMoved) return;

        // Handle mouse input section.
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

            Game.manager.aPlayerIsMoving = true;
            hasMoved = true;
            StartCoroutine(Move(Game.manager.unitGrid.GetCellCenterWorld(gridboxCellPosition)));

        }

    }


    /**
     * Moves the Unit to a given position.
     */
    public override IEnumerator Move(Vector3 end)
    {

        yield return StartCoroutine(base.Move(end));

        // If moved into the exit area, load new area.
        if (CheckforTileProperty("endZone", transform.position))
        {

            yield return StartCoroutine(Game.manager.GotoNextLevel());
            yield break;

        }

        // TODO: move to method vv Player.DoneMoving? or Game.manager.PlayerDoneMoving()

        // Refresh all BFS of Ally units
        Game.manager.aPlayerIsMoving = false;
        bool aPlayerCanMove = false;
        foreach (Player player in Game.manager.players)
        {

            if (!player.hasMoved)
            {

                aPlayerCanMove = true;
                player.RefreshGridBoxes();

            }

        }

        if (!aPlayerCanMove)
        {

            // BFS 4 units away.
            // Move to a random position.
            foreach (Enemy enemy in Game.manager.enemies)
            {

                // TODO: move to Enemy method vv

                Vector2Int enemyCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(enemy.transform.position));

                // If aggroing an Ally, move to that Ally.
                if (enemy.aggroAlly != null)
                {

                    Vector2Int allyCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(enemy.aggroAlly.transform.position));
                    List<Vector2Int> path = enemy.BFS(enemyCellPosition, allyCellPosition, enemy.speed);
                    if (path.Count > 0)
                    {

                        yield return enemy.StartCoroutine(enemy.Move(Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)allyCellPosition)));
                        continue;

                    }

                    // Wasn't able to path to the Ally, so stop aggroing the Ally.
                    enemy.aggroAlly = null;

                    // Unset the aggroed icon
                    if (enemy.statusIcons != null) enemy.statusIcons.SetCategoryAndLabel("all", "none");

                }


                // If unable to move to that Ally, stop aggroing the Ally.
                List<Vector2Int> cellPositions = enemy.BFS(enemyCellPosition, null, 4);
                if (cellPositions.Count > 0)
                {

                    enemyCellPosition = cellPositions[Random.Range(0, cellPositions.Count)];
                    yield return enemy.StartCoroutine(enemy.Move(Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)enemyCellPosition)));

                }
                // ^^

            }

            // Refresh all ally BFS and set hasMoved = false;
            foreach (Player player in Game.manager.players)
            {

                player.hasMoved = false;
                player.RefreshGridBoxes();

            }

        }

    }

    public override IEnumerator FaintAnimation()
    {

        // Update existing players.
        Game.manager.players.Remove(this);

        yield return StartCoroutine(base.FaintAnimation());

    }


}
