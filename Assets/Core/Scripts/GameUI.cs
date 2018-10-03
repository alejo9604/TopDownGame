﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWavebanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;

    Spawner spawner;

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWay += OnNewWave;
    }

    void Start () {

        FindObjectOfType<Player>().OnDeath += GameOver;

	}
	
	void GameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);

    }

    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "one", "two", "three", "four", "five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = spawner.waves[waveNumber - 1].infinite ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "";
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float percent = 0;
        float speed = 3f;
        float delayTime = 1.5f;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while( percent >= 0)
        {
            percent += Time.deltaTime * speed * dir;

            if(percent >= 1)
            {
                percent = 1;
                if(Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWavebanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 45, percent);
            yield return null;
        }
    }


    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }

    }

    //UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
}
