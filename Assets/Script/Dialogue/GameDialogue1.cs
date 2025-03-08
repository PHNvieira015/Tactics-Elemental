using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameDialogueCopy : MonoBehaviour
{
    public TextMeshProUGUI textComponentUP;
    public TextMeshProUGUI nameComponentUP;
    public TextMeshProUGUI textComponentDOWN;
    public TextMeshProUGUI nameComponentDOWN;
    public string[] names;
    [TextArea(2, 5)]
    public string[] lines;
    public float textSpeed;

    public int index;
    public int dialogueIndex;
    // Start is called before the first frame update
    void Start()
    {
        textComponentUP.text = string.Empty;
        textComponentDOWN.text = string.Empty;
        StartDialogue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (dialogueIndex == 0)
            {
                if (textComponentDOWN.text == lines[index])
                {
                    if(nameComponentDOWN.text == names[index + 1])
                    {
                        NextLine(textComponentDOWN, nameComponentDOWN);
                        return;
                    }
                    NextLine(textComponentUP, nameComponentUP);
                }
                else
                {
                    StopAllCoroutines();
                    textComponentDOWN.text = lines[index];
                }
            }
            else
            {
                if (textComponentUP.text == lines[index])
                {
                    if (nameComponentUP.text == names[index + 1])
                    {
                        NextLine(textComponentUP, nameComponentUP);
                        return;
                    }
                    NextLine(textComponentDOWN, nameComponentDOWN);
                }
                else
                {
                    StopAllCoroutines();
                    textComponentUP.text = lines[index];
                }
            }
            dialogueIndex = index % 2;
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine(textComponentUP, nameComponentUP));
    }
    
    IEnumerator TypeLine(TextMeshProUGUI textComponent, TextMeshProUGUI nameComponent)
    {
        nameComponent.text = names[index];
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine(TextMeshProUGUI textComponent, TextMeshProUGUI nameComponent)
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine(textComponent, nameComponent));
        }
        else
        {
            gameObject.SetActive(false);
            nameComponent.gameObject.SetActive(false);
        }
    }
}