-- global defines for coroutines mostly

function await ()
    coroutine.yield()
end

function wait(seconds)
    cs_wait(seconds)
    await()
end

function speak(name, line)
    if (line == nil) then
        showFace(nil)
        cs_showText(name)
    else
        showFace(name)
        cs_showText(line)
    end

    await()
end

function speakLine(name, line)
    speak(name, line)
    cs_hideTextbox()
    await()
end

function teleportCoords(mapName, x, y)
    cs_teleportCoords(mapName, x, y)
    await()
end

function teleport(mapName, eventName)
    cs_teleport(mapName, eventName)
    await()
end

function pathEventTo(event, eventName)
    local target = eventNamed(eventName)
    event.cs_pathTo(target)
    await()
end

function pathTo(eventName)
    local target = eventNamed(eventName)
    avatar.cs_pathTo(target.x(), target.y())
end

function step(event, dir)
    event.cs_step(dir)
    await()
end

function walk(event, dir, count)
    event.cs_walk(dir, count)
    await()
end

function fadeOutBGM(seconds)
    cs_fadeOutBGM(seconds)
    await()
end
