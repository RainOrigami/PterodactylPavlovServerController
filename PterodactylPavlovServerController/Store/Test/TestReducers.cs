using Fluxor;

namespace PterodactylPavlovServerController.Store.Test
{
    public class TestReducers
    {
        [ReducerMethod]
        public static TestState OnTestLoad(TestState testState, TestSetValueAction testSetValueAction)
        {
            return testState with
            {
                Value = testSetValueAction.Value
            };
        }
    }
}
