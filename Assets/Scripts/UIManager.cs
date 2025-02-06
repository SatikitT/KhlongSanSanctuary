using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject buttonGroup;
    public PlayerData playerData;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI faithText;

    public ShopManager shopManager;
    private Animator canvasAnimator; 

    void Start()
    {
        canvasAnimator = canvas.GetComponent<Animator>();
        

        if (canvasAnimator == null)
        {
            Debug.LogError("Animator not found on the canvas.");
        }

    }

    public void ToggleConstructionPanel()
    {
        if (canvasAnimator == null) return;

        bool isOpen = canvasAnimator.GetBool("isConstructionPanelOpen");

        if (!isOpen && shopManager.selectedButtonImage != null)
        {
            shopManager.selectedButtonImage.color = Color.black;
            shopManager.CancelPlacement();
        }

        canvasAnimator.SetBool("isConstructionPanelOpen", !isOpen);
    }

    public void ToggleSettingPanel()
    {
        if (canvasAnimator == null) return;

        bool isOpen = canvasAnimator.GetBool("isSettingPanelOpen");

        canvasAnimator.SetBool("isSettingPanelOpen", !isOpen);
    }
    public void ToggleStatisticPanel()
    {
        if (canvasAnimator == null) return;

        bool isOpen = canvasAnimator.GetBool("isStatisticPanelOpen");

        canvasAnimator.SetBool("isStatisticPanelOpen", !isOpen);
    }

    private void Update()
    {
        moneyText.text = playerData.GetMoney().ToString();
        faithText.text = playerData.GetFaith().ToString();
    }

}
