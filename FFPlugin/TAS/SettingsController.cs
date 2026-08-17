using System.Collections.Generic;
using UnityEngine;

namespace FreezeFrame.TAS
{
    class SettingsController
    {
        private const string tasBindings = """
            {
              "controlScheme": "Keyboard & Mouse",
              "modifiedActions": {
                "Punch": [],
                "Punch (Feedbacker)": [
                  {
                    "path": "<Keyboard>/f"
                  }
                ],
                "Punch (Knuckleblaster)": [
                  {
                    "path": "<Keyboard>/v"
                  }
                ],
                "Primary Fire": [
                  {
                    "path": "<Keyboard>/comma"
                  }
                ],
                "Secondary Fire": [
                  {
                    "path": "<Keyboard>/period"
                  }
                ],
                "Next Variation": [],
                "Previous Variation": [],
                "Variation Slot 2": [
                  {
                    "path": "<Keyboard>/e"
                  }
                ],
                "Variation Slot 3": [
                  {
                    "path": "<Keyboard>/q"
                  }
                ]
              }
            }
            """;
        private string prevBindings;
        private Dictionary<string, object> tasPrefs;
        private Dictionary<string, object> tasLocalPrefs;
        private Dictionary<string, object> prevPrefs = new Dictionary<string, object>();
        private Dictionary<string, object> prevLocalPrefs = new Dictionary<string, object>();

        public SettingsController()
        {
            tasLocalPrefs = new Dictionary<string, object>
            {
                { "mouseSensitivity", 50f },
            };
            tasPrefs = new Dictionary<string, object>
            {
                { "variationMemory", false },
                { "WeaponRedrawBehaviour", 1 },
                { "weapon.rev.order", "1324" },
                { "weapon.sho.order", "1234" },
                { "weapon.nai.order", "1234" },
                { "weapon.rai.order", "1234" },
                { "weapon.rock.order", "1234" },
                { "InvertRocketRide", false }
            };
        }

        public void Apply()
        {
            prevBindings = MonoSingleton<InputManager>.Instance.InputSource.Actions.asset.ToJson();
            MonoSingleton<InputManager>.Instance.InputSource.Actions.asset.LoadFromJson(tasBindings);
            PrefsManager prefs = MonoSingleton<PrefsManager>.Instance;
            foreach (var (key, value) in tasPrefs)
            {
                if (value is bool)
                {
                    prevPrefs[key] = prefs.GetBool(key);
                    prefs.SetBool(key, (bool) value);
                }
                else if (value is int)
                {
                    prevPrefs[key] = prefs.GetInt(key);
                    prefs.SetInt(key, (int) value);
                }
                else if (value is float)
                {
                    prevPrefs[key] = prefs.GetFloat(key);
                    prefs.SetFloat(key, (float) value);
                }
                else if (value is string)
                {
                    prevPrefs[key] = prefs.GetString(key);
                    prefs.SetString(key, (string) value);
                }
            }
            foreach (var (key, value) in tasLocalPrefs)
            {
                if (value is bool)
                {
                    prevLocalPrefs[key] = prefs.GetBoolLocal(key);
                    prefs.SetBoolLocal(key, (bool)value);
                }
                else if (value is int)
                {
                    prevLocalPrefs[key] = prefs.GetIntLocal(key);
                    prefs.SetIntLocal(key, (int)value);
                }
                else if (value is float)
                {
                    prevLocalPrefs[key] = prefs.GetFloatLocal(key);
                    prefs.SetFloatLocal(key, (float)value);
                }
                else if (value is string)
                {
                    prevLocalPrefs[key] = prefs.GetStringLocal(key);
                    prefs.SetStringLocal(key, (string)value);
                }
            }
        }

        public void Revert()
        {
            MonoSingleton<InputManager>.Instance.InputSource.Actions.asset.LoadFromJson(prevBindings);
            PrefsManager prefs = MonoSingleton<PrefsManager>.Instance;
            foreach (var (key, value) in prevPrefs)
            {
                if (value is bool)
                {
                    prefs.SetBool(key, (bool)value);
                }
                else if (value is int)
                {
                    prefs.SetInt(key, (int)value);
                }
                else if (value is float)
                {
                    prefs.SetFloat(key, (float)value);
                }
                else if (value is string)
                {
                    prefs.SetString(key, (string)value);
                }
            }
            foreach (var (key, value) in prevLocalPrefs)
            {
                if (value is bool)
                {
                    prefs.SetBoolLocal(key, (bool)value);
                }
                else if (value is int)
                {
                    prefs.SetIntLocal(key, (int)value);
                }
                else if (value is float)
                {
                    prefs.SetFloatLocal(key, (float)value);
                }
                else if (value is string)
                {
                    prefs.SetStringLocal(key, (string)value);
                }
            }
            prevPrefs = [];
            prevLocalPrefs = [];
        }
    }
}
