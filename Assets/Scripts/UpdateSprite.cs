using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    public Sprite dominoFace;
    public Sprite dominoBack;
    private SpriteRenderer spriteRenderer;
    private Selectable selectable;
    private Domino42 domino42;
    private UserInput userInput;

    // Start is called before the first frame update
    void Start()
    {
        List<string> deck = Domino42.GenerateDeck();
        domino42 = FindObjectOfType<Domino42>();
        userInput = FindObjectOfType<UserInput>();

        int i = 0;
        foreach(string domino in deck)
        {
            if (this.name == domino)
            {
                dominoFace = domino42.dominoFaces[i];
                break;
            }
            i++;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectable = GetComponent<Selectable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectable.faceUp == true)
        {
            spriteRenderer.sprite = dominoFace;
        }
        else
        {
            spriteRenderer.sprite = dominoBack;
        }

        if (name == userInput.prevObjectClicked.name)
        {
            spriteRenderer.color = Color.yellow;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}
