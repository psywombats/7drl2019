-- global defines for cutscenes

function teleportCoords(mapName, x, y)
    cs_teleportCoords(mapName, x, y)
    await()
end

function teleport(mapName, eventName)
    cs_teleport(mapName, eventName)
    await()
end

function fadeOutBGM(seconds)
    cs_fadeOutBGM(seconds)
    await()
end

function speak(speaker, line)
    cs_speak(speaker, line)
    await()
end

function speak2(speaker, faceNo, line)
    cs_speak2(speaker, faceNo, line)
    await()
end

function nextMap()
    cs_nextMap()
    await()
end
