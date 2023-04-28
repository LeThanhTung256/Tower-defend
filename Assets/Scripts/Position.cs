using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    [SerializeField]
    private GameObject blockPrefabs;
    [SerializeField]
    private Material normalMat;
    [SerializeField]
    private Material activeMat;
    [SerializeField]
    private Material selectedMat;
    private Renderer blockRenderer;

    private GameObject block;
    private bool isUsed;
    public float size = 1.2f;

    public void SetValue(Vector2 gridPosition, Vector2 center)
    {
        Vector2 position2D = (gridPosition - center) * size;
        isUsed = false;
        transform.position = new Vector3(position2D.x, 0, -position2D.y);
        block = Instantiate(blockPrefabs, transform);
        blockRenderer = block.GetComponent<Renderer>();
    }

    public void Active()
    {
        if (isUsed)
            return;

        blockRenderer.material = activeMat;
    }

    public void Inactive()
    {
        blockRenderer.material = normalMat;
    }

    public void Use()
    {
        isUsed = true;
    }

    public void Reset()
    {
        isUsed = false;
    }

    public bool IsUsed()
    {
        return isUsed;
    }

    public void Selecting()
    {
        blockRenderer.material = selectedMat;
    }

    public void UnSelect()
    {
        blockRenderer.material = activeMat;
    }
}
