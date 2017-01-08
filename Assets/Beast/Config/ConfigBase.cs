

namespace BeastConsole
{
    using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
    public class ConfigBase : MonoBehaviour
    {

        public virtual void OnEnable()
        {
            ConfigSystem.RegisterConfig(this);
        }

        public virtual void OnDisable()
        {
            ConfigSystem.UnregisterConfig(this);
        }
    }
}