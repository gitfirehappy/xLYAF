local gameobject = CS.UnityEngine.GameObject.Find("TestGameObject")
local transform = gameobject.transform
transform.position = CS.UnityEngine.Vector3(0,2,0)
CS.UnityEngine.Debug.Log("Move TestGameObject to new position via Lua!")