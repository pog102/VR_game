using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject registrationMenu;
    public GameObject keyboardMenu;

    // void Start()
    // {
    //     ShowMainMenu();
    // }

    // public void ShowMainMenu()
    // {
    //     mainMenu.SetActive(true);
    //     registrationMenu.SetActive(false);
    //     keyboardMenu.SetActive(false);
    // }

    public void ShowRegistration()
    {
        mainMenu.SetActive(false);
        registrationMenu.SetActive(true);
        keyboardMenu.SetActive(true);
    }
}
