using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class updatePopUp : MonoBehaviour
{
    public void UpdateButton() {
        //"market://details?q=pname:com.myCompany.myAppName/"
        Application.OpenURL("market://details?id=com.hanulneotech.thearc");
    }
    public void NotUpdateButton() {
        this.gameObject.SetActive(false);
    }
}
