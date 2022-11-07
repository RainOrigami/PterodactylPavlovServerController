using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Test
{
    public class TestLoadAction { }

    public class TestSetValueAction
    {
        public TestSetValueAction(PterodactylServerModel[] value)
        {
            this.Value = value;
        }

        public PterodactylServerModel[] Value { get; }
    }
}
