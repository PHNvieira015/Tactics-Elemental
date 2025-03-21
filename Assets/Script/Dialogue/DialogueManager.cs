using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject dialogueBox;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI textComponent;

    public static string ProtagonistName = "Edu";
    public DialogueTrigger[] triggers;
    public int triggerIndex = 0;
    public Message currentMessage;
    public Character[] charactersInDialogue;
    private Queue<Message> messages = new Queue<Message>();
    public bool isDialogueActive;
    public float typingSpeed;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        triggers[triggerIndex].TriggerDialogue();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (textComponent.text == currentMessage.message)
            {
                if(currentMessage.waitTime != 0)
                {
                    dialogueBox.SetActive(false);
                    StartCoroutine(WaitForNextDialogue(currentMessage.waitTime));
                    return;
                }
                DisplayNextDialogue();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = currentMessage.message;
            }
        }
    }

    public void StartDialogue(Dialogues dialogue, Character[] characters)
    {
        textComponent.text = $"";
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        charactersInDialogue = characters;
        if(messages != null)
            messages.Clear();
        foreach(Message message in dialogue.messages)
        {
            messages.Enqueue(message);
        }

        DisplayNextDialogue();
    }

    public void DisplayNextDialogue()
    {
        if(messages.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentMessage = messages.Dequeue();

        characterName.text = charactersInDialogue[currentMessage.characterID].name;
        if (characterName.text == "{ProtagonistName}")
            characterName.text = $"{ProtagonistName}";

        StopAllCoroutines();
        StartCoroutine(TypeSentence());
    }

    IEnumerator TypeSentence()
    {
        textComponent.text = "";
        currentMessage.message = currentMessage.message.Replace("{ProtagonistName}", ProtagonistName);

        bool isTag = false;
        string currentTag = "";

        foreach (char letter in currentMessage.message)
        {
            if (letter == '<')
            {
                isTag = true;
                currentTag += letter;
            }
            else if (letter == '>')
            {
                isTag = false;
                currentTag += letter;
                textComponent.text += currentTag;
                currentTag = "";
                continue;
            }
            else if (isTag)
            {
                currentTag += letter;
            }
            else
            {
                textComponent.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    IEnumerator WaitForNextDialogue(float wait)
    {
        yield return new WaitForSeconds(wait);
        dialogueBox.SetActive(true);
        DisplayNextDialogue();
    }

    IEnumerator WaitForNextTrigger(float wait)
    {
        yield return new WaitForSeconds(wait);
        triggers[triggerIndex].TriggerDialogue();
    }

    void EndDialogue()
    {
        triggerIndex++;
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        StartCoroutine(WaitForNextTrigger(3f));
    }
}
