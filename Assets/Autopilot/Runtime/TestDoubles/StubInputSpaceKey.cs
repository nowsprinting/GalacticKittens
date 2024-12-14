using TestHelper.Input;
using UnityEngine;

namespace Autopilot.TestDoubles
{
    public class StubInputSpaceKey : InputWrapper
    {
        public int Interval = 20;
        private int _frameCount;

        public override bool GetKeyDown(KeyCode key)
        {
            if (key == KeyCode.Space)
            {
                if (_frameCount-- == 0)
                {
                    _frameCount = Interval;
                    return true;
                }
            }

            return false;
        }
    }
}