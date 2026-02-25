using BasicDependencyInjectionLibrary.Decorators;
using FirstDecoratorSample;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Pretend defaults already ran:
services.AddSingleton<IGreeter, StandardGreeter>()
    .AddSingleton<MainService>();

// Mode-specific composition: force base + add decorators in readable order.
// IMPORTANT: With this library, FIRST added runs FIRST.
services.ReplaceSingleton<IGreeter, StandardGreeter>();
services.Decorate<IGreeter, PrefixDecorator>();   // runs 1st
services.Decorate<IGreeter, ExcitedDecorator>();  // runs 2nd
services.Decorate<IGreeter, SuffixDecorator>();   // runs 3rd

using var sp = services.BuildServiceProvider();


var main = sp.GetRequiredService<MainService>();
main.RunTest();
Console.WriteLine("Good Job If Everything worked");

//var greeter = sp.GetRequiredService<IGreeter>();

//Console.WriteLine(greeter.Greet("Andy"));
