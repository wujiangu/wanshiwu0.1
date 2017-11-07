using System;
using UnityEngine;

public interface IConsoleView
{
    void Awake();
    void OnConsole(int InWindowID);
    void OnDestory();
    void OnDisable();
    void OnEnable();
    void OnEnter();
    void OnToggleVisible(bool bVisible);
    Rect SelectWindowRect();

    IConsoleLogger logger { get; }
}

