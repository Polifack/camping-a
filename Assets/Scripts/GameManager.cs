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
    public GameObject interactUI;


    // DAYNIGHT CYCLE
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



    // INTERACT UI
    public void enableInteractUI()
    {
        interactUI.SetActive(true);
    }
    public void disableInteractUI()
    {
        interactUI.SetActive(false);
    }
    public void printInteractPrompt(string name, string content)
    {
        InteractPromptManager interactWindow = interactUI.GetComponent<InteractPromptManager>();
        interactWindow.show(name, content);
    }



    // DIALOG UI
    public void enableTalkUI()
    {
        talkUI.SetActive(true);
    }
    public void disableTalkUI()
    {
        talkUI.SetActive(false);
    }
    public void printDialog(string name, string content)
    {
        DialogManager dialogWindow = talkUI.GetComponent<DialogManager>();
        dialogWindow.show(name, content);
    }
    public bool isTalkingDone()
    {
        DialogManager dialogWindow = talkUI.GetComponent<DialogManager>();
        return !dialogWindow.isWriting;
    }


    // SINGLETON
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
}
