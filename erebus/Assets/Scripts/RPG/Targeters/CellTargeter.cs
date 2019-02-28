using System.Collections;
using UnityEngine;

public class CellTargeter : Targeter {

    private CellTargeterParams data;
    
    public class CellTargeterParams : TargeterParams {
        [Tooltip("Range in tiles, -1 for unlimited")]
        public int range = 0;

        [Space]
        public bool emptyTilesAllowed = false;
        public bool enemyUnitsAllowed = true;
        public bool friendlyUnitsAllowed = false;
        public bool selfSelectAllowed = false;

        public override Targeter Instantiate() {
            return new CellTargeter(this);
        }
    }

    public CellTargeter(CellTargeterParams data) {
        this.data = data;
    }

    public override IEnumerator AcquireTargets(Result<bool> result) {
        throw new System.NotImplementedException();
    }
}
