using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Modules.Game.Role
{
    public class AlwaysFaceCamera:MonoBehaviour
    {
        private void Update()
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
