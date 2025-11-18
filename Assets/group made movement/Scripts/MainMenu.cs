//using System.Collections;
using TMPro;
//using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI mainMenu;
    public Image mainMenuBackground;
    [SerializeField] private TextMeshProUGUI mainSettings;
    public Image mainSettingsBackground;

    [Header("Padding")]
    public float padding = 10f; // padding around text


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
