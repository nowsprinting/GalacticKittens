using TestHelper.Input;

namespace Autopilot.TestDoubles
{
    public class StubInputAnyKey : InputWrapper
    {
        public override bool anyKey => true;
    }
}