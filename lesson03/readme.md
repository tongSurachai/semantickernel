# Function Calling with Parameters

To try out this sample:

-  `ollama run llama3.2`
- Clone the repo
- `dotnet run` !


## Notes

This lesson recaps the same ideas on lesson02 with some additional metadata for the methods in the plugin being registered:

```csharp
builder.Plugins.AddFromType<MangoPlugin>();
```

**Note:**
- The `Description` attribute for the method mentions the price is in `USD`
- The `Description` attribute for the `numberOfKilos` parameters let's the LLM know that the weight is in kilos


```csharp
public class MangoPlugin
{
    [KernelFunction("GetMangoPrice")]
    [Description("Calculates the price of mangoes based on weight in kilograms. Returns price in USD.")]
    public double GetMangoPrice (
        [Description("The weight of mangoes in kilograms.")]
        double numberOfKilos
    ) 
    {
        var pricePerKilo = 4.0;
        return numberOfKilos * pricePerKilo;
    }
}
```

Therefore, our prompt:

```
How much for .5 kg of mangoes
```

Provides an accurate response in USD for the correct price based on weight:

```
The current price for 0.5 kg of mangoes is ? 2.
```
