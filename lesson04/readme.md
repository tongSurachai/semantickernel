# Command-line chat app with multiple plugins


## Command line chat app

To emulate a command-line chat app, we simply read input from the command-line in an infinite loop. On every input - we pass the input to the kernel and output the response:

```csharp
while (true)
{
    Console.WriteLine();
    Console.WriteLine("How can I help ? Enter a prompt: ");
    Console.WriteLine("--------------------------------");

    // Read input from console
    var prompt = Console.ReadLine();

    // Send the prompt to the kernel if it's not null
    if (!string.IsNullOrEmpty(prompt))
    {
        var response = await kernel.InvokePromptAsync(prompt, new(settings));
        // Print the response
        Console.WriteLine(response);
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("Prompt cannot be empty. Please enter a valid prompt.");
    }
}
```

## Registering multiple plugins

The following code registers our plugins [program.cs](./Program.cs):

```csharp
// Add function by type
builder.Plugins.AddFromType<DatePlugin>();
builder.Plugins.AddFromType<MangoPlugin>();
```

## Sample Output

### Testing mango plugin:

```
❯ dotnet run

How can I help ? Enter a prompt:
--------------------------------
how much for 2 kilos of mangoes
The current price for 2 kilograms of mangoes is ₹ 8.
```

### Testing Date plugin

```
How can I help ? Enter a prompt:
--------------------------------
what is the date today ?
The current date is February 3, 2025.
```

### Testing base LLM llama3.2 model

```
How can I help ? Enter a prompt:
--------------------------------
Tell me a story
Here's a story for you:

Once upon a time, in a small village surrounded by lush green forests and vibrant orange groves, there lived a young girl named Lily. She was known throughout the village for her extraordinary talent - she could make anyone smile with just one glance of her bright blue eyes.
```

