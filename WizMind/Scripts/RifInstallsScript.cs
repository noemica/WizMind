using System;
using System.Collections.Generic;
using System.Text;

namespace WizMind.Scripts
{
    public class RifInstallsScript : IScript
    {
        private ScriptWorkspace ws = null!;

        public void Initialize(ScriptWorkspace ws)
        {
            this.ws = ws;
        }

        public void Run()
        {
            while (true)
            {
                // TODO
            }
        }
    }
}
