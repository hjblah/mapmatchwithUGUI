using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    [SerializeField] private string debugGPS;

    [Tooltip("Right Top")]
    [SerializeField] private string miniMapMax;
    [Tooltip("Left Bottom")]
    [SerializeField] private string miniMapMin;

    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject miniMapChar;

    private double currentLatitude;
    private double currentLongitude;

    private string[] max;
    private string[] min;

    private Vector2 mapSize;
    private float mapRate;

    // Start is called before the first frame update

    void Start()
    {
#if UNITY_ANDROID
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
#endif
        Input.location.Start();

        max = miniMapMax.Split(',');
        min = miniMapMin.Split(',');

        mapRate = miniMap.GetComponent<RectTransform>().sizeDelta.y / miniMap.GetComponent<RectTransform>().sizeDelta.x;
        mapSize = new Vector2(Screen.width, Screen.width * mapRate);
        miniMap.GetComponent<RectTransform>().sizeDelta = mapSize;
        miniMap.transform.GetComponent<RectTransform>().position = new Vector3(miniMap.transform.GetComponent<RectTransform>().position.x, miniMap.transform.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
    }

    private void Update()
    {
#if UNITY_EDITOR
        string[] gps = debugGPS.Split(',');
        currentLatitude = Convert.ToDouble(gps[0]);
        currentLongitude = Convert.ToDouble(gps[1]);
#elif UNITY_ANDROID
        currentLatitude = Input.location.lastData.latitude;
        currentLongitude = Input.location.lastData.longitude;
#endif
        getLocation(currentLatitude, currentLongitude);

    }

    public void getLocation(double lat, double lng) {
        currentLatitude = lat;
        currentLongitude = lng;



        GameObject ins = Instantiate(miniMapChar, MapPosPointCreate(currentLatitude, currentLongitude), Quaternion.identity, GameObject.Find("Canvas").transform);
        Destroy(ins, 1.5f);
    }

    private Vector2 MapPosPointCreate(double latitude_gps, double longitude_gps)
    {
        double latitude = (latitude_gps - Convert.ToDouble(min[0])) / (Convert.ToDouble(max[0]) - Convert.ToDouble(min[0]));
        double longitude = (longitude_gps - Convert.ToDouble(min[1])) / (Convert.ToDouble(max[1]) - Convert.ToDouble(min[1]));

        return new Vector2(((float)longitude * miniMap.GetComponent<RectTransform>().sizeDelta.x), (float)latitude * miniMap.GetComponent<RectTransform>().sizeDelta.y);
    }
}
