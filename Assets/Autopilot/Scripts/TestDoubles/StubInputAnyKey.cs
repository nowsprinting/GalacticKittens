using TestHelper.Input;

namespace Autopilot.Scripts.TestDoubles
{
    public class StubInputAnyKey : InputWrapper
    {
        public override bool anyKey => true;
    }
}