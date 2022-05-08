
// Various blocks of unused code and examples.

/*  Checks for SuperTiled2Unity properties.
 *  Ended up using Tiled collision meshes instead (works with Unity's raycast methods).
public bool SolidTileIsHere(Vector3 worldPosition)
{

	Vector2Int[] collisionOffsets = new Vector2Int[] { Vector2Int.zero, Vector2Int.right, new Vector2Int(1, 1), Vector2Int.up };

	// There is probably a better way to do this but there's no documentation that I can find.
	SuperTiled2Unity.CustomProperty isSolid = new SuperTiled2Unity.CustomProperty();
	isSolid.m_Name = "isSolid";
	isSolid.m_Type = "bool";
	isSolid.m_Value = "true";

	foreach (Tilemap tileMap in tileMaps)
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
*/

/*
This wouldn't work no matter what I tried.
For now will need to set a tile's isSolid property.

public bool SolidTileIsHere(Vector2Int cellPosition)
{

	// Get the world position of the center of the 16x16 tile.
	Vector3 cellCenterWorldPosition = unitGrid.GetCellCenterWorld((Vector3Int)cellPosition);

	// TODO: Point-based raycasts may sometimes fail here, they were failing in BFS.
	//Collider2D hit = Physics2D.OverlapPoint(cellCenterWorldPosition, 0, -1f);
	//if (hit != null) return true;
	//RaycastHit2D hit = Physics2D.BoxCast(cellCenterWorldPosition, new Vector2(1f, 1f), 0f, Vector2.zero, .1f);
	//if (hit.transform != null) return true;
	Collider2D hit = Physics2D.OverlapBox(cellCenterWorldPosition, new Vector2(1f, 1f), 1f, 8);
	if (hit != null) return true;
	//RaycastHit2D hit = Physics2D.CircleCast(cellCenterWorldPosition, 0.3f, Vector2.one, .3f);
	//if (hit.transform != null) return true;

	return false;

}

*/


/*



                RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                // If user clicked on the Player, then show all movement gridboxes.
                if (raycastHit.transform == transform)
                {

                    gridBox.SetActive(true);
                    foreach (GameObject gridBox in gridBoxes)
                    {
                        gridBox.SetActive(true);
                    }

                }
                // Else hide all movement gridboxes.
                else
                {
                    Debug.Log("hoi2");

                    foreach (GameObject gridBox in gridBoxes)
                    {
                        gridBox.SetActive(false);
                    }

                }

            }
			*/