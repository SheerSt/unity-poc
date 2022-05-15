using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.Animation;

public class Unit : MonoBehaviour
{
    public GameObject gridBox;
    public float moveTime;

    protected bool isMoving = false;
    protected Animator animator;
    protected BoxCollider2D boxCollider;
    protected List<GameObject> gridBoxes = new List<GameObject>();
    protected List<Vector2Int> cellPositions = new List<Vector2Int>();

    // Stats
    protected int attack;
    protected int maxHp;
    protected int hp;
    protected int speed;

    private SpriteResolver spriteResolver;

    // Start is called before the first frame update
    protected virtual void Start()
    {

        // Keep track of necessary components.
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteResolver = GetComponent<SpriteResolver>();

        // Only way I could get the SetLabel event to work.
        if (spriteResolver != null) spriteResolver.enabled = false;

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    private void OnMouseEnter()
    {
        string template = "{0}\n\n"
                        + "  {1}/{2}\n\n"
                        + "  {3}   {4}";

        Game.manager.mouseover.SetActive(true);
        Game.manager.mouseoverText.text = string.Format(template, this.name, this.hp, this.maxHp, this.attack, this.speed);

    }

    private void OnMouseOver()
    {

        // Convert to Vector2 then Vector3 in order to remove the z component.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.x += .05f;
        mousePosition.y += .15f;
        mousePosition.z = 0f;
        Game.manager.mouseover.transform.position = mousePosition;

    }

    private void OnMouseExit()
    {
        Game.manager.mouseover.SetActive(false);
    }

    /**
     * Moves the Unit to a given position.
     */
    protected IEnumerator Move(Vector3 end)
    {
        boxCollider.enabled = false;

        // Destroy all of the gridboxes.
        foreach (GameObject gridBox in gridBoxes) Destroy(gridBox);

        // Get BFS path to target position.
        int maxDistance = 12;
        Vector2Int playerCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(transform.position));
        Vector2Int targetCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(end));
        List<Vector2Int> path = BFS(playerCellPosition, targetCellPosition, maxDistance);

        for (int i = 0; i < path.Count; ++i)
        {

            Vector2Int targetCell = path[i];
            Vector3 targetWorldPosition;

            if (i < path.Count - 1)
            {

                //Vector3 startWorldPosition;

                Vector2Int nextCell = path[i + 1];
                targetWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)nextCell);

