using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI killCount;
    [SerializeField] private TextMeshProUGUI coolDown;
    [SerializeField] private TextMeshProUGUI slowAvailabilty;

    [SerializeField] private CanvasGroup fadePanel;



    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthDisplay();

        UpdateKillCountDisplay();

        UpdateCoolDownDisplay();

        UpdateSlowAvailabiltyDisplay();
    }

    public void UpdateHealthDisplay()
    {
        health.text = "Current Health: " + Player.Instance.GetPlayerHealth();
    }

    public void UpdateKillCountDisplay() 
    {
        killCount.text = "Enemies Defeated: " + GameManager.Instance.GetKillCount();
    }

    public void UpdateCoolDownDisplay()
    {
        coolDown.text = "Cooldown: " + Math.Truncate(GameManager.Instance.GetSlowTimer());

    }

    public void UpdateSlowAvailabiltyDisplay()
    {
        bool isOnCoolDown = GameManager.Instance.GetIsOnCoolDown();

        if (isOnCoolDown)
        {
            slowAvailabilty.text = "Power Available? No!";
        } 
        else if (!isOnCoolDown)
        {
            slowAvailabilty.text = "Power Available? Yes! (Press F!)";
        }

    }

    public void GameOverFade()
    {
        // create fade, make sure gamemanager waits until screen is fully black
        StartCoroutine(FadePanel(0f, 1f));
        //GameManager.Instance.RestartScene();

    }

    private IEnumerator FadePanel(float start, float end)
    {
        float time = 0f;
        float duration = 1.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }

        fadePanel.alpha = end;

    }

}
