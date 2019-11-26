using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ChatMessage : MonoBehaviour, IPointerClickHandler
{
    bool fadeOut;
    bool canSleep;

    public float DisappearTime = 20.0f;
    public float DisappearSpeed = 2.0f;

    IEnumerator CloseCoroutine;
    TextMeshProUGUI text;
    float alpha = 1;

    public void Initialize(bool canSleep, string message)
    {
        text = GetComponent<TextMeshProUGUI>();
        text.SetText(message);

        if (canSleep)
            Sleep();
        else
            Wake();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            // was a link clicked?
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }

    private IEnumerator SleepMessage()
    {
        alpha = 1;
        fadeOut = false;
        yield return new WaitForSeconds(DisappearTime);
        fadeOut = true;
    }

    private void LateUpdate()
    {
        if (!fadeOut || !canSleep || alpha <= 0)
            return;

        alpha -= DisappearSpeed * Time.deltaTime;
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }

    public void Wake()
    {
        canSleep = false;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
    }

    public void Sleep()
    {
        canSleep = true;

        if (CloseCoroutine != null)
            StopCoroutine(CloseCoroutine);

        CloseCoroutine = SleepMessage();
        StartCoroutine(CloseCoroutine);
    }
}
