using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject dialogueBox;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI textComponent;

    public static gender ProtagonistGender;
    public static gender SiblingGender;

    public bool lastCutscene;
    public PlayableDirector director;
    public static string ProtagonistName;
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
        if (director.playOnAwake && triggers.Length > 0)
        {
            director.stopped += TriggerDialogue;
            director.paused += TriggerDialogue;
        }
        else if(!director.playOnAwake && triggers.Length > 0)
        {
            triggers[triggerIndex].TriggerDialogue();
        }
        if(triggers.Length == 0)
        {
            director.stopped += NextBattleFromCutscene;
        }
    }

    private void Update()
    {
        if (lastCutscene)
        {
            if (director.time > 1.75)
            {
                director.Pause();
                director.stopped -= TriggerDialogue;
                director.paused -= TriggerDialogue;
                lastCutscene = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Return) && isDialogueActive)
        {
            if (textComponent.text == currentMessage.message)
            {
                if (currentMessage.cutscene)
                {
                    director.Play();
                    director.playOnAwake = true;
                    director.stopped += DisplayNextDialogue;
                    dialogueBox.SetActive(false);
                    return;
                }
                if (currentMessage.waitTime != 0)
                {
                    dialogueBox.SetActive(false);
                    StartCoroutine(WaitForNextDialogue(currentMessage.waitTime));
                    return;
                }
                DisplayNextDialogue(null);
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = currentMessage.message;
            }
        }
    }

    public void TriggerDialogue(PlayableDirector director)
    {
        triggers[triggerIndex].TriggerDialogue();
    }

    public void StartDialogue(Dialogues dialogue, Character[] characters)
    {
        textComponent.text = $"";
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        charactersInDialogue = characters;
        if (messages != null)
            messages.Clear();
        foreach (Message message in dialogue.messages)
        {
            messages.Enqueue(message);
        }

        if (dialogue.lastCutscene)
        {
            isDialogueActive = false;
            DisplayLastCutsceneDialogue();
            return;
        }

        DisplayNextDialogue(null);
    }

    public void StartTypeCutscene(PlayableDirector director)
    {
        dialogueBox.SetActive(true);
        characterName.text = charactersInDialogue[currentMessage.characterID].name;
        currentMessage = messages.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeCutsceneSentence());
    }

    public void DisplayLastCutsceneDialogue()
    {
        dialogueBox.SetActive(true);
        if (messages.Count == 1)
        {
            dialogueBox.SetActive(false);
            director.Play();
            director.stopped += StartTypeCutscene;
            return;
        }
        if (messages.Count == 0)
        {
            EndDialogue();
            return;
        }
        characterName.text = charactersInDialogue[currentMessage.characterID].name;

        currentMessage = messages.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeCutsceneSentence());
    }

    public void DisplayNextDialogue(PlayableDirector director)
    {
        dialogueBox.SetActive(true);
        if (messages.Count == 0)
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

    IEnumerator TypeCutsceneSentence()
    {
        textComponent.text = "";
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
        yield return new WaitForSeconds(currentMessage.waitTime);
        DisplayLastCutsceneDialogue();
    }

    IEnumerator TypeSentence()
    {
        textComponent.text = "";
        currentMessage.message = currentMessage.message.Replace("{ProtagonistName}", ProtagonistName);
        currentMessage.message = currentMessage.message.Replace("{G1}", ProtagonistGender == gender.Masculino ? "o" : "a");
        currentMessage.message = currentMessage.message.Replace("{G2}", ProtagonistGender == gender.Masculino ? "meu" : "minha");
        currentMessage.message = currentMessage.message.Replace("{G3}", ProtagonistGender == gender.Masculino ? "ão" : "ã");
        currentMessage.message = currentMessage.message.Replace("{G4}", ProtagonistGender == gender.Masculino ? "ele" : "ela");
        currentMessage.message = currentMessage.message.Replace("{GS1}", SiblingGender == gender.Masculino ? "o" : "a");
        currentMessage.message = currentMessage.message.Replace("{GS2}", SiblingGender == gender.Masculino ? "meu" : "minha");
        currentMessage.message = currentMessage.message.Replace("{GS3}", SiblingGender == gender.Masculino ? "ã" : "ã");
        currentMessage.message = currentMessage.message.Replace("{GS4}", SiblingGender == gender.Masculino ? "ele" : "ela");

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
        if(triggerIndex + 1 >= triggers.Length)
        {
            EndDialogue();
            yield break;
        }
        dialogueBox.SetActive(true);
        DisplayNextDialogue(null);
    }

    IEnumerator WaitForNextTrigger(float wait)
    {
        yield return new WaitForSeconds(wait);
        triggers[triggerIndex].TriggerDialogue();
    }

    void NextTriggerFromCutscene(PlayableDirector director)
    {
        triggers[triggerIndex].TriggerDialogue();
    }
    void NextSceneFromCutscene(PlayableDirector director)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void NextBattleFromCutscene(PlayableDirector director)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void EndDialogue()
    {
        if (triggerIndex + 1 >= triggers.Length)
        {
            if (director.playOnAwake)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            else
            {
                director.Play();
                director.stopped += NextSceneFromCutscene;
            }
        }
        else if(triggerIndex + 1 < triggers.Length && !director.playOnAwake)
        {
            director.Play();
            triggerIndex++;
            director.playOnAwake = true;
            isDialogueActive = false;
            dialogueBox.SetActive(false);
            director.stopped += NextTriggerFromCutscene;
            return;
        }
            triggerIndex++;
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        StartCoroutine(WaitForNextTrigger(3f));
    }
}
