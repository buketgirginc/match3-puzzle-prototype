using System.Collections.Generic;
using UnityEngine;
using Match3.Core;
using Match3.Gameplay;

public class ObjectivesPanelPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectivesPanelView panelView;
    [SerializeField] private GameState gameState;

    [Header("Board Layout")]
    [SerializeField] private BoardWorldLayoutFromUI boardLayout;

    private readonly List<ObjectiveRowView> _rows = new();

    private void Awake()
    {
        if (panelView == null)
            panelView = GetComponent<ObjectivesPanelView>();

        if (boardLayout == null)
            boardLayout = FindFirstObjectByType<BoardWorldLayoutFromUI>();
    }

    private void OnEnable()
    {
        if (panelView == null || gameState == null) return;

        BuildRowsIfNeeded();
        RefreshAll(playFeedback: false);

        ApplyBoardLayout();

        gameState.ObjectivesReset += OnObjectivesReset;
        gameState.ObjectiveProgressChanged += OnObjectiveProgressChanged;
        gameState.StoneProgressChanged += OnStoneProgressChanged;
    }

    private void OnDisable()
    {
        if (gameState == null) return;

        gameState.ObjectivesReset -= OnObjectivesReset;
        gameState.ObjectiveProgressChanged -= OnObjectiveProgressChanged;
        gameState.StoneProgressChanged -= OnStoneProgressChanged;
    }

    private void OnObjectivesReset()
    {
        BuildRowsIfNeeded();
        RefreshAll(playFeedback: false);
        ApplyBoardLayout();
    }

    private void OnObjectiveProgressChanged(int index)
    {
        RefreshRow(index, playFeedback: true);
    }

    private void OnStoneProgressChanged()
    {
        if (!gameState.HasStoneObjective) return;
        int stoneRowIndex = gameState.Objectives.Count; // appended row
        RefreshRow(stoneRowIndex, playFeedback: true);
    }

    private void BuildRowsIfNeeded()
    {
        var objectives = gameState.Objectives;
        if (objectives == null) return;

        int desired = objectives.Count + (gameState.HasStoneObjective ? 1 : 0);

        if (_rows.Count != desired)
        {
            panelView.Clear();
            _rows.Clear();

            for (int i = 0; i < desired; i++)
            {
                var row = panelView.AddRow();
                _rows.Add(row);
            }

            //Panel yüksekliği değişti -> board hizasını bir sonraki frame sonunda uygula
            ApplyBoardLayout();
        }
    }

    private void RefreshAll(bool playFeedback)
    {
        for (int i = 0; i < _rows.Count; i++)
            RefreshRow(i, playFeedback);
    }

    private void RefreshRow(int index, bool playFeedback)
    {
        if (index < 0 || index >= _rows.Count) return;

        int tileObjectiveCount = gameState.Objectives.Count;

        //Tile objective rows
        if (index < tileObjectiveCount)
        {
            var obj = gameState.Objectives[index];
            var row = _rows[index];

            var uiType = ToObjectiveType(obj.type);

            row.SetIcon(panelView.GetIcon(uiType));
            row.SetText($"{GetDisplayName(obj.type)}: {obj.current}/{obj.target}");
            row.SetCompleted(obj.current >= obj.target, playFeedbackIfJustCompleted: playFeedback);
            return;
        }

        // Stone objective row
        if (gameState.HasStoneObjective && index == tileObjectiveCount)
        {
            var row = _rows[index];

            int cur = gameState.StonesBrokenCurrent;
            int target = gameState.StonesBrokenTarget;

            row.SetIcon(panelView.GetIcon(ObjectiveType.Stone));
            row.SetText($"Stone: {cur}/{target}");
            row.SetCompleted(cur >= target, playFeedbackIfJustCompleted: playFeedback);
        }
    }

    private void ApplyBoardLayout()
    {
        if (boardLayout == null) return;
        boardLayout.RequestApply();
    }

    private ObjectiveType ToObjectiveType(TileType tileType)
    {
        return tileType switch
        {
            TileType.Red => ObjectiveType.RedTile,
            TileType.Blue => ObjectiveType.BlueTile,
            TileType.Green => ObjectiveType.GreenTile,
            TileType.Yellow => ObjectiveType.YellowTile,
            _ => ObjectiveType.RedTile
        };
    }

    private string GetDisplayName(TileType tileType)
    {
        return tileType switch
        {
            TileType.Red => "Red",
            TileType.Blue => "Blue",
            TileType.Green => "Green",
            TileType.Yellow => "Yellow",
            _ => tileType.ToString()
        };
    }
}
