﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Jellyfish.Library;

namespace Jellyfish.Virtu.Services
{
    public abstract class AudioService : MachineService
    {
        protected AudioService(Machine machine) : 
            base(machine)
        {
        }

        public void Output(int data) // machine thread
        {
            _buffer[_index + 0] = (byte)(data & 0xFF);
            _buffer[_index + 1] = (byte)(data >> 8);
            _index = (_index + 2) % SampleSize;
            if (_index == 0)
            {
                _readEvent.Set();
                if (Machine.Settings.Cpu.IsThrottled)
                {
                    _writeEvent.WaitOne(SampleLatency * 2); // allow timeout; avoids deadlock
                }
            }
        }

        public void Reset()
        {
            Buffer.BlockCopy(SampleZero, 0, _buffer, 0, SampleSize);
        }

        public abstract void SetVolume(double volume); // machine thread

        protected void Update(int bufferSize, Action<byte[], int> updateBuffer) // audio thread
        {
            if (Machine.State == MachineState.Running)
            {
                _readEvent.WaitOne(SampleLatency * 2); // allow timeout; avoids deadlock
            }
            if (updateBuffer != null)
            {
                updateBuffer(_buffer, bufferSize);
            }
            _writeEvent.Set();
        }

        public const int SampleRate = 44100; // hz
        public const int SampleChannels = 1;
        public const int SampleBits = 16;
        public const int SampleLatency = 40; // ms
        public const int SampleSize = (SampleRate * SampleLatency / 1000) * SampleChannels * (SampleBits / 8);

        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        protected static readonly byte[] SampleZero = new byte[SampleSize];

        private byte[] _buffer = new byte[SampleSize];
        private int _index;

        private AutoResetEvent _readEvent = new AutoResetEvent(false);
        private AutoResetEvent _writeEvent = new AutoResetEvent(false);
    }
}
