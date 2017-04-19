-- global defines for coroutines mostly

function runRoutine (routine)
    debugLog('runRoutine')
    local active = coroutine.running()
    for dummy in routine do
        coroutine.yield(active)
    end
end

function speak (line)
    debugLog('speak')
    runRoutine(cs_speak(line))
end

function speakLine (line)
    debugLog('speakLine')
    runRoutine(cs_speak(line))
    debugLog('speakLine2')
    runRoutine(cs_hideText())
end

function wait (seconds)
    runRoutine(cs_wait(seconds))
end
