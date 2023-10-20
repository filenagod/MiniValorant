using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    public List<EnemyController> enemyControllers;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject panel;
    private void Awake()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    public void Remove(EnemyController enemyController)
    {
        enemyControllers.Remove(enemyController);
        if(enemyControllers.Count == 0 )
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Invoke("TimeScale", 1f);
            winPanel.SetActive(true);
            losePanel.SetActive(false);
            panel.SetActive(false);
            
        }
    }

    public void TimeScale()
    {
        Time.timeScale = 0;

    }

    public void LosePanel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        losePanel.SetActive(true);
        winPanel.SetActive(false);
        panel.SetActive(false);
    }
   
}
