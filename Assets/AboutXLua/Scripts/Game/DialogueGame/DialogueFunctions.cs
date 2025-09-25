using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFunctions : IDialogueFuncProvider
{
    [DialogueFunc("Check")]
    public static int Check(int a)
    {
        int rightNextID = 15;
        int wrongNextID = 16;
        
        if (a > 0)
        {
            return rightNextID;
        }

        return wrongNextID;
    }
}
