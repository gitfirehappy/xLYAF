using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

[LuaCallCSharp]
public class LuaUIEventBridge : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IScrollHandler
{
    public LuaTable luaTable; // Lua对象（需设置）

    private void CallLua(string methodName, BaseEventData data)
    {
        if (luaTable == null) return;
        var func = luaTable.Get<LuaFunction>(methodName);
        if (func != null)
        {
            func.Call(luaTable, data); // self + 参数
        }
    }

    public void OnPointerClick(PointerEventData eventData) => CallLua("OnPointerClick", eventData);
    public void OnPointerDown(PointerEventData eventData) => CallLua("OnPointerDown", eventData);
    public void OnPointerUp(PointerEventData eventData) => CallLua("OnPointerUp", eventData);
    public void OnPointerEnter(PointerEventData eventData) => CallLua("OnPointerEnter", eventData);
    public void OnPointerExit(PointerEventData eventData) => CallLua("OnPointerExit", eventData);
    public void OnBeginDrag(PointerEventData eventData) => CallLua("OnBeginDrag", eventData);
    public void OnDrag(PointerEventData eventData) => CallLua("OnDrag", eventData);
    public void OnEndDrag(PointerEventData eventData) => CallLua("OnEndDrag", eventData);
    public void OnScroll(PointerEventData eventData) => CallLua("OnScroll", eventData);
}