using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopUp : MonoBehaviour
{
    public Text displayedText;

    // private
    private Color textColour;

    private bool scrolling;
    private bool checkProximity;
    private bool checkOutsideProx;
    private Transform proxTarget;
    private float proxDistance = 10;

    private float scrollSpeed = 2;
    private float fadeWaitTime = 0.5f;
    private float fadeSpeed = 3;

    public void SetUp(string message, Color color)
    {
        displayedText = GetComponentInChildren<Text>();
        displayedText.text = message;
        displayedText.color = textColour;
        textColour = color;
        scrolling = true;
    }

    //overloaded statoinary 
    public void SetUp(string message, Color color, bool checkProximity, bool disableWhenClose, float proxDistance, Transform proxTarget, bool scrolling)
    {
        displayedText = GetComponent<Text>();
        textColour = color;
        displayedText.text = message;
        displayedText.color = textColour;
        this.checkProximity = checkProximity;
        checkOutsideProx = disableWhenClose;
        this.proxDistance = proxDistance;
        this.proxTarget = proxTarget;
        this.scrolling = scrolling;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.gameObject.transform.forward);

        if (checkProximity)
        {
            if (checkOutsideProx)
            {
                if (Vector3.Distance(this.transform.position, proxTarget.position) < proxDistance)
                {
                    checkProximity = false;
                }
            }
            else
            {
                if (Vector3.Distance(this.transform.position, proxTarget.position) > proxDistance)
                {
                    checkProximity = false;
                }
            }
        }

        if (checkProximity == false)
        {
            fadeWaitTime -= Time.deltaTime;

            if (scrolling) // only scroll while fading else it'll fly away
            {
                transform.position += new Vector3(0f, scrollSpeed * Time.deltaTime, 0);
            }

            if (fadeWaitTime <= 0) // time left to be alive expires
            {
                textColour.a -= fadeSpeed * Time.deltaTime;
                displayedText.color = textColour;

                if (textColour.a <= 0) // visibility is gone
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
