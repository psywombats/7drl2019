-- global defines for coroutines mostly

function await ()
    coroutine.yield()
end

function speakLine (line)
    speak(line)
    hideTextbox()
end
