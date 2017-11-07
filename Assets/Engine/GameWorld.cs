using UnityEngine;
using System.Collections;

namespace cs
{
    public class GameWorld : MonoBehaviour
    {
        public virtual void Initialize()
        {
            AssetManager.Get();
            EventSystem.Get();

            GameStateManager.Get();
            LuaManager.Get();
            GuiManager.Get();
        }

        public virtual void Tick(float a_fElapsed)
        {
            GameStateManager.Get().Tick(a_fElapsed);
            AssetManager.Get().Tick(a_fElapsed);
            EventSystem.Get().Tick(a_fElapsed);
        }

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(this);

            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            Tick(Time.deltaTime);
        }
    }
}


