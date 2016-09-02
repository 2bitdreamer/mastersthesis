using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildMenu : MonoBehaviour
{

    public GameObject m_prefabButton;
    public RectTransform m_parentPanel;
    public HexGrid m_hexGridRef;
    private List<GameObject> m_buildUnitButtons;
    private GameObject m_buildMenuCanvas;

    // Use this for initialization
    void Start()
    {
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        m_buildUnitButtons = new List<GameObject>();

        m_buildMenuCanvas = GameObject.FindWithTag("BuildMenuCanvas");
        m_buildMenuCanvas.SetActive(false);
        //GenerateMenus();

    }

    public void EnterMenu()
    {

        if (m_buildUnitButtons.Count != 0)
        {
            m_buildMenuCanvas.SetActive(true);
        }
        else
        {
            for (int i = 0; i < HexGrid.s_unitBlueprints.Count; i++)
            {
                Unit blueprint = HexGrid.s_unitBlueprints[i];
                string unitName = blueprint.m_name;
                int unitCost = blueprint.m_cost;
                string buttonString = unitName + " (" + unitCost + "g" + ")";

                GameObject goButton = Instantiate(m_prefabButton);

                goButton.transform.SetParent(m_parentPanel, false);
                goButton.transform.localScale = new Vector3(1, 1, 1);

                Button tempButton = goButton.GetComponent<Button>();
                tempButton.interactable = true;
                int tempInt = i;

                tempButton.onClick.AddListener(() => ButtonClicked(tempInt));
                Text buttonText = goButton.transform.GetComponentInChildren<Text>();
                buttonText.text = buttonString;

                m_buildUnitButtons.Add(goButton);
            }

            m_buildMenuCanvas.SetActive(true);
        }
    }

    public void ExitMenu()
    {
        m_buildMenuCanvas.SetActive(false);
    }

    void ButtonClicked(int buttonNo)
    {
        m_hexGridRef.BuildUnit(buttonNo);
        ExitMenu();
    }

}