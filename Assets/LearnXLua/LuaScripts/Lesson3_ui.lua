--改UI文本
function set_text(txtComponent,newTest)
    txtComponent.text = newTest
end

--Button点击
function on_button_click(txtComponent)
    txtComponent.text = "Button Clicked from Lua"
    print("Lua: Button Clicked")
end 

--改Image
function change_image(img,spriteName)
    local sprite = CS.UnityEngine.Resources.Load(spriteName,typeof(CS.UnityEngine.Sprite))
    if sprite ~= nil then
        img.sprite = sprite
        img.color = CS.UnityEngine.Color(1,0.5,0.5)
    else
        print("Lua: Failed to load sprite:", spriteName)
    end
end

-- 更新倒计时
function update_timer(txt, seconds)
    txt.text = "Lated: " .. seconds .. "seconds"
end