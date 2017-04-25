-- global defines for coroutines mostly

function await ()
    coroutine.yield()
end

function wait (seconds)
    cs_wait(seconds)
    await()
end

function speak (line)
    cs_showText(line)
    await()
end

function speakLine (line)
    speak(line)
    cs_hideTextbox()
    await()
end

function teleport (mapName, x, y)
    cs_teleport(mapName, x, y)
    await()
end

function pathTo(event, eventName)
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
