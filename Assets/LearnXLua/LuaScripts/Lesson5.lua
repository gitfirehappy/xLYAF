--引用C#命名空间
local Person = CS.Person_Lesson5
local Point = CS.Point
local Lesson5 = CS.Lesson5

--定义接口实现
local PersonActionImpl = {
    OnMove = function(self, pos)
        print(string.format("Lua Person moved to %s",pos:ToString()))
    end
}

function Main()
    --1. 创建Person对象(引用类型)
    local p = Person("Alan",25,Point(0,0))
    p:SayHello()

    --2. 调用属性的Get/Set方法
    print("Lua Age before:",p:GetAge())
    p:SetAge(26)
    print("Lua Age after:",p:GetAge(p))

    --3. 值类型交互
    p.Position = Point(10,20)
    p:SayHello()

    --4. 委托绑定
    local cb = function(msg)
        print("Lua Callback received:",msg)
    end
    cb("Test message from Lua")

    --5. 接口绑定
    local action = PersonActionImpl
    action:OnMove(Point(5,5))

    --6. 对象池应用
    local tbl = Lesson5.GetLuaTableFromPool() or {}
    tbl.temp = "Some data"
    print("Lua Using table from pool:", tbl.temp)
    Lesson5.ReturnLuaTableToPool(tbl)

    --7. 热更新
    xlua.hotfix(CS.Person_Lesson5,"SayHello",function(self)
        print(string.format("[Hotfix] Hi! I'm %s, age %d, position %s (Lua patched)", 
                self.Name, self.Age, self.Position:ToString()))
        return nil
    end)

    print("Lua After hotfix:")
    p:SayHello()
    
end