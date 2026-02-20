using System.Text;
using UnityEngine;

namespace EPC {
    public class parser : MonoBehaviour {
        [SerializeField] private int key = -1;
        
        public string encode_text(string txt, int k) {
            if (string.IsNullOrEmpty(txt)) return string.Empty;
            
            int code = key != -1 ? key : (k == 0 || k == 1 ? 1 : k);
            StringBuilder sb = new StringBuilder(txt.Length);
            
            for (int i = 0; i < txt.Length; i++) {
                sb.Append((char)(txt[i] ^ code));
            }
            
            return sb.ToString();
        }
        
        public string decode_text(string txt, int k) {
            if (string.IsNullOrEmpty(txt)) return string.Empty;
            
            int code = key != -1 ? key : (k == 0 || k == 1 ? 1 : k);
            StringBuilder sb = new StringBuilder(txt.Length);
            
            for (int i = 0; i < txt.Length; i++) {
                sb.Append((char)(txt[i] ^ code));
            }
            
            return sb.ToString();
        }
    }
}
