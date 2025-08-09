using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Lesson1_Hello : MonoBehaviour
{
    public LuaInit luaInit;
    
    void Start()
    {
        luaInit.luaEnv.DoString("require 'Lesson1_Hello'");    
    }
}
