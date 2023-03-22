using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogActivator : MonoBehaviour
{
    public TextAsset textObject;

    public bool requireButtonPress;
    private bool waitForPress;

    public bool destroyWhenActivated;

    // Update is called once per frame
    void Update()
    {
        if(waitForPress && Input.GetKeyDown(KeyCode.E) && !DialogueManager.GetInstance().getIsActive())
        {
            DialogueManager.GetInstance().EnterDialogueMode(textObject);

            if (destroyWhenActivated)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //if an object/collider requires a button press
        if (requireButtonPress)
        {
            //waiting for a button press in update before printing to screen
            waitForPress = true;
            return;
        }
        DialogueManager.GetInstance().EnterDialogueMode(textObject);

        if (destroyWhenActivated)
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        //so the player doesn't activate on their way out
        if (other.name == "Player")
        {
            waitForPress = false;
        }
    }
}
