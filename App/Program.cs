using System;
using System.Diagnostics;
using System.Text.Json;
using Lib;

namespace App;

internal class Program
{
    static void Main()
    {
        // Setup
        var context = new JsonSerializerContextConfig()
            .WithOptions(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .AddContext<SomeJsonContext>()
            .AddContext<SomeOtherJsonContext>()
            .WithDataObject<ISomeData, SomeData>(m =>
            {
                m.SetPropertyName(p => p.Text, "__text");
            })
            .WithDataObject<ISomeOtherData, SomeOtherData>()
            .Build();

        // Examples
        var model = JsonSerializer.Deserialize("""
            { "__text": "Hello World!", "number": 42 }
            """, context.GetTypeInfo<ISomeData>()!);
        Console.WriteLine(model);

        var reverse = JsonSerializer.Serialize(new SomeData(69, "HHH"), context.GetTypeInfo<ISomeData>()!);
        Console.WriteLine(reverse);

        var withOptional = JsonSerializer.Deserialize("""
            { "__text": "hihi", "number": 111, "next": { "__text": "Hello World!", "number": 42 } }
            """, context.GetTypeInfo<ISomeData>()!);
        Console.WriteLine(withOptional);

        var reverseWithOptional = JsonSerializer.Serialize(new SomeData(72, "HHH", new(new SomeData(111, "11111"))), context.GetTypeInfo<ISomeData>()!);
        Console.WriteLine(reverseWithOptional);

        // Missing properties
        try
        {
            _ = JsonSerializer.Deserialize("""{}""", context.GetTypeInfo<ISomeData>()!);
            Debug.Fail("Serialization should fail.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Unknown properties
        try
        {
            _ = JsonSerializer.Deserialize("""{ "wrong": 0 }""", context.GetTypeInfo<ISomeData>()!);
            Debug.Fail("Serialization should fail.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
