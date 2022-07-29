using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class ToggleManager : MonoBehaviour
{
    public LineRenderer aStarLineRenderer;
    public LineRenderer dijkstraLineRenderer;
    public LineRenderer nativeLineRenderer;
    public GameObject aStar;
    public GameObject dijkstra;
    public GameObject nativeNav;
    private PlayerNav native;
    public Toggle aStarToggle;
    public Toggle dijkstraToggle;
    public Toggle nativeToggle;
    public NavMeshAgent nativeCol;

    private void Start()
    {
        native = nativeNav.GetComponent<PlayerNav>();
    }

    public void EnableAStar(bool toggle)
    {
        aStar.SetActive(toggle);
        dijkstra.SetActive(false);
        native.active = false;
        dijkstraToggle.isOn = false;
        nativeToggle.isOn = false;
        dijkstraLineRenderer.positionCount = 0;
        nativeLineRenderer.positionCount = 0;
        nativeCol.enabled = false;
    }
    public void EnableDijkstra(bool toggle)
    {
        dijkstra.SetActive(toggle);
        aStar.SetActive(false);
        native.active = false;
        aStarToggle.isOn = false;
        nativeToggle.isOn = false;
        nativeLineRenderer.positionCount = 0;
        aStarLineRenderer.positionCount = 0;
        nativeCol.enabled = false;
    }
    public void EnableNative(bool toggle)
    {
        native.active = toggle;
        nativeCol.enabled = true;
        aStar.SetActive(false);
        dijkstra.SetActive(false);
        aStarToggle.isOn = false;
        dijkstraToggle.isOn = false;
        aStarLineRenderer.positionCount = 0;
        dijkstraLineRenderer.positionCount = 0;
    }
}