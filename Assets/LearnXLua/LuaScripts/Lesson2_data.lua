num = 10
str = "string from lua"
isActive = true

nums = {1,2,3,4,5}

scores = {Alice = 90, Bob = 85, Charlie = 92}

function multiply(x, y)
    return x * y
end 

function call_csharp(action)
    action("Message from Lua")
end

function move_object(gameobject)
    local pos = gameobject.transform.position
    pos.x = pos.x + 1
    gameobject.transform.position = pos
end