                // Raycast to the cell to see if there's something solid in the way.
                // If there isn't, continue.
                // RaycastHit2D hit = Physics2D.Linecast(transform.position, targetWorldPosition);  // <-- Worked but didn't like as much.
                Vector2 direction = targetWorldPosition - transform.position;
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.05f, direction, direction.magnitude);
                if (hit.transform == null) continue;

            }

            // Have the sprite look in the direction of the target world position.
            targetWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)targetCell);
            SpriteLookAt(targetWorldPosition);

            // Moves the player to a position above the cell.
            // Done that way because the Player's transform is in the middle.
            float sqrRemainingDistance = (transform.position - targetWorldPosition).sqrMagnitude;

            // Yield while moving the player.
            while (sqrRemainingDistance > float.Epsilon)
            {

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveTime * Time.deltaTime);
                sqrRemainingDistance = (transform.position - targetWorldPosition).sqrMagnitude;
                yield return null;

            }

        }

        // Done moving.
        transform.position = end;
        isMoving = false;
        animator.SetBool("walk", false);

        // TODO: move to method
        // Re-populate the positions that the player can move to.
        playerCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(transform.position));
        cellPositions.Clear();
        cellPositions.AddRange(BFS(playerCellPosition, null, maxDistance));

        gridBoxes.Clear();

        float sqrMaxDistance = maxDistance * maxDistance / 150f;

        foreach (Vector2Int cellPosition in cellPositions)
        {

            GameObject instance = Instantiate(gridBox);
            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
            instance.transform.position = Game.manager.unitGrid.CellToWorld((Vector3Int)cellPosition);

            // Fade alpha slightly the further the cell is from the player.
            float sqrDistance = (instance.transform.position - transform.position).sqrMagnitude;
            float alpha = sqrMaxDistance / sqrDistance;
            if (alpha > 1f) alpha = 1f;
            alpha -= 0.1f;

            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, alpha);

            instance.SetActive(false);
            gridBoxes.Add(instance);

        }

        boxCollider.enabled = true;

    }

    /**
     * TODO: tried using boxcolliders, can't get that to work. Ideally get that working at some point.
     */
    public bool SolidTileIsHere(Vector3 worldPosition)
    {

        Vector2Int[] collisionOffsets = new Vector2Int[] { Vector2Int.zero, Vector2Int.right, new Vector2Int(1, 1), Vector2Int.up };

        // There is probably a better way to do this but there's no documentation that I can find.
        SuperTiled2Unity.CustomProperty isSolid = new SuperTiled2Unity.CustomProperty();
        isSolid.m_Name = "isSolid";
        isSolid.m_Type = "bool";
        isSolid.m_Value = "true";

        foreach (Tilemap tileMap in Game.manager.tileMaps)
        {
            Vector3Int cellPosition = tileMap.WorldToCell(worldPosition);

            foreach (Vector2Int collisionOffset in collisionOffsets)
            {
                // Get the tile at this position + collisionOffset.
                SuperTile superTile = tileMap.GetTile<SuperTile>(cellPosition + (Vector3Int)collisionOffset);
                if (superTile == null) continue;

                foreach (CustomProperty property in superTile.m_CustomProperties)
                {
                    if (property.m_Name.Equals(isSolid.m_Name) && property.m_Value.Equals(isSolid.m_Value)) return true;
                }

            }

        }

        return false;

    }

    /**
     * Rotates the Unit's sprite in order to look at the given world position.
     */
    protected void SpriteLookAt(Vector3 targetWorldPosition)
    {

        // Rotate the unit so that it's facing the movement direction.
        float rotationAmount = transform.rotation.eulerAngles.y;
        if (transform.position.x > targetWorldPosition.x + 0.05f) rotationAmount = 180f;
        else if (transform.position.x < targetWorldPosition.x - 0.05f) rotationAmount = 0f;
        transform.rotation = Quaternion.Euler(0, rotationAmount, 0);

    }

    /**
     * If target is null, returns all non-solid tiles within maxDistance away.
     * Otherwise, returns the shortest path to target within maxDistance.
     */
    public List<Vector2Int> BFS(Vector2Int origin, Vector2Int? target, int maxDistance)
    {

        List<Vector2Int> allCells = new List<Vector2Int>();
        Queue<Vector2Int> checkThese = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int?> prevTiles = new Dictionary<Vector2Int, Vector2Int?>();

        Vector2Int pos = new Vector2Int(origin.x, origin.y);
        Vector2Int newPos;

        checkThese.Enqueue(pos);
        prevTiles.Add(pos, null);

        Vector2Int[] offsets = new Vector2Int[] { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

        while (checkThese.Count > 0)
        {

            pos = checkThese.Dequeue();

            foreach (Vector2Int offset in offsets)
            {

                newPos = pos + offset;

                // TODO: use a distance map instead
                // Check that the tile isn't too far away.
                if (Mathf.Abs(newPos.y - origin.y) + Mathf.Abs(newPos.x - origin.x) >= maxDistance) continue;

                // Continue if there is already a friendly unit here.
                Vector3 cellCenterWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)newPos);
                Collider2D hit = Physics2D.OverlapPoint(cellCenterWorldPosition);
                if (hit != null)
                {

                    Player allyUnit = hit.gameObject.transform.GetComponent<Player>();
                    if (allyUnit != null) continue;

                }

                // Continue if there is a solid object on this cell.
                Vector3 worldPosition = Game.manager.unitGrid.CellToWorld((Vector3Int)newPos);
                if (SolidTileIsHere(worldPosition)) continue;

                if (!prevTiles.ContainsKey(newPos))
                {

                    allCells.Add(newPos);
                    checkThese.Enqueue(newPos);
                    prevTiles.Add(newPos, pos);

                }

                // If the target position is found, return the path.
                if (newPos == target)
                {

                    allCells.Clear();
                    while (newPos != origin)
                    {

                        allCells.Insert(0, newPos);
                        newPos = prevTiles[newPos].Value;

                    }

                    return allCells;

                }

            }

        }

        return allCells;

    }

    /**
     * Very difficult to figure out.
     * 
     * Note: The spriteResolver must be disabled in order for this to work. It's disabled in Start() right now.
     */
    public void SetLabel(string label)
    {

        spriteResolver.SetCategoryAndLabel(this.name, label);

    }

}