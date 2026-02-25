namespace FirstDecoratorSample;
// Decorator #3 (runs third)
public class SuffixDecorator(IGreeter inner) : IGreeter
{
    string IGreeter.Greet(string name)
    {
        Console.WriteLine("Suffix Used");
        return "[SuffixDecorator] " + inner.Greet(name) + " [end]";
    }
}