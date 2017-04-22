using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SystemMemory {
    
    public int totalPlaySeconds;
    public int lastSlotSaved;

    public SystemMemory() {
        lastSlotSaved = -1;
    }
}
