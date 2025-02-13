# Adding chat history

## Default Kernel

When running the following from Program.cs:

```csharp
await KernelWithoutHistory(kernel);
```

Try the following prompts. As you can see - the model has no memory. It forgets my name in the very next prompt after I introduce my self:


```43
Hi, I'm an AI, how can I help you ?
-----------------------------------
My name is Ameer
Hi Ameer! It's nice to meet you. Is there something I can help you with or would you like to chat?


Hi, I'm an AI, how can I help you ?
-----------------------------------
What's my name ?
I don't know your name. This is the beginning of our conversation, and I don't have any information about you. Would you like to tell me your name?
```

## Kernel with chat history
