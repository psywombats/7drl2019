using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Vaults", menuName = "Data/RPG/Vault List")]
public class VaultList : ScriptableObject {

    public List<TacticsTerrainMesh> vaults;
}
