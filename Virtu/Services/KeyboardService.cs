﻿namespace Jellyfish.Virtu.Services
{
    public abstract class KeyboardService : MachineService
    {
        protected KeyboardService(Machine machine) : 
            base(machine)
        {
        }

        public abstract bool IsKeyDown(int key);

        public virtual void Update() // main thread
        {
            if (IsResetKeyDown)
            {
                if (!_resetKeyDown)
                {
                    _resetKeyDown = true; // entering reset; pause until key released
                    Machine.Pause();
                    Machine.Reset();
                }
            }
            else if (_resetKeyDown)
            {
                _resetKeyDown = false; // leaving reset
                Machine.Unpause();
            }
        }

        public bool IsAnyKeyDown { get; protected set; }
        public bool IsOpenAppleKeyDown { get; protected set; }
        public bool IsCloseAppleKeyDown { get; protected set; }

        protected bool IsResetKeyDown { get; set; }

        private bool _resetKeyDown;
    }
}
