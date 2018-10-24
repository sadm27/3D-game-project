﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseManagerS : MonoBehaviour {


    public GameObject selectedUnit;
    public GameObject UnitInfo;

    public Text UnitName;
    public Text UnitHealth;
    public Text OnTileFood;
    public Text OnTileWood;
    public Text OnTileStone;
    public Text OnTileGold;


    public TileMap map;

    // Use this for initialization
    void Start () {
		
	}
	

	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {

            //if you are having issues with clicking through the UI use this check on you mouse click / or selection functions
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)
            {
                
                if (hitInfo.transform.gameObject.tag == "Unit tag")
                {
                    
                    GameObject hitObject = hitInfo.transform.root.gameObject;
                    Debug.Log("tile" + hitInfo.transform.gameObject.name);

                    SelectUnit(hitObject);
                    UnitInfo.gameObject.SetActive(true);
                }                
            }
            else
            {
                if (Input.GetButtonDown("Cancel"))
                {
                    ClearSelection();
                    UnitInfo.gameObject.SetActive(false);
                }
            }

        }
        if (Input.GetButtonDown("Cancel"))
        {
            ClearSelection();
            UnitInfo.gameObject.SetActive(false);
        }
    }


    void SelectUnit(GameObject obj) {
		if(selectedUnit != null) {
			if(obj == selectedUnit)
				return;

			ClearSelection();
		}

        selectedUnit = obj;

		Renderer[] rs = selectedUnit.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in rs) {
			Material m = r.material;
			m.color = Color.green;
			r.material = m;
		}
	}

	void ClearSelection() {
		if(selectedUnit == null)
			return;

		Renderer[] rs = selectedUnit.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in rs) {
			Material m = r.material;
			m.color = Color.white;
			r.material = m;
		}

        OnTileWood.text = "0";
        OnTileStone.text = "0";
        OnTileFood.text = "0";
        Debug.Log("THE RESORCES SOULD BE CLEARD OUT!");

        selectedUnit = null;
	}








    public void RunMoveNext()
    {
        selectedUnit.GetComponent<Unit>().MoveNextTile();
    }








}
