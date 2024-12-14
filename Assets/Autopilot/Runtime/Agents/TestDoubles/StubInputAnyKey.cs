using TestHelper.Input;

namespace Autopilot.Agents.TestDoubles
{
    public class StubInputAnyKey : InputWrapper
    {
        public override bool anyKey => true;
    }
}