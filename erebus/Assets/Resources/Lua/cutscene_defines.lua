-- global defines for cutscenes

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

function speakLine(line)
    cs_speakLine(line)
    await()
end

function playScene(sceneName)
    cs_playSceen(sceneName)
    await()
end

function fadeIn()
    cs_fadeIn()
    await()
end

function fadeOut()
    cs_fadeOut()
    await()
end
