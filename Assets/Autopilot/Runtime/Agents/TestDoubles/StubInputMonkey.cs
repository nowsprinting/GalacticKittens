using DeNA.Anjin.Utilities;
using TestHelper.Input;

namespace Autopilot.Agents.TestDoubles
{
    public class StubInputMonkey : InputWrapper
    {
        private readonly IRandom _random;
        private float _lastVerticalAxis;
        private int _frameCount;

        public StubInputMonkey(IRandom random)
        {
            _random = random;
        }

        public override float GetAxis(string axisName)
        {
            if (axisName != "Vertical")
            {
                return 0f;
            }

            if (_frameCount-- == 0)
            {
                // (re) lottery
                _lastVerticalAxis = (float)_random.Next(-10, 10) / 10;
                _frameCount = _random.Next(10, 30);
            }

            return _lastVerticalAxis;
        }
    }
}