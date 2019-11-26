using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    private bool fadeOut;
    private bool enableIcon;

    [Header("Attributes")]
    public float FadeSpeed = 2.0f;

    [Header("References")]
    public Image PanelImage;
    public TextMeshProUGUI PanelText;
    public Image PanelIcon;
    Image[] Images;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        PanelImage.gameObject.SetActive(true);
        SceneManager.sceneLoaded += (scene, loadedMode) =>
        {
            DisableScreenAfterTime(0.2f, "Scene Loaded");
        };
        Images = GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        fadeOut = true;
        PanelImage.color = new Color(PanelImage.color.r, PanelImage.color.g, PanelImage.color.b, 0);
    }

    private IEnumerator DisableScreenAfterTime(float time, string text)
    {
        EnableScreen(text, false);
        yield return new WaitForSeconds(time);
        DisableScreen();
    }

    private void Update()
    {
        Color i = PanelImage.color;
        if (fadeOut && PanelImage.color.a > 0)
        {
            i = PanelImage.color;
            i.a -= FadeSpeed * Time.deltaTime;
        }
        else if (!fadeOut && PanelImage.color.a < 1)
        {
            i = PanelImage.color;
            i.a += FadeSpeed * Time.deltaTime;
        }

        i.a = Mathf.Clamp01(i.a);
        foreach (Image img in Images)
        {
            img.color = i;
        }
        //PanelImage.color = i;
        PanelText.color = i;
        PanelIcon.color = enableIcon ? i : Color.clear;

        if (PanelImage.color.a == 0)
            PanelImage.gameObject.SetActive(false);
        else if (PanelImage.color.a > 0)
            PanelImage.gameObject.SetActive(true);
    }

    public void EnableScreen(string text, bool enableIcon = true)
    {
        PanelText.SetText(text);
        fadeOut = false;
        this.enableIcon = enableIcon;
    }

    public void DisableScreen()
    {
        fadeOut = true;
    }

    public void DisableScreenFade(string text, float time)
    {
        StartCoroutine(DisableScreenAfterTime(time, text));
    }
}
