using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public static GameManager instance;

    public Material matDay;
    public GameObject dayLight;
    public Material matNight;
    public GameObject nightLight;

    public List<GameObject> lights = new List<GameObject>();

    public bool isDay = true;

    public AudioSource audioSource;
    
    public GameObject talkUI;

    // add an editor script to change the material of the skybox
    public void ChangeSkybox(bool isDay)
    {
        if (isDay)
        {
            RenderSettings.skybox = matDay;
            nightLight.SetActive(false);
            dayLight.SetActive(true);

            for (int i = 0; i < lights.Count; i++)
            {
                ((GameObject)lights[i]).SetActive(false);
            }
        }
        else
        {
            RenderSettings.skybox = matNight;
            dayLight.SetActive(false);
            nightLight.SetActive(true);

            for (int i = 0; i < lights.Count; i++)
            {
                ((GameObject)lights[i]).SetActive(true);
            }
        }
    }

    public void toggleTalkUI()
    {
        talkUI.SetActive(!talkUI.activeSelf);
    }
    public void printDialog(string name, string content)
    {
        DialogManager dialogWindow = talkUI.GetComponent<DialogManager>();
        dialogWindow.show(name, content);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public GameObject getPlayer()
    {
        return player;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            isDay = !isDay;
            ChangeSkybox(isDay);
        }
    }
}
