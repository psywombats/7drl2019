using System.Collections;
using System.Collections.Generic;

public class Unit {

    public Alignment Align { get; private set; }
    public StatSet Stats { get; private set; }

    public Unit(Alignment align) {
        this.Align = align;
        // TODO: shouldn't be able to init with zero stats
        this.Stats = new StatSet();
    }
}
