namespace PterodactylPavlovServerController.Exceptions;

public class StartupVariableNotFoundException : Exception
{
    public StartupVariableNotFoundException(string envVariable)
    {
        this.EnvVariable = envVariable;
    }

    public string EnvVariable { get; }
}
