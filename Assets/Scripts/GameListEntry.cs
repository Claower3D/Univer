using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameNameText;
    public TextMeshProUGUI playersText;
    public Button joinButton;

    private string gameInfo;
    private System.Action<string> onSelectedCallback;

    public void Setup(string info, System.Action<string> callback)
    {
        gameInfo = info;
        onSelectedCallback = callback;

        // Парсим информацию об игре
        string displayName = info;
        if (info.Contains("-"))
        {
            displayName = info.Split('-')[0].Trim();
        }

        gameNameText.text = displayName;
        playersText.text = Random.Range(1, 8) + "/8 игроков";

        joinButton.onClick.AddListener(OnJoinClicked);
    }

    void OnJoinClicked()
    {
        onSelectedCallback?.Invoke(gameInfo);
    }
}