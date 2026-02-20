using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class main_settings : MonoBehaviour
{
    [System.Serializable]
    public struct set {
        public string name;
        public bool can_change;
        public bool active;
    }
    public List<set> settings_com = new List<set>();
    
    public void addSetting(string str, string rank) {
        for (int i = 0; i < settings_com.Count; i++) {
            if (settings_com[i].name == str && settings_com[i].can_change) {
                var dash = settings_com[i];
                dash.active = !dash.active;
                settings_com[i] = dash;
                return;
            }
        }
    }
    
    public void rendSettting(TMP_Text txt, string str) {
        for (int i = 0; i < settings_com.Count; i++) {
            if (settings_com[i].name == str && settings_com[i].can_change) {
                var dash = settings_com[i];
                dash.active = !dash.active;
                settings_com[i] = dash;
                
                if (txt != null) {
                    txt.text = dash.active ? "On" : "Off";
                }
                return;
            }
        }
    }
}

