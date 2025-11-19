using UnityEngine;

public class OptionsManager : MonoBehaviour
{

    public static OptionsManager Instance { get; private set; }

    private CanvasGroup canvasGroup;
    private bool MenuOn = false;

    public bool IsMenuOpen => MenuOn;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    } 
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        MenuOn = !MenuOn;

        if (MenuOn)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Time.timeScale = 0f;
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Time.timeScale = 1f;
        }
    }

    public void CloseMenu()
    {
        MenuOn = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Time.timeScale = 1f;
    }
}