using UnityEngine;
using System.Collections;
using cs;

public class TestGui : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        GuiScene guiScene = GuiManager.Get().CreateGuiScene("TestScene");
        guiScene.LoadGuiObject("");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
