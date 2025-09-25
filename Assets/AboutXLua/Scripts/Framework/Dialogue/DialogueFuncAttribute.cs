using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method,Inherited = false,AllowMultiple = false)]
public class DialogueFuncAttribute : Attribute
{
    public string DisplayName;
    
    public DialogueFuncAttribute(string name)
    {
        DisplayName = name;
    }
}
