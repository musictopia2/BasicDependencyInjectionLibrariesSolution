namespace FirstDecoratorSample;
public class StandardGreeter : IGreeter
{
    string IGreeter.Greet(string name)
    {
        Console.WriteLine("Standard Used");
        return $"Hello, {name}.";
    }
}