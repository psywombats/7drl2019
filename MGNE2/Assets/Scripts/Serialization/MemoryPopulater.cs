using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MemoryPopulater {

    // store any relevant fields into an object to be serialized, called on save
    void PopulateMemory(Memory memory);

    // load out those relevant fields, called on load
    void PopulateFromMemory(Memory memory);

}
