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
