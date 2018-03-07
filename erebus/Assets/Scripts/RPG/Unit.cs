using System.Collections;
using System.Collections.Generic;

public class Unit {

    public Alignment Align { get; private set; }

    public Unit(Alignment align) {
        this.Align = align;
    }
}
