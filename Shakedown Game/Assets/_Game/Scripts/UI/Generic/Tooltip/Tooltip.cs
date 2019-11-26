using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    public TextMeshProUGUI TooltipText;
    public RectTransform Background;

    public void Initialize()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(transform.root.gameObject);
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void ShowTooltip(string tooltipString)
    {
        if (string.IsNullOrWhiteSpace(tooltipString))
            return;

        // We set this so that there's no teleporting
        transform.position = Input.mousePosition;

        gameObject.SetActive(true);

        TooltipText.SetText(tooltipString);

        Vector2 backgroundSize = new Vector2(TooltipText.preferredWidth, TooltipText.preferredHeight);
        Background.sizeDelta = backgroundSize;
        Background.anchorMin = Vector2.zero;
        Background.anchorMin = Vector2.zero;
        Background.anchoredPosition = new Vector2(backgroundSize.x, backgroundSize.y) / 2;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
