namespace FirstDecoratorSample;
// Decorator #1 (should run FIRST because it is added first)
public class PrefixDecorator(IGreeter inner) : IGreeter
{
    string IGreeter.Greet(string name)
    {
        Console.WriteLine("Prefix Used");
        return "[PrefixDecorator] " + inner.Greet(name);
    }
}