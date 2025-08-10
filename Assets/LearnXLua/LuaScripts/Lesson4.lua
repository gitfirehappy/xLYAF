--委托绑定进阶
function on_event_a()
    print("Lua: Event A triggered")
end

function on_event_b(msg)
    print("Lua: Event B triggered with message:", msg)
end

function on_event_chain(msg1)
    print("Lua: Event C triggered with messages:", msg1)
end

--Lua -- C#类与结构体交互
--读取和修改C#类
function use_person(person)
    print("Lua: Person name:", person.Name, "age:", person.Age)
    person.Name = "ChangeFromLua"
    person.Age = person.Age + 10
end

--返回一个C#结构体
function create_vector3()
    return CS.UnityEngine.Vector3(1,2,3)
end

--Lua协程,协程控制逻辑完全放在Lua端
local coroutine_running = false
local current_co = nil

function start_coroutine()
    if coroutine_running then return nil end

    coroutine_running = true
    current_co = coroutine.create(function()
        CS.UnityEngine.Debug.Log("Lua Coroutine: Step 1")
        coroutine.yield()

        CS.UnityEngine.Debug.Log("Lua Coroutine: Step 2 after 1 second")
        coroutine.yield()

        CS.UnityEngine.Debug.Log("Lua Coroutine: Finished")
        coroutine_running = false
    end)

    -- 首次执行
    coroutine.resume(current_co)
    return true  -- 只返回成功标志，不返回协程对象
end

function resume_coroutine()
    if current_co and coroutine.status(current_co) == "suspended" then
        return coroutine.resume(current_co)
    end
    return false
end


--性能优化
local log = CS.UnityEngine.Debug.Log
function fast_long_test(count)
    for i = 1,count do
        log("Log from Lua: " .. i)
    end
end