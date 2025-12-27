using System.Collections.Generic;
using UnityEngine;
using Match3.Core;

public class ObjectivesPanelView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform objectivesListRoot;
    [SerializeField] private ObjectiveRowView objectiveRowPrefab;

    [Header("Objective Icon Sprites")]
    [SerializeField] private List<Sprite> objectiveIconSprites;
    private readonly List<ObjectiveRowView> _rows = new();

    public void Clear()
    {
        if (objectivesListRoot == null) return;

        for (int i = objectivesListRoot.childCount - 1; i >= 0; i--)
            Destroy(objectivesListRoot.GetChild(i).gameObject);

        _rows.Clear();
    }

    public ObjectiveRowView AddRow()
    {
        var row = Instantiate(objectiveRowPrefab, objectivesListRoot);
        _rows.Add(row);
        return row;
    }

    public ObjectiveRowView GetRow(int index)
    {
        if (index < 0 || index >= _rows.Count) return null;
        return _rows[index];
    }

    public Sprite GetIcon(ObjectiveType type)
    {
        int index = type switch
        {
            ObjectiveType.RedTile => 0,
            ObjectiveType.BlueTile => 1,
            ObjectiveType.GreenTile => 2,
            ObjectiveType.YellowTile => 3,
            ObjectiveType.Stone => 4,
            _ => -1
        };

        if (index < 0 || objectiveIconSprites == null || index >= objectiveIconSprites.Count)
            return null;

        return objectiveIconSprites[index];
    }
}
