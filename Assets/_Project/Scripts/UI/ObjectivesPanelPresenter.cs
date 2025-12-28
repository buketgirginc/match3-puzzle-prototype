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

        //boardu hizala
        ApplyBoardLayout();

        gameState.ObjectivesReset += OnObjectivesReset;
        gameState.ObjectiveProgressChanged += OnObjectiveProgressChanged;
    }

    private void OnDisable()
    {
        if (gameState == null) return;

        gameState.ObjectivesReset -= OnObjectivesReset;
        gameState.ObjectiveProgressChanged -= OnObjectiveProgressChanged;
    }

    private void OnObjectivesReset()
    {
        BuildRowsIfNeeded();
        RefreshAll(playFeedback: false);

        ApplyBoardLayout();
    }

    private void BuildRowsIfNeeded()
    {
        var objectives = gameState.Objectives;
        if (objectives == null) return;

        // Eğer sayılar değiştiyse (level değişimi vs), yeniden kur.
        if (_rows.Count != objectives.Count)
        {
            panelView.Clear();
            _rows.Clear();

            for (int i = 0; i < objectives.Count; i++)
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
        var objectives = gameState.Objectives;
        if (objectives == null) return;

        for (int i = 0; i < objectives.Count; i++)
            RefreshRow(i, playFeedback);
    }

    private void OnObjectiveProgressChanged(int index)
    {
        RefreshRow(index, playFeedback: true);
    }

    private void RefreshRow(int index, bool playFeedback)
    {
        var objectives = gameState.Objectives;
        if (objectives == null) return;
        if (index < 0 || index >= objectives.Count) return;
        if (index < 0 || index >= _rows.Count) return;

        var obj = objectives[index];
        var row = _rows[index];

        // TileType -> ObjectiveType (UI sprite seçimi için)
        var uiType = ToObjectiveType(obj.type);

        row.SetIcon(panelView.GetIcon(uiType));
        row.SetText($"{GetDisplayName(obj.type)}: {obj.current}/{obj.target}");
        row.SetCompleted(obj.current >= obj.target, playFeedbackIfJustCompleted: playFeedback);
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
            _ => ObjectiveType.RedTile // Empty burada gelmemeli; güvenlik için
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
