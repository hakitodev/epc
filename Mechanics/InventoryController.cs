using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weapon {
    public string Name;
    public bool IsGoted;
    public GameObject WeaponObj;
    public GameObject WeaponPanel;
}

public class InventoryController : MonoBehaviour {
    [SerializeField] private int index = 0;
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    private Weapon currentWeapon;

    private void Start() {
        if (weapons is not { Count: > 0 }) return;
        if (weapons[index].IsGoted) ActivateWeapon(index);
        else SelectFirstAvailable();
    }

    public void SwitchWeapon(int newIndex) {
        if (newIndex < 0 || newIndex >= weapons.Count || newIndex == index) return;
        if (!weapons[newIndex].IsGoted) {
            Debug.Log($"{weapons[newIndex].Name} didn't goted!");
            return;
        }
        
        ActivateWeapon(newIndex);
    }

    private void ActivateWeapon(int newIndex) {
        if (currentWeapon != null) {
            if (currentWeapon.WeaponObj != null) currentWeapon.WeaponObj.SetActive(false);
            if (currentWeapon.WeaponPanel != null) currentWeapon.WeaponPanel.SetActive(false);
        }

        var selected = weapons[newIndex];
        if (selected.WeaponObj != null) selected.WeaponObj.SetActive(true);
        if (selected.WeaponPanel != null) selected.WeaponPanel.SetActive(true);
        
        currentWeapon = selected;
        index = newIndex;
    }

    private void SelectFirstAvailable() {
        for (int i = 0; i < weapons.Count; i++) {
            if (weapons[i].IsGoted) {
                ActivateWeapon(i);
                break;
            }
        }
    }
}
