using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using Cinemachine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    private AudioManager audioManager;
    private BackgroundAudioManager backgroundAudioManager;
    private new CinemachineVirtualCamera camera;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    //like enums to identify ink tags
    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string VOICE_TAG = "voice";
    private const string BACKGROUND_MUSIC = "bm";
    private const string CHANGE_CHARACTER = "changeChar";
    private const string MOVEMENT = "movement";
    //changes who/what camera is following
    private const string CAMERA = "camera";
    private const string CHANGE_RIGID = "rigid";

    public Animator portraitAnimator;

    //array of buttons for choices
    public GameObject[] choices;
    //array of the text for those choices, can be changed privately from here
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;

    private bool isActive;
    //allows me to toggle whether i want the player to move or not
    public PlayerMovement playerMovement;
    public bool stopPlayerMovement;

    private void Awake()
    {
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    public bool getIsActive()
    {
        return isActive;
    }

    private void Start()
    {
        isActive = false;
        dialoguePanel.SetActive(false);
        audioManager = FindObjectOfType<AudioManager>();
        backgroundAudioManager = FindObjectOfType<BackgroundAudioManager>();    //in case I want to change it later
        camera = FindObjectOfType<CinemachineVirtualCamera>();

        //get all the choices text from given json file
        //creates the array of choices text based on needed choices
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach(GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        isActive = true;
        dialoguePanel.SetActive(true);
        if (stopPlayerMovement)
        {
            playerMovement.canMove = false;
        }

        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        isActive = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        playerMovement.canMove = true;
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            //sets the current text to the next line
            dialogueText.text = currentStory.Continue();
            //display any choices if any
            DisplayChoices();
            HandleTags(currentStory.currentTags);
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        int index = 0;
        //assign the currentChoices to the choices text on the buttons
        foreach(Choice choice in currentChoices)
        {
            //enables the button
            choices[index].gameObject.SetActive(true);
            //assigns the text to the button text
            choicesText[index].text = choice.text;
            index++;
        }
        //go through remaining choices and deactivates them
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        //EventSystem requres for the gameObject to be cleared first
        //for at least one frame before we can select another object
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    private void HandleTags(List<string> currentTags)
    {
        //parses each tag into a key and value
        foreach (string tag in currentTags)
        {
            //with the key being the string on the left and value being the string on the right
            string[] splitTag = tag.Split(':');

            string tagKey = splitTag[0].Trim();     //trims out any whitespace chars
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case PORTRAIT_TAG:

                    portraitAnimator.Play(tagValue);

                    break;
                case VOICE_TAG:
                    //sending the audiosource component of a game object with the same name as the tag
                    audioManager.ChangeMusic(GameObject.Find(tagValue).GetComponent<AudioSource>());

                    break;
                //using a separate background music tag to play a separate track concurrent with any voice tracks
                case BACKGROUND_MUSIC:

                    backgroundAudioManager.ChangeMusic(GameObject.Find(tagValue).GetComponent<AudioSource>());

                    break;
                //"creates" a new character by toggling them visible via the mask interaction options
                case CHANGE_CHARACTER:

                    SpriteRenderer sprite = GameObject.Find(tagValue).GetComponent<SpriteRenderer>();
                    //if sprite is invisible, clear their mask
                    if (sprite.maskInteraction == SpriteMaskInteraction.VisibleInsideMask)
                    {
                        sprite.maskInteraction = SpriteMaskInteraction.None;
                    }
                    //otherwise apply the mask
                    else
                    {
                        sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    }

                    break;
                case MOVEMENT:

                    GameObject.Find(tagValue).GetComponent<PlayableDirector>().Play();

                    break;
                case CAMERA:

                    camera.Follow = GameObject.Find(tagValue).GetComponent<Transform>();

                    break;
                case CHANGE_RIGID:

                    Rigidbody2D rigidBody = GameObject.Find(tagValue).GetComponent<Rigidbody2D>();

                    if (rigidBody.bodyType == RigidbodyType2D.Static)
                    {
                        rigidBody.bodyType = RigidbodyType2D.Dynamic;
                    }
                    else
                    {
                        rigidBody.bodyType = RigidbodyType2D.Static;
                    }

                    break;
                default:
                    break;
            }
        }
    }

    //uses an OnClick method on a button to send a choice index to currentStory
    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
    }
}