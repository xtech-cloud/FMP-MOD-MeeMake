using UnityEngine;
using UnityEngine.UI;

public class PictureAnimation : MonoBehaviour
{
    public Sprite[] sprites{
        get{
            return sprites_;
        }
        set{
            sprites_ = value;
            timer = 0;
            index = 0;
            image.sprite = sprites[0];
        }
    }

    public float interval = 1;

    public Image image;
    private float timer = 0;
    private int index = -1;
    private Sprite[] sprites_ = new Sprite[0];

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer < interval)
            return;

        timer = 0;
        index += 1;
        if(index >= sprites.Length)
            index = 0;
        image.sprite = sprites[index];
    }
}
