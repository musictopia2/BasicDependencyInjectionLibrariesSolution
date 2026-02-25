namespace FirstDecoratorSample;
// Decorator #2 (runs second)
public class ExcitedDecorator(IGreeter inner) : IGreeter
{
    string IGreeter.Greet(string name)
    {
        Console.WriteLine("Excited Used");
        return "[ExcitedDecorator] " + inner.Greet(name) + " !!!";
    }
}