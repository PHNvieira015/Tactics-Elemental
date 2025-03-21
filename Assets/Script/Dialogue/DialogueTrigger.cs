using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public string name;
}

[System.Serializable]
public class Message
{
    public int characterID;
    [TextArea(3, 10)]
    public string message;
    public GameObject moveCharacter;
    public Vector2 startPosition;
    public Vector2 endPosition;
    public float waitTime;
}

[System.Serializable]
public class Dialogues
{
    public List<Message> messages = new List<Message>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Character[] characters;
    public Dialogues dialogue;
    public int TriggerOrder;

    private void Start()
    {
        foreach (Message message in dialogue.messages)
        {
            message.message.Replace("{ProtagonistName}", DialogueManager.ProtagonistName);
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(dialogue, characters);
    }
}