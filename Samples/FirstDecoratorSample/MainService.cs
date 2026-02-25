namespace FirstDecoratorSample;
public class MainService(IGreeter greeter)
{
    public void RunTest()
    {
        Console.WriteLine("Part 1");
        string details = greeter.Greet("Andy");
        Console.WriteLine(details);
        Console.WriteLine("This is the end of this pretend service");
    }
}